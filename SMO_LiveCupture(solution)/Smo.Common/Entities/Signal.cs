using System;

namespace SmoReader.Entities
{
    [Serializable]
    public class SignalData
    {
        public string Name;
        public ValueType[] Samples;
        public override string ToString() { return Name; }
    }
}