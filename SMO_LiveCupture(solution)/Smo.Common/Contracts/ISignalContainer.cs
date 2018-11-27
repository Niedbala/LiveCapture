using SmoReader.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smo.Common.Contracts
{
    public interface ISignalContainer 
    {
        List<SignalData> Signals { get; set; }

        SignalData GetSignal(string name);

        double[] GetSamples(string name);

        void SetSamples(ValueType[] samples, string name);

        void AddSignal(ValueType[] samples, string name);

        void AddSignal(List<ValueType> samples, string name);

    }
}
