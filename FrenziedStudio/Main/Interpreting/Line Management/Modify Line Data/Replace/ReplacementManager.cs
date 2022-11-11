using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public class ReplacementManager
    {
        public InterpretedProject _project { get; set; }
        public Automation _automationType { get; set; }

        public ReplaceSettings _replaceSettings { get; set; }
        public ReplaceDynamics _replaceDynamics { get; set; }

        public ReplaceFiles _replaceFiles { get; set; }
        public ReplaceBrowser _replaceBrowser { get; set; }

        public ReplacementManager(InterpretedProject pr)
        {
            _project = pr;
            _replaceSettings = new ReplaceSettings(_project);
            _replaceDynamics = new ReplaceDynamics(_project, _automationType);
        }

        public void SetAutomation(Automation at)
        {
            _automationType = at;
            _replaceDynamics._replaceBrowser._automationType = at;
        }

        /// <summary>
        /// Use ReturnSettingValue & ReturnCommonVariableValue (methods above) 
        /// to modify ActionValue OR create new variables based on Lists/Commons.
        /// If SettingValue = ConcurrentQueue: replace {{example}} with dequeue of SettingValue. (ReplaceSettings)
        /// If SettingValue = string: replace {{example}} with SettingValue. (ReplaceSettings)
        /// If SettingValue = {{FirstName}} {{LastName}}, replace too. (ReplaceCommonVariables)
        /// Create: {{List:newVar}} OR {{RandLastName:testVar}}
        /// </summary>
        /// <param name="ActionValue"></param>
        /// <param name="pr"></param>
        /// <returns></returns>
        public async Task<List<Tuple<string, object>>> ReplaceActionValueAddSettingAsync(List<Tuple<string, object>> LocalSettings, InterpretedProject pr)
        {
            // For each {{Var}} in ActionValue (so every one gets replaced).
            MatchCollection SettingOrVariable = Regex.Matches(pr.CurrentActionValue, @"{{(.*)}}");

            // Set creation type
            VarCreationType creationType = new VarCreationType();

            if (pr.CurrentActionName == "setglobalvariable") creationType = VarCreationType.GlobalVar;
            else if (pr.CurrentActionName == "setvariable") creationType = VarCreationType.LocalVar;
            else creationType = VarCreationType.NoCreation;
            bool CreatingNewVariable = creationType != VarCreationType.NoCreation;

            for (int i = 0; i < SettingOrVariable.Count; i++)
            {
                // Replace ActionValue settings/variables with above methods.
                _replaceSettings.ReplaceSettingWithValue(CreatingNewVariable, LocalSettings);

                // Replace spintax {test/test}
                // common variables {RandMaleName}
                // and methods like RandomFile in ActionValue

                _replaceDynamics.ReplaceSpintaxWithValue();
                _replaceDynamics.ReplaceCommonVariableWithValue();
                await _replaceDynamics.ReplaceMethodsWithValue();

                if (pr.CurrentActionName != "goto")
                    _replaceDynamics.ReplaceUrlWithValue();

                if (CreatingNewVariable)
                    SettingOrVariable = Regex.Matches(pr.CurrentActionValue, @"{{(.*)}}");

                // Examples of SettingName: {{NormalWait1}} or {{List:listItem}}
                string SettingName = SettingOrVariable[i].Groups[1].Value;

                // If a setting contains "->", it means a variable is being set.
                // If ValueReplacement has something (setting/var), create var.
                // If not, value->name, just directly set value (works for url).
                if (CreatingNewVariable)
                {
                    string[] SettingSplit = Regex.Split(SettingName, "->");

                    int globIndex = pr.Settings.FindIndex(s => s.Item1 == SettingSplit[1]);
                    if (globIndex >= 0) pr.Settings.RemoveAt(globIndex);

                    int localIndex = LocalSettings.FindIndex(s => s.Item1 == SettingSplit[1]);
                    if (localIndex >= 0) LocalSettings.RemoveAt(localIndex);

                    if (creationType == VarCreationType.GlobalVar)
                        pr.Settings.Add(new Tuple<string, object>(SettingSplit[1], SettingSplit[0]));
                    else
                        LocalSettings.Add(new Tuple<string, object>(SettingSplit[1], SettingSplit[0]));
                }
            }

            return LocalSettings;
        }
    }

    public enum VarCreationType
    {
        NoCreation,
        LocalVar,
        GlobalVar
    }
}
