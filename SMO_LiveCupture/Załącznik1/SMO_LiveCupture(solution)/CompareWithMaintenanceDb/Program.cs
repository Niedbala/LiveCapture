using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace CompareWithMaintenanceDb
{
    static class Program
    {
        private static int TimeStampOffsetMinutes = 10;

        private static bool EqualsByTimestamps(Flight referenceFlight, Flight checkedFlight)
        {
            var minutes = TimeStampOffsetMinutes;

            var refStart = referenceFlight.StartTime;
            var refLanding = referenceFlight.EndTime;

            var isStartMatch = Math.Abs((checkedFlight.StartTime - refStart).TotalMinutes) < minutes;
            var isEndMatch = Math.Abs((checkedFlight.EndTime - refLanding).TotalMinutes) < minutes;

            return (isStartMatch && isEndMatch);
        }

        private static ComparisonSingleFlight CompareFlight(Flight turawaFlight, List<Flight> smoFlights, List<Flight> mjkFlights)
        {
            var matchingSmo = smoFlights
                .Where(f => f.AircraftName == turawaFlight.AircraftName)
                .FirstOrDefault(f => EqualsByTimestamps(turawaFlight, f));

            var matchingMjk = mjkFlights
                .Where(f => f.AircraftName == turawaFlight.AircraftName)
                .FirstOrDefault(f => EqualsByTimestamps(turawaFlight, f));


            smoFlights.Remove(matchingSmo);
            mjkFlights.Remove(matchingMjk);


            var result = new ComparisonSingleFlight()
            {
                MjkFlight = matchingMjk,
                SmoFlight = matchingSmo,
                TurawaFlight = turawaFlight
            };

            return result;

        }


        public static List<Flight> RemoveDuplicates(List<Flight> flights)
        {
            flights = flights.GroupBy(f => f.AircraftName).Select(
                 aircraftGroup => aircraftGroup.GroupBy(f => f.EndTime)
                 .Select(dupes => dupes.FirstOrDefault())
                 )
                 .SelectMany(f => f).ToList();
            return flights;
        }

        public static List<Flight> OrderAndFilterFlights(List<Flight> flights)
        {


            return flights.OrderBy(f => f.EndTime)
                .Where(f => f.EndTime > new DateTime(2016, 7, 1))
               // .Where(f => f.EndTime < new DateTime(2017, 3, 23))
                .ToList();
        }

        private static string listFlights(List<Flight> flights)
        {
            var filtered = OrderAndFilterFlights(flights);


            var listed = filtered.GroupBy(f => f.AircraftName).OrderBy(g => g.Key)
         .Select(fg => fg.Key + " " + fg.Count())
         .ToList();
            var table = string.Join(Environment.NewLine, listed);

            return table;
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var reportParser = new Parser();
            var smoFlights = reportParser.ParseSmoReports(@"C:\ROBOCZY\Turawa_compare\reports").ToList();
            var turawaFlights = reportParser.ParseTurawa(@"C:\ROBOCZY\Turawa_compare\turawa").Distinct()
                .ToList();


            var mjkFlights = reportParser.ParseMjk(@"C:\ROBOCZY\smo_mjk").Distinct()
               .ToList();



            turawaFlights = turawaFlights.Where(t => t.DurationMinutes > 3).ToList();

            smoFlights = OrderAndFilterFlights(smoFlights).Where(f => !f.AircraftName.Contains("310")).ToList();
            turawaFlights = OrderAndFilterFlights(turawaFlights);
            mjkFlights = OrderAndFilterFlights(mjkFlights);

            var aircst = turawaFlights.Select(t => t.AircraftName).Distinct();

            var turawaStart = turawaFlights.Min(f => f.StartTime);
            var turawaEnd = turawaFlights.Max(f => f.EndTime);

            var smoEnd = smoFlights.Max(f => f.EndTime);

            turawaFlights = RemoveDuplicates(turawaFlights);
            var compared = turawaFlights.Where(f => !f.AircraftName.Contains("310")).Select(f => CompareFlight(f, smoFlights, mjkFlights)).OrderBy(c => c.TurawaFlight.AircraftName).ToList();

            var comparedString = String.Join(
                Environment.NewLine,
                compared.Select(c => GenerateComparisonLine(c))
                );

            var smoMissing = String.Join(
                Environment.NewLine,
                compared.Where(c => c.SmoFlight == null)
                .Select(f => f.TurawaFlight.StartTime.ToString() + " " + f.TurawaFlight.AircraftName).ToList());

            var mjkMissing = String.Join(
                Environment.NewLine,
                compared.Where(c => c.MjkFlight == null)
                .Select(f => f.TurawaFlight.StartTime.ToString() + " " + f.TurawaFlight.AircraftName).ToList());

            var orphanedMjkString = GenerateStringFromLines(mjkFlights.Select(f => f.StartTime.ToString() + " " + f.AircraftName));
            var orphanedSmoString = GenerateStringFromLines(smoFlights.Select(f => f.StartTime.ToString() + " " + f.AircraftName));

            var listedTurawa = listFlights(turawaFlights);
            var listedSmo = listFlights(smoFlights);
            var listedMjk = listFlights(mjkFlights);


        }

        public static string GenerateComparisonLine(ComparisonSingleFlight compareResult)
        {
            var resultLine = "";

            var refFlight = compareResult.TurawaFlight;

            var mjkLabel = compareResult.MjkFlight != null ? "OK" : "MISSING!!";
            var smoLabel = compareResult.SmoFlight != null ? "OK" : "MISSING!!";

            var smotime = compareResult?.SmoFlight?.DurationMinutes ?? 0;
            var mjktime = compareResult?.MjkFlight?.DurationMinutes ?? 0;
            var turawaTime = compareResult?.TurawaFlight?.DurationMinutes ?? 0;

            resultLine = $"{refFlight.AircraftName}\t{refFlight.StartTime}\t{refFlight.EndTime}\t{mjkLabel}\t{smoLabel}\t{mjktime}\t{smotime}\t{turawaTime}";

            return resultLine;


        }

        public static string GenerateStringFromLines(IEnumerable<string> lines)
        {
            return String.Join(
               Environment.NewLine,
               lines);
        }


    }
}
