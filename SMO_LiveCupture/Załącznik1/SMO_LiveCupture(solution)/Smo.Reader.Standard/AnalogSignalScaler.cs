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
        IAircraftDataProvider _aircraftRepository;

        public static AnalogSignalScaler BuildScaler(string aircraftNumber, IAircraftDataProvider aircraftDataProvider)
        {
            var scaler = new AnalogSignalScaler(aircraftNumber, aircraftDataProvider);

            if (scaler?.HasCoefficients ?? false)
            {
                return scaler;
            }
            else
                return null;

        }

        private AnalogSignalScaler(string aircraftNumber, IAircraftDataProvider aircraftDataProvider)
        {
            _aircraftRepository = aircraftDataProvider;
            ScaleCoefficientList = GetScalingParameters(aircraftNumber);
        }

        public bool HasCoefficients
        {
            get
            {
                return ScaleCoefficientList?.Any() ?? false;
            }
        }

        private List<ScalingCoefficients> ScaleCoefficientList { get; set; }


        private List<ScalingCoefficients> GetScalingParameters(string aircraftNumber)
        {
            var scalingTableTsv = _aircraftRepository?.GetScalingTableTsvContent(aircraftNumber);

            var scaleParameters = scalingTableTsv?.Split(new[] { '\r', '\n' })
                .Where(l => !String.IsNullOrWhiteSpace(l))
                .Select(l =>
             {
                 var cells = l.Split('\t');
                 var signalName = cells[0];
                 var scaleParameter = new ScalingCoefficients(

                     x1: Convert.ToInt32(cells[1]),
                     y1: Convert.ToInt32(cells[2]),
                     x2: Convert.ToInt32(cells[3]),
                     y2: Convert.ToInt32(cells[4]),
                     signalName: signalName

                 );

                 return scaleParameter;
             }).ToList();


            return scaleParameters;
        }

        /// <summary>
        /// scales sample if scaling coefficients found, otherwise returns input unchanged
        /// </summary>
        /// <param name="inputSample"></param>
        /// <returns></returns>
        public KeyValuePair<string, ValueType> ScaleSample(KeyValuePair<string, ValueType> inputSample)
        {
            //no scaling file
            if (ScaleCoefficientList == null || !ScaleCoefficientList.Any())
            {
                return inputSample;
            }

            var scaleParams = ScaleCoefficientList.SingleOrDefault(sp => sp.SignalName == inputSample.Key);

            if (scaleParams != null)
            {
                var a = scaleParams.a;
                var b = scaleParams.b;

                //Warning - we assume that unscaled sample is uint
                var scaledValue = a * ((uint)inputSample.Value) + b;

                inputSample = new KeyValuePair<string, ValueType>(inputSample.Key, scaledValue);
            }

            return inputSample;
        }


    }
}
