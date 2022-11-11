using ExternalSel;
using FrenziedStudio.External;
using SharpSpin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public class ReplaceDynamics
    {
        public Automation _automationType { get; set; }
        public InterpretedProject _project { get; set; }
        public ReplaceFiles _replaceFiles { get; set; }
        public ReplaceBrowser _replaceBrowser { get; set; }

        public ReplaceDynamics(InterpretedProject _project, Automation _automationType) { 
            this._project = _project; 
            _replaceFiles = new ReplaceFiles(_project);
            _replaceBrowser = new ReplaceBrowser(_project, _automationType);
        }

        /// <summary>
        /// Implementation of Spintax within ActionValue
        /// Example in [Settings]: AccountLink=Spin:{google.com|lool.com}
        /// </summary>
        public void ReplaceSpintaxWithValue()
        {
            Match SpintaxRegex = Regex.Match(_project.CurrentActionValue, @"(?:Spin:)({[^{}]*})");

            if (SpintaxRegex.Success)
            {
                string Spintax = SpintaxRegex.Groups[1].Value;

                _project.CurrentActionValue = _project.CurrentActionValue.Replace(Spintax, Spinner.Spin(Spintax)).Replace("Spin:", "");
            }
        }

        /// <summary>
        /// Replaces a URL given with the value for it.
        /// Safereq, Hidereq or Normalreq must be used
        /// Example: setvariable={{hidereq:http://test.com?two={{List}}->newvariable}}
        /// </summary>
        public void ReplaceUrlWithValue()
        {
            InterpretedLine iu = new InterpretedLine(_project.CurrentActionValue);

            if (iu.HasUrl)
            {
                string UrlValue = InterpretedLineActions.SendRequest(iu);

                _project.LastURLValue = UrlValue;

                _project.CurrentActionValue = _project.CurrentActionValue.Replace(iu.FullUrl, UrlValue);
            }
        }

        /// <summary>
        /// Replaces & returns common variables.
        /// Example: {{RandFirstName}}
        /// </summary>
        /// <param name="_project.CurrentActionValue"></param>
        /// <returns>Returns value of common variable.</returns>
        public string ReplaceCommonVariableWithValue()
        {
            string CommonValue = string.Empty;

            string[] CommonVariables = {"RandFirstName", "RandFirstNameMale", "RandFirstNameFemale", "RandLastName",
            "RandPassword", "RandPopularEmailDomain", "RandNumber", "RandLetter", "RandRange", "IterationCount", "IterationTimeout" };

            ThreadLocal<Random> rnd = new ThreadLocal<Random>(() => new Random(Environment.TickCount * Thread.CurrentThread.ManagedThreadId));

            Randomizer randomizer = new Randomizer(rnd);

            // For each common variable from array above (CommonVariables).
            for (int varIndex = 0; varIndex < CommonVariables.Length; varIndex++)
            {
                string CommonVar = CommonVariables[varIndex];

                if (!_project.CurrentActionValue.Contains(CommonVar)) { continue; }

                switch (varIndex)
                {
                    case 0: CommonValue = randomizer.randFirstName(); break;
                    case 1: CommonValue = randomizer.randMaleName(); break;
                    case 2: CommonValue = randomizer.randFemaleName(); break;
                    case 3: CommonValue = randomizer.randSurname(); break;
                    case 4: CommonValue = randomizer.randPassword(); break;
                    case 5: CommonValue = randomizer.randHQEmailDomain(); break;
                    case 6: CommonValue = randomizer.randNumber(); break;
                    case 7: CommonValue = randomizer.randLetter(); break;
                    case 8:
                        Match rangeMatch = Regex.Match(_project.CurrentActionValue, @"{{RandRange:(.*?),(.*?)}}");
                        CommonValue = randomizer.randRange(Convert.ToInt32(rangeMatch.Groups[1].Value),
                            Convert.ToInt32(rangeMatch.Groups[2].Value));
                        CommonVar = rangeMatch.Value.Substring(2, rangeMatch.Value.Length - 4);

                        break;
                    case 9: CommonValue = _project.InternalLoopCount.ToString(); break;
                    case 10: CommonValue = _project.InternalLoopTimeout.ToString(); break;
                }

                _project.CurrentActionValue = _project.CurrentActionValue.Replace("{{" + CommonVar + "}}", CommonValue);
            }

            if (!string.IsNullOrEmpty(CommonValue)) { return CommonValue; }

            return string.Empty;
        }

        /// <summary>
        /// Replaces common methods
        /// Example: {{RandomFile->C:\Users\Drax\PhpstormProjects\FrenziedSMS}}
        /// </summary>
        /// <param name="_project.CurrentActionValue"></param>
        public async Task ReplaceMethodsWithValue()
        {
            _replaceFiles.ReplaceFileWithName();
            _replaceFiles.ReplaceFileWithPath();
            await _replaceBrowser.ReplacePageURL();
            await _replaceBrowser.ReplacePageHTML();
            await _replaceBrowser.ReplaceText();
            await _replaceBrowser.ReplaceElementCount();
            await _replaceBrowser.ReplaceAttribute();
        }
    }
}
