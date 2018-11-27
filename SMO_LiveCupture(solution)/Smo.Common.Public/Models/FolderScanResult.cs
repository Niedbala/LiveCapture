using SmoReader;
using SmoReader.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Models
{
    public class FileGroupScanResult
    {
        public List<ScannedFileGroup> FileGroupsValidForFurtherProcessing { get; set; }
        public int TotalFileCount { get; set; }

        public List<FileScanResult> OrphanedFiles { get; set; }
    }
}
