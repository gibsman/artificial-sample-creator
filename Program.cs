using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ArtificialSampleCreator
{
    class Program
    {
        private static ILogger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Logger.Info("Application initialized");

            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

            string[] sampleFiles = Directory.GetFiles(appSettings.InputPath);
            List<SampleFile> sampleFileList = new List<SampleFile>();
            foreach (string sampleFile in sampleFiles)
            {
                string[] dataLines = File.ReadAllLines(sampleFile);
                List<double[]> inputDataValues = ParseDataStringsIntoNumList(dataLines, appSettings.DatapointValueCount);

                sampleFileList.Add(new SampleFile(inputDataValues));
                Logger.Info("Acquired data from {0}", sampleFile);
            }

            SampleFile averageSampleFile = new SampleFile(sampleFileList[0].Datapoints.Count, appSettings.DatapointValueCount);

            foreach (SampleFile sampleFile in sampleFileList)
            {
                for (int i = 0; i < sampleFile.Datapoints.Count; i++)
                {
                    for (int j = 0; j < appSettings.DatapointValueCount; j++)
                    {
                        averageSampleFile.Datapoints[i][j] += sampleFile.Datapoints[i][j];
                    }
                }
            }

            averageSampleFile.AverageDatapoints(sampleFileList.Count);

            string averageFilePath = Path.Combine(appSettings.OutputPath, "average_sample.txt");

            WriteSampleFile(averageSampleFile, averageFilePath);
            Logger.Info("Average data values written to file {0}", averageFilePath);

            int finishedFiles = 0;
            Random rand = new Random();
            Parallel.For(0, appSettings.OutputFileCount, (i, loopState) =>
            {
                SampleFile randomSample = sampleFileList[rand.Next(sampleFileList.Count)];
                SampleFile artificialSampleFile = GenerateArtificialSampleFile(randomSample, averageSampleFile,
                    appSettings.DatapointValueCount, appSettings.AverageMultiplier);

                string pathToArtificialSample = Path.Combine(appSettings.OutputPath, $"artficial_sample{i}.txt");
                WriteSampleFile(artificialSampleFile, pathToArtificialSample);

                int curFinishedFiles = Interlocked.Increment(ref finishedFiles);
                double donePercentage = Math.Round(((double)curFinishedFiles / (double)appSettings.OutputFileCount) * 100, 2);
                Logger.Info("Artificial file {0} generated ({1}% done)", pathToArtificialSample, donePercentage);
            });

            Logger.Info("All {0} artificial samples successfully generated", appSettings.OutputFileCount);
            Logger.Info("Program shutdown");
            Console.ReadLine();
        }

        private static SampleFile GenerateArtificialSampleFile(SampleFile randomSample, SampleFile averageSampleFile, 
            int datapointValueCount, double averageMultiplier)
        {
            Random rand = new Random();
            SampleFile artificialSampleFile = new SampleFile(randomSample.Datapoints.Count, datapointValueCount);

            for (int i = 0; i < randomSample.Datapoints.Count; i++)
            {
                for (int j = 0; j < randomSample.Datapoints[i].Length; j++)
                {
                    double absAverageDatapointValue = Math.Abs(averageSampleFile.Datapoints[i][j]);
                    artificialSampleFile.Datapoints[i][j] = randomSample.Datapoints[i][j] + 
                        (-absAverageDatapointValue + 2 * rand.NextDouble() * absAverageDatapointValue) * averageMultiplier;

                    artificialSampleFile.Datapoints[i][j] = Math.Round(artificialSampleFile.Datapoints[i][j], 5);
                }
            }

            return artificialSampleFile;
        }

        private static List<double[]> ParseDataStringsIntoNumList(string[] dataLines, int dataArrayLength)
        {
            List<double[]> inputDataValues = new List<double[]>();
            foreach (string dataLine in dataLines)
            {
                string[] curDataParts = dataLine.Split('|');
                inputDataValues.Add(new double[dataArrayLength]);
                for (int i = 0; i < dataArrayLength; i++)
                {
                    curDataParts[i] = curDataParts[i].Replace(',', '.');

                    inputDataValues[inputDataValues.Count - 1][i] = Math.Round(double.Parse(curDataParts[i]), 5);
                }
            }

            return inputDataValues;
        }

        private static void WriteSampleFile(SampleFile sampleFile, string outputFilePath)
        {
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            string[] dataLines = new string[sampleFile.Datapoints.Count];
            for (int i = 0; i < sampleFile.Datapoints.Count; i++)
            {
                string datapointStr = "";
                for (int j = 0; j < sampleFile.Datapoints[i].Length; j++)
                {
                    datapointStr += sampleFile.Datapoints[i][j].ToString() + '|';
                }
                dataLines[i] = datapointStr;
            }
            File.AppendAllLines(outputFilePath, dataLines);
        }
    }
}
