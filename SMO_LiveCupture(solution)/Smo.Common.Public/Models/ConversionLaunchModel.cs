using System;
using System.Collections.Generic;
using SmoReader;

namespace Smo.Common.Public.Models
{
    public class ConversionLaunchCommand
    {

        public ConversionLaunchCommand(List<ScannedFileGroup> scannedFileGroups)
        {
            ScannedFileGroups = scannedFileGroups;
        }

        public DateTime EarliestDate { get; set; }

        public string TsvOutputFolder { get; set; }

        public List<ScannedFileGroup> ScannedFileGroups { get; private set; }

        /// <summary>
        /// used for testing. dont touch
        /// </summary>
        public bool letSmallFilesThrough = false;

    }
}
