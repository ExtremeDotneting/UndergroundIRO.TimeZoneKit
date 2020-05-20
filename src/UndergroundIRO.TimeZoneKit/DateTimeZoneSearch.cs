using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using IRO.EmbeddedResources;
using NodaTime;

namespace UndergroundIRO.TimeZoneKit
{
    public class DateTimeZoneSearch
    {
        public static DateTimeZoneSearch Inst { get; } = new DateTimeZoneSearch();

        readonly string[] _original;
        readonly IDictionary<string, IDictionary<string, int>> _translateByFile = new ConcurrentDictionary<string, IDictionary<string, int>>();
        readonly IList<IDictionary<string, int>> _translates;
        readonly IDictionary<string, DateTimeZone> _cache = new ConcurrentDictionary<string, DateTimeZone>();

        DateTimeZoneSearch()
        {
            var translatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "timezone_cities");
            var assembly = Assembly.GetExecutingAssembly();
            assembly.ExtractEmbeddedResourcesDirectory("UndergroundIRO.TimeZoneKit.CityTranslates", translatesPath);

            _original = File.ReadAllLines(Path.Combine(translatesPath, "original.txt"));

            var files = Directory.GetFiles(translatesPath, "*.txt");
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var translate = ReadTranslate(filePath);
                _translateByFile[fileName] = translate;
            }

            _translates = _translateByFile
                .Select(r => r.Value)
                .ToList();
        }

        public DateTimeZone GetTimeZone(string location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (_cache.TryGetValue(location, out var tzLoc))
            {
                return tzLoc;
            }

            var tz = GetTimeZoneWithoutCaching(location);
            _cache[location] = tz;
            return tz;
        }

        /// <summary>
        /// </summary>
        /// <param name="translateFile">Filename from ~/timezone_cities/ . Use <see cref="TzTranslate"/> for default translates.</param>
        /// <returns></returns>
        public DateTimeZone GetTimeZoneFromTranslate(string location, string translateFile)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            var editedLocation = EditLocationString(location);
            var translateDict = _translateByFile[translateFile];
            if (TryGetTimeZoneFromTranslate(translateDict, editedLocation, out var tz))
            {
                return tz;
            }
            throw new Exception($"Can't find timezone for location '{location}'.");
        }

        DateTimeZone GetTimeZoneWithoutCaching(string location)
        {
            //Try default.
            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(location);
            if (timeZone != null)
                return timeZone;

            var editedLocation = EditLocationString(location);

            //Try if num.
            if (int.TryParse(editedLocation, out var num))
            {
                var sign = num > 0 ? "+" : "";
                var gmtId = $"Etc/GMT{sign}{num}";
                var timeZoneByNum = DateTimeZoneProviders.Tzdb.GetZoneOrNull(gmtId);
                if (timeZoneByNum != null)
                    return timeZoneByNum;
            }

            foreach (var translateDict in _translates)
            {
                if (TryGetTimeZoneFromTranslate(translateDict, editedLocation, out var tz))
                {
                    return tz;
                }
            }
            throw new Exception($"Can't find timezone for location '{location}'.");
        }

        bool TryGetTimeZoneFromTranslate(IDictionary<string, int> translateDict, string location, out DateTimeZone tz)
        {
            if (translateDict.TryGetValue(location, out var index))
            {
                var fullTzId = _original[index];
                tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(fullTzId);
                return tz != null;
            }
            else
            {
                tz = null;
                return false;
            }
        }

        IDictionary<string, int> ReadTranslate(string filePath)
        {
            var dict = new ConcurrentDictionary<string, int>();
            var lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                var editedLocation= EditLocationString(lines[i]);
                dict[editedLocation] = i;
            }
            return dict;
        }

        string EditLocationString(string location)
        {
            var editedLocation = location
                .Trim()
                .Replace("  ", " ")
                .Replace("\n", "")
                .ToLower();
            return editedLocation;
        }
    }
}
