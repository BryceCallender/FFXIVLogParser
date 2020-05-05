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
            //string path = @"C:\Users\bryce\AppData\Roaming\Advanced Combat Tracker\FFXIVLogs\Network_20510_20200425.log";
            //string path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist");
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Parser parser = new Parser();

            //DateTime startTime = DateTime.Parse("2020-04-25T23:07:48.2930000-07:00");
            //DateTime endTime = DateTime.Parse("2020-04-25T23:10:07.1000000-07:00");

            //Console.WriteLine($"Fight took {endTime.Subtract(startTime)}");

            Console.Write("Parsing your report...");
            using (var progress = new ProgressBar())
            {
                int totalLines = File.ReadAllLines("Network_20510_20200425.log").Length;
                int lineCount = 0;

                using (StreamReader streamReader = File.OpenText(path))
                {
                    string line;
                    uint currentHp = 54990880;

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
                            
                            if((LogMessageType)logMessageType == LogMessageType.AddCombatant || (LogMessageType)logMessageType == LogMessageType.RemoveCombatant)
                            {
                                Debug.WriteLine(line);
                            }

                            //if ((LogMessageType)logMessageType == LogMessageType.NetworkEffectResult && values[3] == "Ramuh")
                            //{
                            //    uint.TryParse(values[5], out uint newHp);
                            //    uint.TryParse(values[6], out uint maxHp);

                            //    uint damage = currentHp - newHp;

                            //    //Debug.WriteLine($"Ramuh took {currentHp - newHp} damage");

                            //    currentHp -= damage;

                            //    Debug.WriteLine(line);
                            //}
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
