﻿using System;
using System.Diagnostics;
using System.IO;

namespace FFXIVLogParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to FFXIV Log Parser!");
            Console.WriteLine("Enter in the path to the log you want parsed: ");

            string path = @"C:\Users\bryce\AppData\Roaming\Advanced Combat Tracker\FFXIVLogs\Network_20510_20200425.log";
            //string path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist");
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Parser parser = new Parser();

            using (StreamReader streamReader = File.OpenText(path))
            {
                string line;

                using (StreamWriter streamWriter = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileNameWithoutExtension(path) + ".txt")))
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        parser.ParseLine(line);


                        int.TryParse(line.Split('|')[0], out int logMessageType);
                        streamWriter.WriteLine((LogMessageType)logMessageType);

                        if((LogMessageType)logMessageType == LogMessageType.NetworkAbility && line.Split('|')[7] == "Ramuh")
                        {
                            Debug.WriteLine(line);
                        }
                    }
                }
            }

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            Console.Read();
        }
    }
}
