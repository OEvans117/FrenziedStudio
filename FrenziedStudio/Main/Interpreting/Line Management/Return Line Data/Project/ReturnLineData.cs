using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public static class ReturnLineData
    {
        public static void GetLineInfoForNestedIf(ref List<bool> CurrentTruths, string line,
            out bool CurrentTruth, out string DashesForTruth, out string DashesInLine)
        {
            // Get the last truth / scope
            // Get the dashes for the last truth / scope
            // Get the dashes in line to check if scope has ended.
            CurrentTruth = CurrentTruths.Last();
            DashesForTruth = string.Concat(Enumerable.Repeat("-", CurrentTruths.Count));
            DashesInLine = Regex.Match(line, @"^([\.|-].*?)(?=\w+)").Value.Replace(".", "");

            // If no dashes in line just return.
            // All scopes have ended.
            if (string.IsNullOrEmpty(DashesInLine)) { return; }

            // If line doesn't have as many dashes as current truths,
            // then one of the current truth scopes has ended.
            // Therefore, remove it from the list, and recalculate
            // CurrentTruth and DashesForTruth for line.
            if (DashesInLine != DashesForTruth)
            {
                if (CurrentTruths.Count == 0) { return; }

                while (CurrentTruths.Count >= DashesInLine.Length + 1)
                    CurrentTruths.Remove(CurrentTruths.Last());

                CurrentTruth = CurrentTruths.Last();
                DashesForTruth = string.Concat(Enumerable.Repeat("-", CurrentTruths.Count));
            }
        }
    }

    public enum LoopType
    {
        While,
        TryWhile,
        For,
        ElemLoop
    }
}
