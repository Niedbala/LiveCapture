using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pechkin;
using Pechkin.Synchronized;
using SmoReader.Entities;

using Smo.Contracts.Enums;
using System.Threading.Tasks;
using Smo.Common;
using Smo.Common.Infrastructure;


namespace ReportCreator
{

    public class ReportEngine
    {
        private readonly ILoggingService _loggingService;
        public string ResultPath { get; set; }


        string _baseOutputPath { get; set; }
        byte[] _output;
        private List<string> _signalHeaders = new List<string>();

        public ReportEngine(string concreteAnalysisOutputFolder, ILoggingService loggingService)
        {
            _loggingService = loggingService;

            _baseOutputPath = concreteAnalysisOutputFolder;
        }



        public static void CreateFileProcessingReport(List<FileSummary> fileSummaries, string destinationPath, string concreteConversionOutputFolder)
        {
            var fileProcessingSummaryLines = new List<string>();

            var headerLine = "Nazwa" + "\t"
                                      + "Odczyt OK" + "\t"
                                      + "Access violation" + "\t"
                                      + "puste pakiety" + "\t"
                                      + "inny blad" + "\t"
                                      + "brak IPv4" + "\t"
                                      + "brak Probek" + "\t";

            var contentLines = fileSummaries.OrderBy(rep => rep.IsReadSuccessful).ThenBy(rep => rep.FilePath).Select(r =>
            {
                var resultString = r.FilePath.Replace(destinationPath, "") + "\t";

                var conditionString = r.IsReadSuccessful.ToString() + "\t"
                                   + r.Flags.AccessViolationError.ToString() + "\t"
                                   + r.Flags.EmptyPacketData.ToString() + "\t"
                                   + r.Flags.GeneralReadError.ToString() + "\t"
                                   + r.Flags.NoIpV4Packet.ToString() + "\t"
                                   + r.Flags.NoSamplesError.ToString() + "\t";

                conditionString = conditionString.Replace("True", "T").Replace("False", "N");

                resultString = resultString + conditionString;

                return resultString;
            });

            fileProcessingSummaryLines.Add(headerLine);
            fileProcessingSummaryLines.AddRange(contentLines);

            var processingResultPath = $@"{concreteConversionOutputFolder}\\fileprocessingreport.txt";
            (new FileInfo(processingResultPath)).Directory.Create();
            File.WriteAllLines(processingResultPath, fileProcessingSummaryLines);
        }

        public void CreateBadAnalysisResultSummary(List<ReportPage> badresults)
        {

            var headerLine =
                "Aircraft" + "\t" +
                "Start Date" + "\t" +
                "End Date" + "\t" +
                "Source Directory" + "\t" +
                "Group Name" + "\t" +
                "Time [s]" + "\t" +
                "Number of source files" + Environment.NewLine;


            var badresultLines = badresults.Select(br =>
            {
                var resultString = br.AircraftName + "\t" +
                                   br.StartDate + "\t" +
                                   br.EndDate + "\t" +
                                   br.DirectoryName + "\t" +
                                   br.GroupName + "\t" +
                                   br.sampleCountInSeconds + "\t" +
                                   br.FileCount;
                return resultString;

            })
            .ToList();

            badresultLines.Insert(0, headerLine);

            File.WriteAllLines($@"{_baseOutputPath}\Analysis_report_badresults.txt", badresultLines);

        }

