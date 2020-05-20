using System;
using System.IO;
using NodaTime;

namespace UndergroundIRO.TimeZoneKit.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var searchStr = "Kiev";
            var tz = DateTimeZoneSearch.Inst.GetTimeZone(searchStr);
            PrintTimeZone(tz, searchStr);

            searchStr = "киЕв";
            tz = DateTimeZoneSearch.Inst.GetTimeZone(searchStr);
            PrintTimeZone(tz, searchStr);

            searchStr = "киЕВ";
            tz = DateTimeZoneSearch.Inst.GetTimeZoneFromTranslate(searchStr, TzTranslate.Russian);
            PrintTimeZone(tz, searchStr);

            searchStr = "Аризона";
            tz = DateTimeZoneSearch.Inst.GetTimeZone(searchStr);
            PrintTimeZone(tz, searchStr);

            searchStr = "+6";
            tz = DateTimeZoneSearch.Inst.GetTimeZone(searchStr);
            PrintTimeZone(tz, searchStr);

            searchStr = "  -3  ";
            tz = DateTimeZoneSearch.Inst.GetTimeZone(searchStr);
            PrintTimeZone(tz, searchStr);

            searchStr = "Київ  ";
            tz = DateTimeZoneSearch.Inst.GetTimeZone(searchStr);
            PrintTimeZone(tz, searchStr);

            Console.ReadLine();
        }

        static void PrintTimeZone(DateTimeZone tz, string searchStr)
        {
            var offsetMilliseconds = tz.GetUtcOffset(Instant.FromDateTimeUtc(DateTime.UtcNow)).Milliseconds;
            var offsetHours = offsetMilliseconds / 1000 / 60 / 60;
            Console.WriteLine($"Search string: {searchStr}");
            Console.WriteLine($"Id: {tz.Id}");
            Console.WriteLine($"Offset: {offsetHours} hours\n");
            Console.WriteLine("\n\n");
        }
    }
}
