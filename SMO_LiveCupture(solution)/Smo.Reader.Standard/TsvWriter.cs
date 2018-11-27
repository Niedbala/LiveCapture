using Smo.Common;
using Smo.Contracts.Enums;
using SmoReader.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Reader.Standard
{
    public class TsvWriter
    {

        private string GenerateName(AnalysisResult analysisResult)
        {
            var format = "yyyy_MM_dd HH_mm_ss";

            var firstts = analysisResult.AcraTime.First().ToString(format);
            var lastts = analysisResult.AcraTime.Last().ToString(format);

            return $"{analysisResult.Metadata.AircraftName}_{firstts}_{lastts}"
                .Replace(':', '_')
                .Replace('/', '_')
                .Replace('\\', '_');
        }

        private readonly ILoggingService _loggingService;

        string _baseOutputPath { get; set; }

        public TsvWriter(string baseOutputPath, ILoggingService loggingService = null)
        {
            _baseOutputPath = baseOutputPath;
            this._loggingService = loggingService;
        }

        public string WriteTsv(AnalysisResult analysisResult)
        {
            var name = GenerateName(analysisResult);

            //start with time samples header
            string headerLine = "Absolute Time" + "\t";

            headerLine = headerLine + "Relative Time [ms]" + "\t";

            analysisResult.Signals.ForEach(signal =>
            {
                headerLine = headerLine + signal.Name + "\t";
            });

            var timeSamples = analysisResult.AcraTime;
            var timeCount = timeSamples.Count();

            var fileName = name + ".tsv";

            var validState = analysisResult.Classification == DataClassification.Valid ? "VALID" : "INVALID";

            var dumpTextDir = $"{_baseOutputPath}\\{analysisResult.Metadata.AircraftName}";

            Directory.CreateDirectory(dumpTextDir);

            var dumpTextPath = dumpTextDir + "\\" + fileName;

            using (StreamWriter sw = File.CreateText(dumpTextPath))
            {
                sw.WriteLine(headerLine);

                var firstTime = analysisResult.AcraTime[0];

                for (int timeIndex = 0; timeIndex < timeCount; timeIndex++)
                {
                    var valuesLine = "";

                    //add absolute time cell
                    valuesLine = valuesLine + analysisResult.AcraTime[timeIndex].ToString() + "\t";

                    //add relative time cell
                    valuesLine = valuesLine + (analysisResult.AcraTime[timeIndex] - firstTime).TotalMilliseconds + "\t";

                    analysisResult.Signals.ForEach(signal =>
                    {
                        if (timeIndex < signal.Samples.Count())
                            valuesLine = valuesLine + signal.Samples[timeIndex] + "\t";
                        else
                        {
                            _loggingService?.Log($"Sample missing while dumping text for record: {name} signal: {signal.Name} timestamp: {analysisResult.AcraTime[timeIndex]}");
                            valuesLine = valuesLine + 0.0 + "\t";
                        }
                    });

                    sw.WriteLine(valuesLine);
                }
            }

            return dumpTextPath;

        }
    }
}

