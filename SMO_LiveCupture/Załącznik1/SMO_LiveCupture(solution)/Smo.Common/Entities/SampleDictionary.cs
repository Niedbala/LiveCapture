using System.Collections.Generic;

namespace SmoReader.Entities
{
    public class SampleDictionary : Dictionary<string,List<object>>
    {
        public void AddSample<T>(KeyValuePair<string, T> sample)
        {
            if (!this.ContainsKey(sample.Key))
                this.Add(sample.Key, new List<object>());
            this[sample.Key].Add((object)sample.Value);
        }
    }
}