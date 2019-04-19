using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmoReader;
using SmoReader.Entities;
using static SmoReader.Utils.MathUtils;
using Names = Smo.Common.Infrastructure.GlobalSettings.Names;
using System.IO;
using Smo.Common.Public.Repositories;
using Smo.Contracts.Enums;
using Smo.Common;

namespace SmoReader
{
    public class AnalysisEngine
    {
        IAircraftDataProvider _aircraftDataProvider;
        ILoggingService _loggingService;

        public AnalysisEngine(IAircraftDataProvider aircraftDataProvider, ILoggingService loggingService = null)
        {
            _aircraftDataProvider = aircraftDataProvider;
            _loggingService = loggingService;
        }

        public AnalysisResult Analyze(MergeResult mergedResult)
        {
            var isValidForAnalysis = !(
                mergedResult?.AcraTime.Length < 3 * 60 * 16
                 || mergedResult?.AcraStartTime < new DateTime(2010, 1, 1)
                 || mergedResult?.AcraEndTime < new DateTime(2010, 1, 1));
                    
            var analysisResult = new AnalysisResult()
            {
                Metadata = mergedResult.Metadata,
                AcraTime = mergedResult.AcraTime,
                EthernetEndTime = mergedResult.EthernetEndTime,
                EthernetStartTime = mergedResult.EthernetStartTime,
                Signals = mergedResult.Signals,
                EthernetTime = mergedResult.EthernetTime,
                FileSummaries = mergedResult.FileSummaries,
                isValidForAnalysis = isValidForAnalysis,
                Classification = DataClassification.InvalidOrUnknown
            };

            if (isValidForAnalysis)
            {
                var aircraftName = mergedResult.Metadata.AircraftName;
                var length = (double)mergedResult.AcraTime.Length;

                var alitudeLo = analysisResult.GetSignal(Names.Altitude);

                TimeSpan timeDelta;
                timeDelta = GetTotalTimeDelta(analysisResult.AcraTime);

                analysisResult.SatelliteErrorPercentage = (analysisResult.ToFewSatellites.Where(sample => sample > 0).Count() / length) * 100.0;
                analysisResult.GpsLockErrorPercentage = (analysisResult.GpsLock.Where(sample => sample < 0).Count() / length) * 100.0;
                analysisResult.VelocityErrorPercentage = (analysisResult.Velocity.Where(sample => sample <= 0).Count() / length) * 100.0;
                analysisResult.FixErrorPercentage = (analysisResult.FixError.Where(sample => sample > 0).Count() / length) * 100.0;
                analysisResult.AltitudeErrorPercentage = (analysisResult.Altitude.Where(sample => sample < 0).Count() / length) * 100.0;

                var totalTimeSpan = analysisResult.AcraEndTime - analysisResult.AcraStartTime;

                timeDelta = TimeSpan.FromTicks(Math.Max(0, timeDelta.Ticks));

                var timeErrorPercentage = (timeDelta.TotalSeconds / totalTimeSpan.Value.TotalSeconds) * 100;

                analysisResult.TimeSpan = timeDelta;
                analysisResult.TimeErrorPercentage = (int)timeErrorPercentage;

                ClassifyRecord(analysisResult);

                analysisResult.FileSummaries.ForEach(f =>
                {
                    f.Classification = analysisResult.Classification;
                });

                try
                {
                    var maxVals = analysisResult.Signals.Select(s => new Tuple<string, double>(s.Name, Convert.ToDouble(s.Samples.Max())));
                    var minVals = analysisResult.Signals.Select(s => new Tuple<string, double>(s.Name, Convert.ToDouble(s.Samples.Min())));

                    analysisResult.MinValues = minVals.ToList();
                    analysisResult.MaxValues = maxVals.ToList();
                }
                catch (Exception e)
                {
                    _loggingService?.Log($"Error while mapping minmax values in reader. Exception message: {e.Message}");
                }
            }
            
            return analysisResult;
        }

        private void ClassifyRecord(AnalysisResult analysisResult)
        {
            var classification = DataClassification.InvalidOrUnknown;

            var MaxVelocity = analysisResult.Velocity.Max();
            var MaxAltitudeChange = analysisResult.Altitude.Max() - analysisResult.Altitude.Min();


            bool isIncorrect =
               analysisResult.FixErrorPercentage > 20
               || analysisResult.SatelliteErrorPercentage > 20
               || analysisResult.TimeErrorPercentage > 20
               || analysisResult.AltitudeErrorPercentage > 20
               || analysisResult.VelocityErrorPercentage > 20;

            if (!isIncorrect) classification = DataClassification.Valid;

            analysisResult.Classification = classification;
        }


        //based on aggregated sample length
        private TimeSpan GetTotalTimeDelta(DateTime[] timeData, double sampleRateHz = 16)
        {
            var sampleLengthTicks = TimeSpan.FromSeconds(1).Ticks / 16;
            var sampleLength = TimeSpan.FromTicks(sampleLengthTicks);

            TimeSpan totalDelta = TimeSpan.Zero;

            var timeDataCopy = new DateTime[timeData.Length];
            timeData.CopyTo(timeDataCopy, 0);

            timeDataCopy = timeDataCopy.OrderBy(t => t).ToArray();

            for (int i = 1; i < timeDataCopy.Length; i++)
            {
                var delta = timeDataCopy[i] - timeDataCopy[i - 1];
                //compensate samplelength
                delta = delta - sampleLength;
                totalDelta = totalDelta + delta;
            }

            return totalDelta;
        }

    }
}
