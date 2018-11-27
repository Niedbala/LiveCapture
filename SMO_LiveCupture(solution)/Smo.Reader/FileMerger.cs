using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmoReader.Entities;
using SmoReader.Utils;
using static SmoReader.Utils.MathUtils;
using Smo.Common;
using Smo.Common.Enums;
using Smo.Common.Models;
using Smo.Common.Infrastructure;
using Smo.Common.Public.Models;
using Smo.Common.Public.Repositories;
using Smo.Common.Contracts;

namespace SmoReader
{
    public class FileMerger
    {

        IAircraftDataProvider _aircraftDataProvider { get; set; }
        IGlobalPaths _globalPaths { get; set; }

        //Converts and mergers multiple files
        public FileMerger(IAircraftDataProvider aircraftDataProvider,
            IGlobalPaths globalPaths, ILoggingService loggingService = null)
        {


            _aircraftDataProvider = aircraftDataProvider;
            _globalPaths = globalPaths;
            _loggingService = loggingService;

        }

        public DateTime cutoffDate = new DateTime(1980, 1, 1);

        public string FolderPath;
        private readonly ILoggingService _loggingService;

        //Merges a list of convereted files into a single record
        public MergeResult MergeFiles(List<FileReadResult> fileReadResults)
        {
            var fileMetadatas = fileReadResults.Select(f => f.Metadata).ToList();

            fileReadResults = fileReadResults.Where(fr => fr.Metadata.IsReadSuccessful).ToList();

            var powerupCounts = fileReadResults.Select(sfr => sfr.Metadata.PowerUpCount).Distinct().ToList();

            if (powerupCounts.Count() != 1)
            {
                var filesMsg = fileReadResults.Select(file => file?.Metadata?.FilePath);
                var powerUpCountMsg = powerupCounts.Select(c => c.ToString() + ' ').ToList();
                _loggingService?.Log($"list of powerupcounts {String.Join(" ", powerUpCountMsg)} for files: { String.Join(" ", filesMsg) }", LogLevel.Error);
                _loggingService?.Log("Tried to merge reader results with different power-up counts.", LogLevel.Error);

                return null;
            }
            var powerupCount = powerupCounts.SingleOrDefault();

            var aircraftNamesInGroup = fileReadResults.Select(f => f.Metadata.AircraftName).Distinct();
            if (aircraftNamesInGroup.Count() != 1)
            {
                var msg = "Error: Attempted to merge files from multiple aircraft!";
                _loggingService?.Log(msg, LogLevel.Error);
                throw new Exception();
            }

            //actual merging:
            var mergedReadResult = fileReadResults
                .Where(fr => fr.Metadata.IsReadSuccessful)
                .OrderBy(r => r.AcraStartTime).Aggregate((current, next) =>
               {
                   var mergedTime = current.AcraTime.Concat(next.AcraTime);

                   var mergedSignals = current.Signals.Select(signal =>
                   {
                       var samplesToAppend =
                           next.Signals.FirstOrDefault(nextSignal => nextSignal.Name == signal.Name).Samples;
                       var mergedSamples = signal.Samples.Concat(samplesToAppend);

                       return new SignalData()
                       {
                           Name = signal.Name,
                           Samples = mergedSamples.ToArray()
                       };
                   });
                   var _mergedTime = mergedTime.ToArray();

                   var tempMergeResult = new FileReadResult()
                   {
                       //Parameters = singleFileResults.FirstOrDefault().Parameters,
                       Signals = mergedSignals.ToList(),
                       AcraTime = _mergedTime
                   };

                   return tempMergeResult;
               });

            
            var finalMergeResult = new MergeResult()
            {
                EthernetEndTime = mergedReadResult.EthernetEndTime,
                EthernetStartTime = mergedReadResult.EthernetStartTime,

                Metadata = new RecordMetaData()
                {
                    //Todo: this is unsafe. group by aircraftname?
                    AircraftName = fileReadResults.Select(f => f.Metadata.AircraftName).Distinct().Single(),
                    PowerUpCount = powerupCount,
                    AcraEndTs = mergedReadResult?.AcraTime?.Last(),
                    AcraStartTs = mergedReadResult?.AcraTime?.First(),
                    Classification = Smo.Contracts.Enums.DataClassification.InvalidOrUnknown,
                    EthernetEndTs = mergedReadResult?.EthernetTime?.First(),
                    EthernetStartTs = mergedReadResult?.EthernetTime?.Last(),
                    SourceMacAddress = fileMetadatas.FirstOrDefault().SourceMacAddress
                },
                AcraTime = mergedReadResult.AcraTime,
                Signals = mergedReadResult.Signals,
                EthernetTime = mergedReadResult.EthernetTime,

                FileSummaries = fileMetadatas
                //Parameters = mergedReadResult.Parameters,
            };

            finalMergeResult.FileSummaries = fileReadResults.Select(fr => fr.Metadata).ToList();
          
            if (finalMergeResult != null)
            {
                RemoveOutlierTs(finalMergeResult);
            }

            return finalMergeResult;
        }

        /// <summary>
        /// remove wrong indices from a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        private IEnumerable<T> RemoveInvalidIndices<T>(IEnumerable<T> collection, List<int> indices)
        {
            if (collection == null || !collection.Any())
                return new List<T>();
            
            return collection.Where((e, i) => !indices.Contains(i));
        }

        private void RemoveOutlierTs(MergeResult mergeResult)
        {
            var tsCount = mergeResult.AcraTime.Count();

            var tsCopy = new List<DateTime>();

            tsCopy.AddRange(mergeResult.AcraTime);

            var medianTime = tsCopy.OrderBy(ts => ts).ToList()[tsCount / 2];

            var badTimeIndices = new List<int>();

            for (int i = tsCount - 1; i >= 0; i--)
            {
                var timeDeltaMinutes = Math.Abs((mergeResult.AcraTime[i] - medianTime).TotalMinutes);

                if (timeDeltaMinutes > 180)
                {
                    badTimeIndices.Add(i);
                }
            }

            if (badTimeIndices.Any())
            {
                mergeResult.AcraTime = RemoveInvalidIndices(mergeResult.AcraTime, badTimeIndices).ToArray();

                mergeResult.EthernetTime = RemoveInvalidIndices(mergeResult.EthernetTime, badTimeIndices).ToArray();

                var signalCount = mergeResult.Signals.Count();

                for (int j = 0; j < signalCount; j++)
                {
                    var signal = mergeResult.Signals[j];


                    mergeResult.Signals[j] = new SignalData()
                    {
                        Name = signal.Name,
                        Samples = RemoveInvalidIndices(signal.Samples, badTimeIndices).ToArray()
                    };


                };
            }
        }
    }
}
