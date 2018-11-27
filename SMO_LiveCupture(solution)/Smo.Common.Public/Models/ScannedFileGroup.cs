using System.Collections.Generic;
using SmoReader.Entities;
using System;
using System.Linq;

namespace SmoReader
{
    [Serializable]
    //group of files that may be a single record after merge that are a result of initial raw file scan
    public class ScannedFileGroup
    {
        public string AircraftName { get; set; }
        public string ContainingDirectory { get; set; }
        
        public List<FileScanResult> Files { get; set; }

        public int PowerUpCount { get; set; }

        public DateTime StartDate => Files.Where(f=>f.StartDate.HasValue).Min(f => f.StartDate.Value);
    }
}