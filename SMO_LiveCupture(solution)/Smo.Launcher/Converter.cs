using Smo.Common;
using Smo.Common.Enums;
using Smo.Common.Infrastructure;
using Smo.Common.Models;
using Smo.Common.Utils;
using Smo.Launcher;
using SmoReader.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Smo.Common.Public.Models;
using Smo.Common.Public.Repositories;
using Smo.Reader.Standard;
using SharpPcap;
using SmoReader;

namespace Smo.Startup
{
    public class ConversionBatchStatus
    {
        public int TotalGroupCount { get; set; }
        public int TotalFileCount { get; set; }

        public int ProcessedFileCount { get; set; }

        public int ProcessedGroupCount { get; set; }
    }

    public class Converter
    {

        public static BatchConverter BuildBatchConverter(IAircraftDataProvider aircraftDataProvider, string instrumentSettingsXmlPath)
        {
            var iocBootstrapper = new InjectorBootstraper(aircraftDataProvider, instrumentSettingsXmlPath);

            var launcher = iocBootstrapper.GetService(typeof(BatchConverter)) as BatchConverter;

            return launcher;
        }

        public static LiveConverter BuildLiveConverter(
            string aircraftName,
            string configXmlPath,
            string scalingTsvPath,
            string instrumentSettingsXmlPath)
        {
            var liveAircraftDataProvider = new LiveAircraftDataProvider(aircraftName, configXmlPath, scalingTsvPath);

            var converter = new LiveConverter(aircraftName, liveAircraftDataProvider, instrumentSettingsXmlPath);

            return converter;
        }

    }


    public class LiveConverter
    {
        PacketDecoder _decoder { get; set; }

        string _aircraftName { get; set; }

        public LiveConverter(string aircraftName, IAircraftDataProvider aircraftDataProvider, string instrumentSettingsXmlPath)
        {
            var cfgParser = new ConfigParser(aircraftDataProvider, instrumentSettingsXmlPath);
            var parameterDefinitions = cfgParser.ReadConfigurationXml(aircraftName, DateTime.Now);
            _decoder = new PacketDecoder(parameterDefinitions, aircraftDataProvider);
        }

        public PacketReadResult DecodePacket(RawCapture packet)
        {
            var result = _decoder.DecodePacket(packet, _aircraftName);
            return result;
        }
    }

    public class BatchConverter
    {
        private readonly IFilePersistence _filePersistence;
        private readonly IAircraftDataProvider _aircraftDataProvider;
        IGlobalPaths _globalPaths;
        private readonly ILoggingService _loggingService;

        public ConversionBatchStatus ConversionBatchStatus = new ConversionBatchStatus();
        public AnalysisBatchStatus AnalysisBatchStatus = new AnalysisBatchStatus();

        public BatchConverter(
            IFilePersistence filePersistence,

            IAircraftDataProvider aircraftRepository,
            IGlobalPaths globalPaths,
            ILoggingService loggingService)
        {
            _filePersistence = filePersistence;
            _aircraftDataProvider = aircraftRepository;
            _globalPaths = globalPaths;
            _loggingService = loggingService;
        }


        public FileGroupScanResult ScanFiles(List<string> inputPaths)
        {

            var fileGroupScanner = new FileGroupScanner(_aircraftDataProvider, _globalPaths, _loggingService);
            var fileGroupScanResult = fileGroupScanner.Scan(inputPaths);

            return fileGroupScanResult;

        }

        public async Task<ConversionBatchResult> StartConversion(ConversionLaunchCommand command)
        {

            ConversionBatchStatus.TotalGroupCount = command.ScannedFileGroups.Count;
            ConversionBatchStatus.TotalFileCount = command.ScannedFileGroups.SelectMany(fpr => fpr.Files).Count();

            Trace.TraceInformation($"Scanning Finished. Found {ConversionBatchStatus.TotalFileCount} files in {ConversionBatchStatus.TotalGroupCount} groups");

            var fileMerger = new FileMerger(
                _aircraftDataProvider,
                _globalPaths);

            fileMerger.cutoffDate = command.EarliestDate;

            var extractedRecords = new ConcurrentBag<RecordModel>();
            var readFileMetadatas = new ConcurrentBag<FileReadMetadata>();

            await AsyncHelper.AsyncParallelForEach(command.ScannedFileGroups,
                async (group) =>
                 {
                     _loggingService?.Log("Starting Parallel Scan");
                     var fileGroupReader = new FileGroupReader(_aircraftDataProvider, _globalPaths, _loggingService);
                     var fileReadResults = fileGroupReader.ReadFiles(group.Files);

                     fileReadResults.ForEach(fr => readFileMetadatas.Add(fr.Metadata));

                     var mergeResult = fileMerger.MergeFiles(fileReadResults);



                     //now analyze 
                     var analysisResult = new AnalysisEngine(_aircraftDataProvider, _loggingService).Analyze(mergeResult);

                     if (analysisResult.isValidForAnalysis || command.letSmallFilesThrough)
                     {
                         //write tsv
                         var tsvWriter = new TsvWriter(command.TsvOutputFolder, _loggingService);
                         var tsvPath = tsvWriter.WriteTsv(analysisResult);

                         ConversionBatchStatus.ProcessedGroupCount++;

                         Trace.TraceInformation($"Converted {ConversionBatchStatus.ProcessedGroupCount}/{ConversionBatchStatus.TotalGroupCount} groups");

                         var extractedRecord = new RecordModel()
                         {
                             AircraftName = analysisResult.Metadata.AircraftName,
                             MetaData = analysisResult.Metadata,
                             InputFiles = group.Files.Select(f => f.ReadMetadata).ToList(),
                             TsvPath = tsvPath,
                             MaxValues = analysisResult.MaxValues,
                             MinValues = analysisResult.MinValues
                         };

                         extractedRecords.Add(extractedRecord);

                     }

                     await Task.Yield();
                 },
                8, TaskScheduler.Current);

            //GC.Collect();

            Trace.TraceInformation($"Final Extracted group number: {ConversionBatchStatus.ProcessedGroupCount} of {ConversionBatchStatus.TotalGroupCount} potential groups");

            //this is for better logging of file read results into the db
            var inputFiles = command.ScannedFileGroups.SelectMany(g => g.Files.Select(f => f.ReadMetadata)).ToList();
            var readFiles = readFileMetadatas.ToList();

            var updatedInputFiles = new List<FileReadMetadata>();

            inputFiles.ForEach(f =>
            {
                var match = readFiles.FirstOrDefault(r => r.FilePath == f.FilePath);

                if (match?.IsReadSuccessful ?? false)
                {
                    f = match;
                }
                else if (match != null)
                {
                    f.Flags = match.Flags;
                    f.IsReadSuccessful = match.IsReadSuccessful;
                }

                updatedInputFiles.Add(f);
            });

            var allExtractedMetadatas = extractedRecords.SelectMany(r => r.InputFiles);


            return new ConversionBatchResult
            {
                ConverterVersion = GetVersion(),
                Records = extractedRecords.ToList(),
                OrphanedFiles = updatedInputFiles
                .Where(ifm => !allExtractedMetadatas.Any(fm => fm.FilePath == ifm.FilePath))
                .ToList()
            };
        }

        public string GetVersion()
        {
            return Assembly.GetAssembly(typeof(FileMerger)).GetName().Version.ToString();
        }
    }

}
