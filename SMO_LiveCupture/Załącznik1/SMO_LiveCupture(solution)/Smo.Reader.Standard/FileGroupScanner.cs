using Smo.Common;
using Smo.Common.Infrastructure;
using Smo.Common.Models;
using SmoReader.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Smo.Common.Public.Repositories;

namespace SmoReader
{
    /// <summary>
    /// used for initial scan of files to be converted
    /// </summary>
    public class FileGroupScanner
    {
        private IAircraftDataProvider _aircraftDataProvider;
        private readonly ILoggingService _loggingService;

        IGlobalPaths _globalPaths { get; set; }

        public FileGroupScanner(           
            IAircraftDataProvider aircraftDataProvider,
            IGlobalPaths globalPaths,
            ILoggingService loggingService = null)
        {

            _aircraftDataProvider = aircraftDataProvider;
            _loggingService = loggingService;           
            _globalPaths = globalPaths;
        }

        public FileGroupScanResult Scan(List<string> filepaths)
        {

            var totalFileCount = 0;

            var processedFileGroups = new List<ScannedFileGroup>();

            var scannedFiles = filepaths.Select(fil => ScanFile(fil))
               .Where(fsr => fsr != null)
               .ToList();

            var invalidScannedFiles = scannedFiles.Where(sf => !sf.IsReadSuccessful || !sf.StartDate.HasValue || !sf.ReadMetadata.PowerUpCount.HasValue)
                .ToList();

            scannedFiles = scannedFiles.Except(invalidScannedFiles).ToList();

            var fileGroups = GroupScannedFiles(scannedFiles);

            processedFileGroups.AddRange(fileGroups.ToList());
            totalFileCount = totalFileCount + scannedFiles.Count;

            return new FileGroupScanResult()
            {
                FileGroupsValidForFurtherProcessing = processedFileGroups,
                TotalFileCount = totalFileCount,
                OrphanedFiles = invalidScannedFiles
            };
        }

        private DateTime _cutoffDate = new DateTime(1980, 1, 30);

        /// <summary>
        /// extract groups for merging
        /// </summary>
        /// <param name="scannedFiles"></param>
        /// <returns></returns>
        private List<ScannedFileGroup> GroupScannedFiles(List<FileScanResult> scannedFiles)
        {
            var FilesWithoutTimeData = scannedFiles.Where(sf => sf.StartDate == null);

            scannedFiles = scannedFiles.Except(FilesWithoutTimeData).ToList();

            //group by powerupcount,day and aircraft
            var grouped = scannedFiles.GroupBy(f => new Tuple<int, DateTime, string>(
                f.ReadMetadata.PowerUpCount.Value,
                new DateTime(f.StartDate.Value.Year, f.StartDate.Value.Month, f.StartDate.Value.Day),
                f.ReadMetadata.AircraftName
                ))
                .ToList();

            var fileGroups = grouped.Select(g =>
            {
                var fdir = new DirectoryInfo(System.IO.Path.GetDirectoryName(g.FirstOrDefault().FilePath)).Name;
                return new ScannedFileGroup()
                {
                    AircraftName = g.Key.Item3,
                    ContainingDirectory = fdir,
                    Files = g.Select(f => f).ToList(),
                  
                    PowerUpCount = g.Key.Item1

                };
            });

            return fileGroups.ToList();

        }

        private FileScanResult ScanFile(string filePath)
        {
            var fileinfo = new FileInfo(filePath);
            var ext = fileinfo.Extension;

            var filename = fileinfo.Name;

            if (filename.Length >= 9)
            {
                var fileNumber = filename.Substring(0, 4);
                var fileSubNumber = fileinfo.Name.Substring(5, 4);
            }


            //check file name validity                
            var isExtensionValid = ext == ".cap";

            if (!isExtensionValid)
                return null;

            var reader = new FileReader(_aircraftDataProvider, new ConfigParser(_aircraftDataProvider, _globalPaths), _loggingService);

            var preliminaryResult = reader.PreliminaryReadFile(filePath);

            var fileScanResult = new FileScanResult(filePath)
            {                
                ReadMetadata = preliminaryResult.ReadMetadata,
                IsReadSuccessful = preliminaryResult.IsReadSuccessful
            };

            

            return fileScanResult;
        }

    }
}
