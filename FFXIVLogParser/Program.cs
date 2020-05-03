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
            //string path = @"C:\Users\bryce\AppData\Roaming\Advanced Combat Tracker\FFXIVLogs\Network_20510_20200425.log";
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
                int totalLines = File.ReadAllLines("Network_20510_20200425.log").Length;
                int lineCount = 0;

                using (StreamReader streamReader = File.OpenText(path))
                {
                    string line;

                    using (StreamWriter streamWriter = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileNameWithoutExtension(path) + ".txt")))
                    {
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            lineCount++;

                            progress.Report((double)lineCount / totalLines);

                            parser.ParseLine(line);

                            string[] values = line.Split('|');


                            int.TryParse(values[0], out int logMessageType);
                            streamWriter.WriteLine((LogMessageType)logMessageType);

                            uint currentHp = 54990880;

                            if ((LogMessageType)logMessageType == LogMessageType.NetworkEffectResult && values[3] == "Ramuh")
                            {
                                uint.TryParse(values[5], out uint newHp);
                                uint.TryParse(values[6], out uint maxHp);

                                Debug.WriteLine($"Ramuh took {currentHp - newHp} damage");

                                currentHp = newHp;

                                //Debug.WriteLine(line);
                            }
                        }
                    }

                    //Thread.Sleep(20);
                }
            }

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
