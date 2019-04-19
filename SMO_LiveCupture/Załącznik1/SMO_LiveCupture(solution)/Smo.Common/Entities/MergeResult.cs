using Smo.Common;
using Smo.Common.Contracts;
using Smo.Common.Entities;
using Smo.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader.Entities
{
    /// <summary>
    /// Merged conversion result from multiple cap files
    /// </summary>
    [Serializable]
    public class MergeResult : DataContainerBase, IHasMetadata<RecordMetaData>, IFileGroupBased
    {

        public RecordMetaData Metadata
        { get; set; }

        public List<FileReadMetadata> FileSummaries
        { get; set; }



    }
}
