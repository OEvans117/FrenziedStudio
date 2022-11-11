using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ExternalSel
{
    public class TextWriter
    {
        public static class LoggingExtensions
        {
            static ReaderWriterLock locker = new ReaderWriterLock();
            public static void WriteDebug(string pathFile, string text)
            {
                try
                {
                    locker.AcquireWriterLock(int.MaxValue);
                    File.AppendAllText(pathFile, text + Environment.NewLine);
                }
                finally
                {
                    locker.ReleaseWriterLock();
                }
            }
            public static void ReWriteDebug(string pathFile, string text)
            {
                try
                {
                    locker.AcquireWriterLock(int.MaxValue);
                    File.WriteAllText(pathFile, text);
                }
                finally
                {
                    locker.ReleaseWriterLock();
                }
            }
            public static void WriteLinesDebug(string pathFile, List<string> text)
            {
                try
                {
                    locker.AcquireWriterLock(int.MaxValue);
                    File.WriteAllLines(pathFile, text);
                }
                finally
                {
                    locker.ReleaseWriterLock();
                }
            }
            public static List<string> ReadDebug(string pathFile)
            {
                try
                {
                    locker.AcquireWriterLock(int.MaxValue);
                    return File.ReadAllLines(pathFile).ToList();
                }
                finally
                {
                    locker.ReleaseWriterLock();
                }
            }
            public static void DeleteLine(string path, string line)
            {
                List<string> fileContents = ReadDebug(path);
                foreach (string file in fileContents){if (file == line){fileContents.Remove(file);break;}}
                WriteLinesDebug(path, fileContents);
            }
        }
    }
}
