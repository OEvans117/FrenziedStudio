using FrenziedStudio.Interpretation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    class LoopMethods : MethodManager
    {
        public LoopMethods(InterpretedProject pr, ReplacementManager rm, Automation at) : base(pr, rm, at) { }

        public override void ForLoop(ref LoopType lType, List<Tuple<string, object>> LocalSettings)
        {
            lType = LoopType.For;

            int IterationCount = ReturnSettingData.ReturnSettingLength(_project.CurrentActionValue, LocalSettings, _project);

            _project.InternalLoopTimeout = IterationCount;

            if (IterationCount > 0)
            {
                _project.Loop = true;
                _project.LoopIndex = _project.CurrentLineIndex;
            }
            else
                _project.Loop = false;
        }

        /// <summary>
        /// While loop. Examples:
        /// for=1->notequals->1->50
        /// </summary>
        /// <param name="lType"></param>
        /// <param name="Try"></param>
        public override void WhileLoop(ref LoopType lType, bool Try)
        {
            lType = LoopType.While;

            _replacementManager._replaceDynamics.ReplaceUrlWithValue();

            string IdentifierValue = Regex.Split(_project.CurrentActionValue, "->").First();
            _project.InternalLoopTimeout = Convert.ToInt32(Regex.Split(_project.CurrentActionValue, "->").Last());
            string ComparisonValue = Regex.Split(_project.CurrentActionValue, "->").AsEnumerable().Reverse().Skip(1).FirstOrDefault();
            string ComparisonType = Regex.Split(_project.CurrentActionValue, "->").AsEnumerable().Reverse().Skip(2).FirstOrDefault();

            if (CompareLineData.CompareTwoValues(IdentifierValue, ComparisonValue, ComparisonType)) { 
                _project.Loop = true; _project.LoopIndex = _project.CurrentLineIndex;
                if (Try) { _project.TryLoop = true; } }
            else { _project.Loop = false; if (Try) { _project.TryLoop = false; } }
        }

        /// <summary>
        /// If WhileLoop timed out, reset NumberOfWhiles and continue.
        /// </summary>
        /// <param name="lType"></param>
        public override void ResetLoop(ref LoopType lType)
        {
            _project.Loop = false;
            _project.InternalLoopCount = 0;

            if (lType == LoopType.While || lType == LoopType.TryWhile)
            {
                _automationType.Quit();

                ChromeActions.QuitMessage("While loop timeout", _project.TryLoop);

                return;
            }
        }

        /// <summary>
        /// Restart the loop if it has finished executing, but the timeout
        /// is higher so it has not looped enough times yet
        /// </summary>
        /// <param name="lType"></param>
        public override bool RestartLoop(ref string line, ref List<bool> CurrentTruths)
        {
            _project.CurrentLineIndex = _project.LoopIndex;

            line = _project.InterpretingLines[_project.CurrentLineIndex];

            // Same as above but new line.
            if (CurrentTruths.Count != 0)
            {
                ReturnLineData.GetLineInfoForNestedIf(ref CurrentTruths, line, out bool CurrentTruth,
                    out string DashesForTruth, out string DashesInLine);
                if (!CurrentTruth && DashesInLine == DashesForTruth) { return true; }
                if (CurrentTruth && DashesInLine == DashesForTruth)
                    line = Regex.Replace(line, @"^([\.|-].*?)(?=\w+)", "");
            }

            return false;
        }
    }
}
