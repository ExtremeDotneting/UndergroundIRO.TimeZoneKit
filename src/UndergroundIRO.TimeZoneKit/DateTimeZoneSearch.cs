using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using IRO.EmbeddedResources;
using NodaTime;

namespace UndergroundIRO.TimeZoneKit
{
    public class DateTimeZoneSearch
    {
        public static DateTimeZoneSearch Inst { get; } = new DateTimeZoneSearch();

        readonly string _translatesPath;

        readonly IList<string> _original;

        readonly IDictionary<string, IDictionary<string, int>> _translateByFile = new ConcurrentDictionary<string, IDictionary<string, int>>();

        readonly IList<IDictionary<string, int>> _translates = new List<IDictionary<string, int>>();

        DateTimeZoneSearch()
        {
            _translatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "timezone_cities");
            var assembly = Assembly.GetExecutingAssembly();
            assembly.ExtractEmbeddedResourcesDirectory("UndergroundIRO.TimeZoneKit.CityTranslates", _translatesPath);

            Directory.GetFiles(_translatesPath, "*.txt");

        }

        bool TryGetTimeZoneFromTranslate(IDictionary<string, int> translateDict, string location, out DateTimeZone tz)
        {
            if (translateDict.TryGetValue(location, out var index))
            {
                var fullTzId=_original[index];
                tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(fullTzId);
                return true;
            }
            else
            {
                tz = null;
                return false;
            }
        }

        public DateTimeZone GetTimeZone(string location)
        {
            var prefixes = new[]
            {
                "Europe",
                "Asia",
                "Africa",
                "America",
                "America/Argentina",
                "America/Indiana",
                "America/North_Dakota",
                "America/Kentucky",
                "Antarctica",
                "Arctic",
                "Atlantic",
                "Australia",
                "Brazil",
                "Canada",
                "Chile",
                "Indian",
                "Mexico",
                "Pacific",
                "US"
            };

            if (TryGetTimeZoneWithPrefix(location, out var tz))
            {
                return tz;
            }

            foreach (var prefix in prefixes)
            {
                if (TryGetTimeZoneWithPrefix($"{prefix}/{location}", out var tzLoc))
                {
                    return tzLoc;
                }
            }
            throw new Exception($"Can't find timezone for '{location}'");
        }

        static bool TryGetTimeZoneWithPrefix(string fullLocation, out DateTimeZone tz)
        {
            tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(fullLocation);
            return tz != null;
        }
    }
}
