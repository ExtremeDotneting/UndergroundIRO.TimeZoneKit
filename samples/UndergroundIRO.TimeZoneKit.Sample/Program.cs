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
            var offsetMilliseconds = tz.GetUtcOffset(Instant.FromDateTimeUtc(DateTime.UtcNow)).Milliseconds;
            var offsetHours = offsetMilliseconds / 1000 / 60 / 60;
            Console.WriteLine($"Search string: {searchStr}");
            Console.WriteLine($"Id: {tz.Id}");
            Console.WriteLine($"Offset: {offsetHours} hours\n");

            Console.WriteLine("\n\n");
            var ids = File.ReadAllLines("timezone_cities/original.txt");
            var strToWrite = "";
            foreach (var id in ids)
            {
                var str = id;
                var index = str.IndexOf("/");
                if (index > 0)
                {
                    str = str.Substring(index + 1);
                }
                index = str.IndexOf("/");
                if (index > 0)
                {
                    str = str.Substring(index + 1);
                }

                str = str.Replace("_", " ");
                strToWrite += str+ "\n" ;
            }

            File.WriteAllText("timezone_cities/english.txt", strToWrite);

            //var ids = DateTimeZoneProviders.Tzdb.Ids;
            //var tz = GetTimeZone(location);
            //var s = tz.GetUtcOffset(Instant.FromDateTimeUtc(DateTime.UtcNow)).Seconds;
            //var hours = s / 60 / 60;
            //await SendTextMessageAsync(hours.ToString());
            //await SendTextMessageAsync(JsonConvert.SerializeObject(tz));
            //Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
