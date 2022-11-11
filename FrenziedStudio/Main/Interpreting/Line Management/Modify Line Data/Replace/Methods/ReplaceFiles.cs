using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public class ReplaceFiles
    {
        public InterpretedProject _project { get; set; }

        public ReplaceFiles(InterpretedProject _project)
        {
            this._project = _project;
        }

        /// <summary>
        /// Used by Replace method classes to identify method patterns
        /// </summary>
        private Random MethodRandom = new Random();
        /// <summary>
        /// Replaces regex method pattern with name of method
        /// </summary>
        /// <returns>
        /// Returns whether CurrentActionValue contained method
        /// </returns>
        private string ReplaceMethodPattern(string MethodName)
        {
            string MethodPattern = @"{{(.*?)->(.*?)}}";

            string MethodNamePattern = "{{" + MethodName + "->";

            bool ContainsMethod = _project.CurrentActionValue.Contains(MethodNamePattern);

            if (ContainsMethod)
            {
                MethodPattern = MethodPattern.Replace("{{(.*?)->", MethodNamePattern);

                return MethodPattern;
            }
            else
                return string.Empty;
        }

        public void ReplaceFileWithName()
        {
            string MethodPattern = ReplaceMethodPattern("RandomFile");

            if (!string.IsNullOrEmpty(MethodPattern))
            {
                string Folder = Regex.Match(_project.CurrentActionValue, MethodPattern).Groups[1].Value;

                var FolderFiles = Directory
                .GetFiles(Folder, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetFileName(f));

                _project.CurrentActionValue = Regex.Replace(_project.CurrentActionValue, MethodPattern, 
                    FolderFiles.ElementAt(MethodRandom.Next(0, FolderFiles.Count())));
            }
        }
        
        public void ReplaceFileWithPath()
        {
            string MethodPattern = ReplaceMethodPattern("RandomFilePath");

            if (!string.IsNullOrEmpty(MethodPattern))
            {
                string Folder = Regex.Match(_project.CurrentActionValue, MethodPattern).Groups[1].Value;

                var FolderFiles = Directory.GetFiles(Folder);

                string RandomFile = FolderFiles[MethodRandom.Next(FolderFiles.Length)];

                _project.CurrentActionValue = Regex.Replace(_project.CurrentActionValue, MethodPattern, RandomFile);
            }
        }
    }
}
