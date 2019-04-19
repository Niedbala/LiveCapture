using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoReader.Utils
{
    public static  class MathUtils
    {
        public static double Median(double[] input)
        {
            var xs = new double[input.Length];

            input.CopyTo(xs, 0);

            Array.Sort(xs);
            return xs[xs.Length / 2];
        }

        public static IEnumerable<Tuple<int, double>> LocalMaxima(IEnumerable<double> source, int windowSize)
        {
            // Round up to nearest odd value
            windowSize = windowSize - windowSize % 2 + 1;
            int halfWindow = windowSize / 2;

            int index = 0;
            var before = new Queue<double>(Enumerable.Repeat(double.NegativeInfinity, halfWindow));
            var after = new Queue<double>(source.Take(halfWindow + 1));

            foreach (double d in source.Skip(halfWindow + 1).Concat(Enumerable.Repeat(double.NegativeInfinity, halfWindow + 1)))
            {
                double curVal = after.Dequeue();
                if (before.All(x => curVal > x) && after.All(x => curVal >= x))
                {
                    yield return Tuple.Create(index, curVal);
                }

                before.Dequeue();
                before.Enqueue(curVal);
                after.Enqueue(d);
                index++;
            }
        }

        public static double[] LowPass(List<double> input, double deltaTimeInSec)
        {

            var outputSamples = Filtering.Butterworth(input.ToArray(), deltaTimeInSec, (1 / 5.0));
            return outputSamples;

        }

        public static List<double> WindowDifferentiate(List<double> inputSignal)
        {


            var windowDelay = 150;

            var processedSamples = new List<double>();

            for (int i = 0; i < inputSignal.Count(); i++)
            {
                var pastIndex = (i - windowDelay);

                if (pastIndex < 0)
                    processedSamples.Add(0);

                else
                {
                    var curr = inputSignal[i];
                    var past = inputSignal[pastIndex];
                    var res = curr - past;
                    processedSamples.Add(res);
                }

            }

            return processedSamples;
        }

        public static IList<int> FindPeaks(IList<double> values, int window)
        {
            List<int> peaks = new List<int>();
            double current;
            IEnumerable<double> range;

            int checksOnEachSide = window / 2;
            for (int i = 0; i < values.Count; i++)
            {
                current = values[i];
                range = values;

                if (i > checksOnEachSide)
                {
                    range = range.Skip(i - checksOnEachSide);
                }

                range = range.Take(window);
                if ((range.Count() > 0) && (current == range.Min()))
                {
                    peaks.Add(i);
                }
            }

            return peaks;
        }

        public static IList<double> AverageSignals(List<List<double>> signalList)
        {
            var signalLength = signalList.First().Count;
            var signalCount = signalList.Count;
            var returnList = new List<double>();

            if (signalList.Any(s=> s.Count != signalLength))
                throw new Exception("Signals for averaging are not the same length");

            for (int i=0; i < signalLength; i++)
            {
               var averageSample = (signalList.Select(s => s[i]).Sum())/signalCount;
                returnList.Add(averageSample);
            }

            return returnList;

        }
    }




}
