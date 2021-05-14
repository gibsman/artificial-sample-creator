using System;
using System.Collections.Generic;
using System.Text;

namespace ArtificialSampleCreator
{
    public class AppSettings
    {
        public string InputPath { get; set; }

        public string OutputPath { get; set; }

        public int DatapointValueCount { get; set; }

        public int OutputFileCount { get; set; }

        public double AverageMultiplier { get; set; }
    }
}
