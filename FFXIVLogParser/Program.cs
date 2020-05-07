using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FFXIVLogParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to FFXIV Log Parser!");
            Console.WriteLine("Enter in the path to the log you want parsed: ");

            string path = "Network_20510_20200425.log";
            //string path = @"C:\Users\bryce\Desktop\FFXIVLogParser\FFXIVLogParser\Ramuh_Encounter_1.txt";
            //string path = @"C:\Users\bryce\AppData\Roaming\Advanced Combat Tracker\FFXIVLogs\Network_2050_20200208.log";
            //string path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist");
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Parser parser = new Parser();

            Console.Write("Parsing your report...");
            using (var progress = new ProgressBar())
            {
                int totalLines = File.ReadAllLines(path).Length;
                int lineCount = 0;

                string line;

                using StreamReader streamReader = File.OpenText(path);
                using StreamWriter streamWriter = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileNameWithoutExtension(path) + ".txt"));

                while ((line = streamReader.ReadLine()) != null)
                {
                    lineCount++;

                    progress.Report((double)lineCount / totalLines);

                    parser.ParseLine(line);
                }
            }

            Debug.WriteLine(parser.encounters.Count);

            Console.WriteLine("Done.");

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Elapsed time " + elapsedTime);
        }
    }
}
