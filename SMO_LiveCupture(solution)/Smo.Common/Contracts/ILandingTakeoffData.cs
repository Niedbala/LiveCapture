using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Contracts
{
    public interface ILandingTakeoffData
    {
        List<int> LandingIndices { get; set; }
        List<int> TakeOffIndices { get; set; }
        List<int> TouchAndGoIndices { get; set; }
    }
}
