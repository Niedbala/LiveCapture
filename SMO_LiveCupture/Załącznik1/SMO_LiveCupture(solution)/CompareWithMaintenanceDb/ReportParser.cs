using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareWithMaintenanceDb
{

    public class Flight
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public string AircraftName;
        public double DurationMinutes
        {
            get
            {
                return (EndTime - StartTime).TotalMinutes;
            }
        }

        public bool IsValid;
    }

    public class Parser
    {
        public List<T> ParseFiles<T>(string reportPath, Func<string, string, T> strategy, bool skipHeader = false)
        {
            var result = new List<T>();

            var files = Directory.GetFiles(reportPath);

            files.ToList().ForEach(file =>
            {
                var lines = File.ReadAllLines(file);


                if (skipHeader)
                    lines = lines.Skip(1).ToArray();

                lines.ToList().ForEach(line =>
                {


                    var singleResult = strategy(line, file);

                    if (result != null)
                        result.Add(singleResult);

                });

            });

            return result;
        }

        private Flight ParseSmoLine(string line, string filename)
        {
            var cells = line.Split('\t');

            var baseCell = cells[0].Replace(" - ", " ");
            var baseCells = baseCell.Split(' ');

            var aircraftName = baseCells[0];

            var startDate = baseCells[1];
            var startTime = baseCells[2];

            var endDate = baseCells[3];
            var endTime = baseCells[4];

            //var startStamp = Convert.ToDateTime(startDate + " " + startTime);
            //var endStamp = Convert.ToDateTime(endDate + " " + endTime);

            var startStamp = Convert.ToDateTime(cells[11]);
            var endStamp = Convert.ToDateTime(cells[12]);

            var flight = new Flight()
            {
                AircraftName = aircraftName,
                StartTime = startStamp,
                EndTime = endStamp
            };

            var isValid = cells[10] == "POPRAWNY";

            //var isValid = true;

            if (isValid)
                return flight;

            return null;
        }

        private Flight ParseTurawaLine(string line, string filename)
        {
            var cells = line.Split(' ');

            var aircraftName = cells[1];

            var startDate = cells[0];
            var startTime = cells[5];

            var endDate = cells[0];
            var endTime = cells[6];

            var startStamp = Convert.ToDateTime(startDate + " " + startTime);
            var endStamp = Convert.ToDateTime(endDate + " " + endTime);

            var flight = new Flight()
            {
                AircraftName = aircraftName,
                StartTime = startStamp,
                EndTime = endStamp
            };

            return flight;

        }

        private Flight ParseMjkLine(string line, string filename)
        {

            var aircraftName = Path.GetFileNameWithoutExtension(filename);

            aircraftName = aircraftName.Trim();



            if (string.IsNullOrWhiteSpace(line))
                return null;



            var cells = line.Split(' ', '\t');

            var day = cells[1].Replace(":", "-");
            var startHour = cells[2];
            var landingHour = cells[3];

            var startString = day + " " + startHour;
            var endString = day + " " + landingHour;

            var flight = new Flight()
            {
                AircraftName = aircraftName,
                StartTime = Convert.ToDateTime(startString),
                EndTime = Convert.ToDateTime(endString)
            };

            return flight;
        }

        public List<Flight> ParseSmoReports(string reportPath)
        {
            var flights = ParseFiles(reportPath, ParseSmoLine, true).Where(f => f != null).ToList();

            return flights;
        }

        public List<Flight> ParseTurawa(string reportPath)
        {
            var flights = ParseFiles(reportPath, ParseTurawaLine).Where(f => f != null).ToList();

            return flights;
        }

        public List<Flight> ParseMjk(string reportPath)
        {
            var flights = ParseFiles(reportPath, ParseMjkLine).Where(f => f != null).ToList();

            return flights;
        }

    }
}
