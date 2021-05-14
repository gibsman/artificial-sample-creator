using System;
using System.Collections.Generic;
using System.Text;

namespace ArtificialSampleCreator
{
    public class SampleFile
    {
        public List<double[]> Datapoints { get; set; }

        public SampleFile(int datapointCount, int datapointValueCount)
        {
            Datapoints = new List<double[]>();
            for (int i = 0; i < datapointCount; i++)
            {
                Datapoints.Add(new double[datapointValueCount]);
            }
        }

        public SampleFile(List<double[]> datapoints)
        {
            Datapoints = datapoints;
        }

        public void AverageDatapoints(int divisor)
        {
            foreach (double[] datapoint in Datapoints)
            {
                for (int i = 0; i < datapoint.Length; i++)
                {
                    datapoint[i] = Math.Round(datapoint[i] / divisor, 5);
                }
            }
        }
    }
}
