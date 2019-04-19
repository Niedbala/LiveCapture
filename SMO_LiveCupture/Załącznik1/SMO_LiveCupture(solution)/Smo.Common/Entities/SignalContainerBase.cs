using Smo.Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmoReader.Entities;

namespace Smo.Common.Entities
{
    public class DataContainerBase : ISignalContainer, ITimeData
    {
        public List<SignalData> Signals
        {
            get; set;
        }

        public SignalData GetSignal(string name)
        {
            return Signals.FirstOrDefault(s => s.Name.Contains(name));

        }
        public double[] GetSamples(string name)
        {
            var signal = GetSignal(name);
            return signal.Samples.Select(Convert.ToDouble).ToArray();
        }
        public void SetSamples(ValueType[] samples, string name)
        {
            var signal = Signals.FirstOrDefault(s => s.Name.Contains(name));
            var whereToPut = Signals.IndexOf(signal);
            signal = new SignalData()
            {
                Samples = samples.ToArray(),
                Name = signal.Name
            };

            Signals[whereToPut] = signal;

        }
        public void AddSignal(ValueType[] samples, string name)
        {
            AddSignal(samples.ToList(), name);

        }
        public void AddSignal(List<ValueType> samples, string name)
        {
            var samplearray = samples.ToArray();

            var signal = new SignalData()
            {
                Name = name,
                Samples = samplearray
            };
            Signals.Add(signal);

        }
        
        protected T[] GetSubArray<T>(T[] origSignal, int startIndex, int endIndex)
        {
            var subArrayLength = endIndex - startIndex;
            var subSamples = new T[subArrayLength];

            Array.Copy(origSignal, startIndex, subSamples, 0, subArrayLength);

            return subSamples;
        }

        protected List<SignalData> GetSignalsForTimeRange(int startIndex, int endIndex)
        {

            var subSignals = new List<SignalData>();

            subSignals = Signals.Select(origSignal =>
            {
                var subSamples = GetSubArray(origSignal.Samples, startIndex, endIndex);

                var subSignal = new SignalData()
                {
                    Name = origSignal.Name,
                    Samples = subSamples
                };

                return subSignal;
            }).ToList();

            return subSignals;
        }

        public DateTime? AcraStartTime
        {
            get
            {
                //var timeSignal = Signals.FirstOrDefault(s => s.Name.Contains("acraTime"));

                // var minTime = UnixTimeStampToDateTime((long) timeSignal.Samples.Min());
                var minTime = AcraTime?.Min();
                return minTime;
            }
        }
        public DateTime? AcraEndTime
        {
            get
            {
                //var timeSignal = Signals.FirstOrDefault(s => s.Name.Contains("acraTime"));

                //var minTime = UnixTimeStampToDateTime((long)timeSignal.Samples.Max());
                var minTime = AcraTime.Max();
                return minTime;
            }
        }

        public DateTime[] AcraTime { get; set; }

        public DateTime[] EthernetTime { get; set; }

        DateTime? ITimeData.AcraStartTime
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        DateTime? ITimeData.AcraEndTime
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime? EthernetStartTime { get; set; }

        public DateTime? EthernetEndTime { get; set; }
    }
}