        public string DumpRecordTextOutput(AnalysisResult analysisResult, string concreteAnalysisOutputFolder)
        {
            //start with time samples header
            string headerLine = "Absolute Time" + "\t";

            headerLine = headerLine + "Relative Time [ms]" + "\t";

            analysisResult.Signals.ForEach(signal =>
            {
                headerLine = headerLine + signal.Name + "\t";
            });

            var timeSamples = analysisResult.AcraTime;
            var timeCount = timeSamples.Count();

            var fileName = analysisResult.Name + ".tsv";

            var validState = analysisResult.Classification == DataClassification.ValidFlightWhole ? "VALID" : "INVALID";

            var dumpTextDir = $"{concreteAnalysisOutputFolder}\\{analysisResult.AircraftName}\\{validState}";

            Directory.CreateDirectory(dumpTextDir);

            var dumpTextPath = dumpTextDir + "\\" + fileName;

            if (File.Exists(dumpTextPath)) File.Delete(dumpTextPath);

            File.WriteAllText(dumpTextPath, string.Empty);

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
                            _loggingService.Log($"Sample missing while dumping text for record: {analysisResult.Name} signal: {signal.Name} timestamp: {analysisResult.AcraTime[timeIndex]}");
                            valuesLine = valuesLine + 0.0 + "\t";
                        }




                    });

                    sw.WriteLine(valuesLine);
                }
            }

            //validate the created file
            var lineCount = File.ReadLines(dumpTextPath).Count();
            if (lineCount != timeCount + 1)
                throw new Exception($"Wrong number of lines in dumped tsv: {dumpTextPath}");


            return dumpTextPath;

        }

        public void CreatePdfReport(IList<ReportPage> pages, string aircraftName)
        {
            if (!(_signalHeaders != null && _signalHeaders.Any()))
                //get headers from all analyzed flights, regardless of number of signals
                _signalHeaders = pages.SelectMany(p => p.SignalHeaders).Distinct().ToList();

            pages.ToList().ForEach(p => AddAnalysisSummaryLine(p, _signalHeaders));


            var pageBudndles = new List<List<ReportPage>>();

            var bundleSize = 100;
            while (pages.Any())
            {
                pageBudndles.Add(pages.Take(bundleSize).ToList());
                pages = pages.Skip(bundleSize).ToList();
            }

            for (int i = 0; i < pageBudndles.Count; i++)
            {
                var pageBundle = pageBudndles[i];

                var filePath = getReportTempFileName(aircraftName, i);
                (new FileInfo(filePath)).Directory.Create();

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filePath, false))
                {
                    foreach (var page in pageBundle)
                    {
                        file.WriteLine(page.ContentString);

                    }
                    // _output = string.Join(Environment.NewLine, pages.Select(p => p.ContentString).ToList());
                };
            }


        }

        public void DumpPdfFile(string aircraftName, bool isDumpPdf = true)
        {
            DumpAnalysisSummary(aircraftName);

            if (!isDumpPdf)
                return;

            var tempFiles = Directory.EnumerateFiles($@"{_baseOutputPath}\\{aircraftName}\\PdfTemp\\", "*.txt");

            int i = 0;
            foreach (var tempfile in tempFiles)
            {
                try
                {
                    _output = File.ReadAllBytes(tempfile);

                    if (_output.Length <= 0)
                        return;

                    GlobalConfig gc = new GlobalConfig();
                    IPechkin pechkin = new SynchronizedPechkin(gc);
                    var convertedPdf = pechkin.Convert(_output);

                    var filePath = $@"{_baseOutputPath}\\{aircraftName}_report{i}.pdf";
                    i++;

                    ResultPath = filePath;

                    File.WriteAllBytes(filePath, convertedPdf);

                    analysisSummaryLines.Clear();
                }
                //TODO implement log
                catch (Exception e)
                {

                };
            }



        }

        public ReportPage CreatePage(ReportInput input)
        {
            //todo move to config directory.
            var htmlTemplate = File.ReadAllText(@"reportTemplate.xml");

            var processedHtml = htmlTemplate;
            processedHtml = processedHtml.Replace("@strainGauge1", CreateBase64SrcAttribute(input.strainGauge1Image));
            processedHtml = processedHtml.Replace("@strainGauge2", CreateBase64SrcAttribute(input.strainGauge2Image));

            processedHtml = processedHtml.Replace("@strainGauge3", CreateBase64SrcAttribute(input.strainGauge3Image));
            processedHtml = processedHtml.Replace("@strainGauge4", CreateBase64SrcAttribute(input.strainGauge4Image));

            processedHtml = processedHtml.Replace("@strainGauge5", CreateBase64SrcAttribute(input.strainGauge5Image));
            processedHtml = processedHtml.Replace("@strainGauge6", CreateBase64SrcAttribute(input.strainGauge6Image));

            processedHtml = processedHtml.Replace("@strainGauge7", CreateBase64SrcAttribute(input.strainGauge7Image));
            processedHtml = processedHtml.Replace("@strainGauge8", CreateBase64SrcAttribute(input.strainGauge8Image));

            processedHtml = processedHtml.Replace("@altitude", CreateBase64SrcAttribute(input.altitudeImage));
            processedHtml = processedHtml.Replace("@velocity", CreateBase64SrcAttribute(input.velocityImage));

            processedHtml = processedHtml.Replace("@GaGinput", CreateBase64SrcAttribute(input.diffedLoPassedImage));

            processedHtml = processedHtml.Replace("@Title", $"{input.InputResult.AircraftName} {input.InputResult.AcraStartTime} - {input.InputResult.AcraEndTime}");
            processedHtml = processedHtml.Replace("@SatteliteErrorPercent", $"{(int)input.InputResult.SatelliteErrorPercentage}");
            processedHtml = processedHtml.Replace("@VelocityErrorPercent", $"{(int)input.InputResult.VelocityErrorPercentage}");
            processedHtml = processedHtml.Replace("@FixErrorPercent", $"{(int)input.InputResult.FixErrorPercentage}");
            processedHtml = processedHtml.Replace("@GpsLockErrorPercent", $"{(int)input.InputResult.GpsLockErrorPercentage}");

            processedHtml = processedHtml.Replace("@NumberOfLandings", $"{input.InputResult.LandingIndices.Count}");
            processedHtml = processedHtml.Replace("@NumberOfTakeOffs", $"{input.InputResult.TakeOffIndices.Count}");
            processedHtml = processedHtml.Replace("@NumberOfTGs", $"{input.InputResult.TouchAndGoIndices.Count}");
            processedHtml = processedHtml.Replace("@TimeErrorPercent", $"{input.InputResult.TimeErrorPercentage}");
            processedHtml = processedHtml.Replace("@TimeErrorSeconds", $"{(int)input.InputResult.TimeDelta.TotalSeconds}");
            processedHtml = processedHtml.Replace("@NumberOfFiles", $"{input.NumberOfFiles}");

            var finalAssesmentStyle = "";

            finalAssesmentStyle = input.InputResult.Classification == Smo.Contracts.Enums.DataClassification.ValidFlightWhole
                || input.InputResult.Classification == Smo.Contracts.Enums.DataClassification.ValidFlightInAir ? "GOOD" : "BAD";
            var finalAssesmentDisplayed = "NIEPOPRAWNY";

            if (input.InputResult.Classification == DataClassification.Hangar)
                finalAssesmentDisplayed = "HANGAR";

            if (input.InputResult.Classification == DataClassification.ValidFlightInAir ||
                input.InputResult.Classification == DataClassification.ValidFlightWhole)
                finalAssesmentDisplayed = "POPRAWNY";

            processedHtml = processedHtml.Replace("@finalAssessmentStyle", finalAssesmentStyle.ToLower());
            processedHtml = processedHtml.Replace("@finalAssessment", finalAssesmentDisplayed);
            processedHtml = processedHtml.Replace("@group", input.InputResult.GroupName);
            processedHtml = processedHtml.Replace("@directory", input.InputResult.DirectoryName);

            var headers = input.InputResult.Signals.Select(s => s.Name);
            var maxVals = input.InputResult.Signals.Select(s => new Tuple<string, double>(s.Name, Convert.ToDouble(s.Samples.Max())));
            var minVals = input.InputResult.Signals.Select(s => new Tuple<string, double>(s.Name, Convert.ToDouble(s.Samples.Min())));

            return new ReportPage()
            {
                AltitudeErrorPercentage = input.InputResult.AltitudeErrorPercentage,
                ContentString = processedHtml,
                DirectoryName = input.InputResult.FileSummaries.FirstOrDefault().FilePath,
                FixErrorPercentage = input.InputResult.FixErrorPercentage,
                GpsLockErrorPercentage = input.InputResult.GpsLockErrorPercentage,
                GroupName = input.InputResult.GroupName,
                SatelliteErrorPercentage = input.InputResult.SatelliteErrorPercentage,
                TimeDelta = input.InputResult.TimeDelta,
                TimeErrorPercentage = input.InputResult.TimeErrorPercentage,
                VelocityErrorPercentage = input.InputResult.VelocityErrorPercentage,
                EndDate = input.InputResult.EndDate,
                StartDate = input.InputResult.StartDate,
                Classification = input.InputResult.Classification,
                Name = input.InputResult.Name,
                AircraftName = input.InputResult.AircraftName,
                TakeOffIndices = input.InputResult.TakeOffIndices,
                LandingIndices = input.InputResult.LandingIndices,
                TouchAndGoIndices = input.InputResult.TouchAndGoIndices,
                FileCount = input.InputResult.FileCount,
                MaxValues = maxVals.ToList(),
                MinValues = minVals.ToList(),
                SignalHeaders = headers.ToList(),
                LandingTs = input.InputResult.LandingTs,
                TakeOffTs = input.InputResult.TakeOffTs


            };

        }



        private string getReportTempFileName(string aircraftName, int index)
        {
            return $"{_baseOutputPath}\\{aircraftName}\\PdfTemp\\WriteLinesTemp{index}.txt";
        }

        private double GetValuesForHeader(string header, List<Tuple<string, double>> kvPairs)
        {

            double val = 0;
            var kvPair = kvPairs.FirstOrDefault(v => v.Item1 == header);
            if (kvPair == null)
                val = double.NaN;
            else
                val = kvPair.Item2;

            return val;

        }

        private List<string> analysisSummaryLines = new List<string>();
        private void AddAnalysisSummaryLine(IAnalysisSummary r, List<string> signalheaders)
        {

            var resultString = $"{ r.AircraftName } { r.StartDate} - { r.EndDate}\t";

            int duration = (int)(r.LandingTs - r.TakeOffTs).TotalMinutes;

            resultString = resultString
                                  + r.LandingIndices.Count + "\t"
                                  + r.TakeOffIndices.Count + "\t"
                                  + r.TouchAndGoIndices.Count + "\t"
                                  + r.SatelliteErrorPercentage + "\t"
                                  + r.FixErrorPercentage + "\t"
                                  + r.GpsLockErrorPercentage + "\t"
                                  + r.TimeErrorPercentage + "\t"
                                  + r.VelocityErrorPercentage + "\t"
                                  + r.FileCount + "\t"
                                  + (r.Classification == Smo.Contracts.Enums.DataClassification.ValidFlightWhole ? "POPRAWNY" : "NIEPOPRAWNY") + "\t"
                                  + r.TakeOffTs + "\t"
                                  + r.LandingTs + "\t"
                                  + duration + "\t"
                                  //  + r.
                                  ;

            var minValuesWithBlanks = signalheaders.Select(header => GetValuesForHeader(header, r.MinValues));
            var maxValuesWithBlanks = signalheaders.Select(header => GetValuesForHeader(header, r.MaxValues));

            var minVals = string.Join("\t", minValuesWithBlanks.Select(v => double.IsNaN(v) ? "*" : v.ToString()));
            var maxVals = string.Join("\t", maxValuesWithBlanks.Select(v => double.IsNaN(v) ? "*" : v.ToString()));

            resultString = resultString + minVals + "\t" + maxVals;


            analysisSummaryLines.Add(resultString);

        }

        private void DumpAnalysisSummary(string aircraftName)
        {

            var summaryToFileLines = new List<string>();
            //custom headers
            var headerLine = "Nazwa" + "\t"
                           + "Ladowania" + "\t"
                           + "Starty" + "\t"
                           + "TG" + "\t"

                           + "satelita %" + "\t"
                           + "fixError %" + "\t"
                           + "GPS lock %" + "\t"
                           + "Czas %" + "\t"
                           + "Predkosc %" + "\t"

                           + "l. plików" + "\t"
                           + "status" + "\t"
                           + "take off" + "\t"
                           + "landing" + "\t"
                           + "czas [min]" + "\t"
                           ;


            //add min max headers

            var minHeaders = string.Join("\t", _signalHeaders.Select(h => "min_" + h));
            var maxHeaders = string.Join("\t", _signalHeaders.Select(h => "max_" + h));

            headerLine = headerLine + minHeaders + "\t" + maxHeaders;

            summaryToFileLines.Add(headerLine);

            summaryToFileLines.AddRange(analysisSummaryLines);

            File.WriteAllLines($@"{_baseOutputPath}\Analysis_report_{aircraftName}.txt", summaryToFileLines);
        }


        private string CreateBase64SrcAttribute(string base64Image)
        {
            var returnstring = $@"src='data:image/jpg;base64,{base64Image}'";
            return returnstring;
        }

    }
}
