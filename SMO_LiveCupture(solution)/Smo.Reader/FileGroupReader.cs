using Smo.Common;
using Smo.Common.Infrastructure;
using Smo.Common.Public.Models;
using Smo.Common.Public.Repositories;
using SmoReader.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader
{
    public class FileGroupReader
    {
        public FileGroupReader(
            IAircraftDataProvider aircraftDataProvider,
            IGlobalPaths globalPaths,
            ILoggingService loggingService = null)
        {
            _aircraftDataProvider = aircraftDataProvider;
            _globalPaths = globalPaths;
            _loggingService = loggingService;

            _configParser = new ConfigParser(_aircraftDataProvider, _globalPaths.InstrumentSettingsXml);
        }

        private IAircraftDataProvider _aircraftDataProvider;
        private ILoggingService _loggingService;
        private IGlobalPaths _globalPaths;
        private ConfigParser _configParser;

        public List<FileReadResult> ReadFiles(List<FileScanResult> files)
        {
            
            var readResults = files.Select(f =>
            {
                var reader = new FileReader(

                    _aircraftDataProvider, _configParser,
                    _loggingService);

                var readResult = reader.ReadFile(f.FilePath, f.ReadMetadata.AircraftName, f.StartDate);

              //  GC.Collect();
                return readResult;

            }).Where(f => f != null).ToList();

            readResults = readResults
              .Where(fr => fr != null)             
              .ToList();

            return readResults;
        }
    }
}
