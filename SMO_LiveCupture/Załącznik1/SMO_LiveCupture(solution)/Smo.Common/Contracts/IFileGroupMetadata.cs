using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Contracts
{
    public interface IFileGroupBased
    {
        List<FileReadMetadata> FileSummaries { get; set; }
      
    }
}
