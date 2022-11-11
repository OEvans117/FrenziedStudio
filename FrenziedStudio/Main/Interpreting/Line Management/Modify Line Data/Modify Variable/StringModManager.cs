using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public static class StringModManager
    {
        public static void ApplyStringFunctions(string ActionValue, ref string Text, ref int Count, int SettingPosition)
        {
            string SettingDelimitation = string.Empty;
            bool SettingNeedsDelimitation = false;

            try
            {
                string Substr = ActionValue.Substring(SettingPosition, 6);
                SettingNeedsDelimitation = Regex.Matches(Substr, @"\[.\]\[[0-9]\]").Count != 0;
            }
            catch { }

            if (SettingNeedsDelimitation)
                SettingDelimitation = ActionValue.Substring(SettingPosition, 6);

            string SettingSubstring = string.Empty;
            bool SettingNeedsSubstring = false;
            int SubstringLength = 0;

            try
            {
                var substringMatches = Regex.Matches(ActionValue, @".*?(\[s\]\[\d+,\d+\])");
                SettingNeedsSubstring = substringMatches.Count != 0;
                SubstringLength = substringMatches[0].Groups[1].Value.Length;
            }
            catch { }

            if (SettingNeedsSubstring)
                SettingSubstring = ActionValue.Substring(SettingPosition, SubstringLength);

            string SettingRegex = string.Empty;
            bool SettingNeedsRegex = false;
            int RegexLength = 0;

            try
            {
                var regexMatches = Regex.Matches(ActionValue, @".*?(\[r\]\[\d+,\d+\]\```.*?\```)");
                SettingNeedsRegex = regexMatches.Count != 0;
                RegexLength = regexMatches[0].Groups[1].Value.Length;
            }
            catch { }

            if (SettingNeedsRegex)
                SettingRegex = ActionValue.Substring(SettingPosition, RegexLength);


            if (SettingNeedsDelimitation)
            {
                Text = DelimitText(Text + SettingDelimitation);
                Count += 6;
            }

            if (SettingNeedsSubstring)
            {
                Text = SubstringText(Text + SettingSubstring);
                Count += SubstringLength;
            }

            if (SettingNeedsRegex)
            {
                Text = RegexText(Text + SettingRegex);
                Count += RegexLength;
            }
        }
        public static void ApplyStringFunctions(InterpretedProject pr, ref string Text, int SettingPosition)
        {
            string SettingDelimitation = string.Empty;
            bool SettingNeedsDelimitation = false;

            try
            {
                var DelimitationMatches = Regex.Matches(pr.CurrentActionValue, @"\[.\]\[[0-9]\]");
                SettingNeedsDelimitation = DelimitationMatches.Count != 0;

                if (SettingNeedsDelimitation)
                {
                    SettingDelimitation = DelimitationMatches[0].Value;
                }

            }
            catch { }

            string SettingSubstring = string.Empty;
            bool SettingNeedsSubstring = false;

            try
            {
                var substringMatches = Regex.Matches(pr.CurrentActionValue, @".*?(\[s\]\[\d+,\d+\])");
                SettingNeedsSubstring = substringMatches.Count != 0;

                if (SettingNeedsSubstring)
                {
                    SettingSubstring = substringMatches[0].Groups[1].Value;
                }

            }
            catch { }

            string SettingRegex = string.Empty;
            bool SettingNeedsRegex = false;

            try
            {
                var regexMatches = Regex.Matches(pr.CurrentActionValue, @".*?(\[r\]\[\d+,\d+\]\```.*?\```)");
                SettingNeedsRegex = regexMatches.Count != 0;

                if (SettingNeedsRegex)
                {
                    SettingRegex = regexMatches[0].Groups[1].Value;
                }

            }
            catch { }

            if (SettingNeedsDelimitation)
            {
                Text = DelimitText(Text + SettingDelimitation);
                pr.CurrentActionValue = pr.CurrentActionValue.Replace(SettingDelimitation, "");
            }

            if (SettingNeedsSubstring)
            {
                Text = SubstringText(Text + SettingSubstring);
                pr.CurrentActionValue = pr.CurrentActionValue.Replace(SettingSubstring, "");
            }

            if (SettingNeedsRegex)
            {
                Text = RegexText(Text + SettingRegex);
                pr.CurrentActionValue = pr.CurrentActionValue.Replace(SettingRegex, "");
            }
        }

        /// <summary>
        /// Delimit a string and retun the result.
        /// </summary>
        /// <param name="DelimitString">Example: username:null[:][1]</param>
        private static string DelimitText(string DelimitString)
        {
            string DelimitPattern = @"(.*?)\[(.)\]\[([0-9])\]";

            var DelimitationMatch = Regex.Match(DelimitString, DelimitPattern);

            if (!DelimitationMatch.Success) { return string.Empty; }

            char Delimiter = char.Parse(DelimitationMatch.Groups[2].Value);

            int Index = Convert.ToInt32(DelimitationMatch.Groups[3].Value);

            return DelimitationMatch.Groups[1].Value.Split(Delimiter)[Index];
        }
        private static string SubstringText(string SubstringString)
        {
            string SubstringPattern = @"(.*?)\[s\]\[(\d+),(\d+)\]";

            var SubstringMatch = Regex.Match(SubstringString, SubstringPattern);

            if (!SubstringMatch.Success) { return string.Empty; }

            int StartIndex = Convert.ToInt32(SubstringMatch.Groups[2].Value);
            int Length = Convert.ToInt32(SubstringMatch.Groups[3].Value);

            return SubstringMatch.Groups[1].Value.Substring(StartIndex, Length);
        }
        private static string RegexText(string RegexString)
        {
            string RegexPattern = @"(.*?)\[r\]\[(\d+,\d+)\]\```(.*?)\```";

            var RegexMatch = Regex.Match(RegexString, RegexPattern);

            if (!RegexMatch.Success) { return string.Empty; }

            int MatchCount = Convert.ToInt32(RegexMatch.Groups[2].Value.Split(',')[0]);

            int GroupsCount = Convert.ToInt32(RegexMatch.Groups[2].Value.Split(',')[1]);

            return Regex.Matches(RegexMatch.Groups[1].Value, RegexMatch.Groups[3].Value)[MatchCount].Groups[GroupsCount].Value;
        }
    }
}
