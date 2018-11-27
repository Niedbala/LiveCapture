using System.Collections.Generic;

namespace Smo.Common.Public.Models
{
    public class ConversionBatchResult
    {
        public ConversionBatchResult()
        {
            Records = new List<RecordModel>();
        }
        public List<RecordModel> Records { get; set; }
        public string ConverterVersion { get; set; }
        public List<FileReadMetadata> OrphanedFiles { get; set; }
    }

}
