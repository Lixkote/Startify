using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace StartifyBackend.Helpers
{
    public class Logger
    {
        public static void LogError(string message)
        {
            string logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Startify", "Logs");
            string logFilePath = Path.Combine(logFolder, "StartifyLog.txt");

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            if (!File.Exists(logFilePath))
            {
                File.CreateText(logFilePath);
                StreamWriter sw = File.AppendText(logFilePath);
                sw.WriteLine($"[Error] {DateTime.Now}: {message}");
            }
            else
            {
                try
                {
                    StreamWriter sw = File.AppendText(logFilePath);
                    sw.WriteLine($"[Error] {DateTime.Now}: {message}");
                }
                catch
                {
                    Debug.WriteLine("Couldn't Make a log file.");
                }

            }
        }
    }

}
