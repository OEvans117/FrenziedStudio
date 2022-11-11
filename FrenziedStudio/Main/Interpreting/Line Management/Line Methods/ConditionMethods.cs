using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public class ConditionMethods : MethodManager
    {
        public ConditionMethods(InterpretedProject pr, ReplacementManager rm, Automation at) : base(pr, rm, at) { }

        /// <summary>
        /// If statement
        /// if=text->xpath->equals->hello
        /// </summary>
        /// <param name="CurrentTruths"></param>
        public override void IfCondition(ref List<bool> CurrentTruths)
        {
            var OrSplit = Regex.Split(_project.CurrentActionValue, "or").Select(c => c.Replace(" ", ""));

            if (OrSplit.Count() != 0 && Regex.Matches(OrSplit.First(), "->").Count == 2 && _project.CurrentActionValue.Contains(" or "))
            {
                bool FoundTruth = false;

                foreach (string Comparison in OrSplit)
                {
                    string IdentifierValue = Regex.Split(Comparison, "->").First();
                    string ComparisonValue = Regex.Split(Comparison, "->").Last();
                    string ComparisonType = Regex.Split(Comparison, "->").AsEnumerable().Reverse().Skip(1).FirstOrDefault();

                    if (CompareLineData.CompareTwoValues(IdentifierValue, ComparisonValue, ComparisonType))
                    {
                        FoundTruth = true;
                        break;
                    }
                }

                if (FoundTruth)
                    CurrentTruths.Add(true);
                else
                    CurrentTruths.Add(false);
            }
            else
            {
                string IdentifierValue = Regex.Split(_project.CurrentActionValue, "->").First();
                string ComparisonValue = Regex.Split(_project.CurrentActionValue, "->").Last();
                string ComparisonType = Regex.Split(_project.CurrentActionValue, "->").AsEnumerable().Reverse().Skip(1).FirstOrDefault();

                if (CompareLineData.CompareTwoValues(IdentifierValue, ComparisonValue, ComparisonType))
                    CurrentTruths.Add(true);
                else
                    CurrentTruths.Add(false);
            }
        }
    }
}