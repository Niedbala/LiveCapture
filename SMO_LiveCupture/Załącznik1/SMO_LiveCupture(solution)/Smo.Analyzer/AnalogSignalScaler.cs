using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Smo.Common.Public.Repositories;
using SmoReader.Entities;

namespace SmoReader
{
    public class AnalogSignalScaler
    {
        IAircraftDataProvider _aircraftDataProvider;

        public string AircraftNumber { get; } 

        public AnalogSignalScaler(string aircraftNumber, IAircraftDataProvider aircraftRepository)
        {
            _aircraftDataProvider = aircraftRepository;
            ScalingCoefficients = GetScalingCoefficients(aircraftNumber);
            AircraftNumber = aircraftNumber;
        }

        private List<ScalingCoefficients> ScalingCoefficients = new List<Entities.ScalingCoefficients>();


        private List<ScalingCoefficients> GetScalingCoefficients(string aircraftNumber)
        {
            var scalingTableTsv = _aircraftDataProvider?.GetScalingTableTsvContent(aircraftNumber);

            var scalingCoefficients = scalingTableTsv?.Split(new[] { '\r', '\n' })
                .Where(l => !String.IsNullOrWhiteSpace(l))
                .Select(l =>
             {
                 var cells = l.Split('\t');
                 var signalName = cells[0];
                 var scalingCoefficient = new ScalingCoefficients(

                     x1: Convert.ToInt32(cells[1]),
                     y1: Convert.ToInt32(cells[2]),
                     x2: Convert.ToInt32(cells[3]),
                     y2: Convert.ToInt32(cells[4]),
                     signalName: signalName

                 );

                 return scalingCoefficient;
             }).ToList();


            return scalingCoefficients;
        }
        
        public List<double> Scale(uint[] inputData, string signalName)
        {
            //no scaling file, return input
            if (ScalingCoefficients == null || !ScalingCoefficients.Any())
            {
                return null;
            }

            var scaleParams = ScalingCoefficients.SingleOrDefault(sp => sp.SignalName == signalName);

            var a = scaleParams.a;
            var b = scaleParams.b;

            var output = inputData.Select(sample => a * (sample
           // + 32768
            ) + b).ToList();

            return output;
        }



    }
}
