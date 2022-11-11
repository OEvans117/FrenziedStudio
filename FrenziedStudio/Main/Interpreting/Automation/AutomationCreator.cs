using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    public class AutomationCreator
    {
        public static Automation CreateAutomation(AutomationType at, InterpretedProject pr, ReplacementManager rm)
        {
            switch (at)
            {
                case AutomationType.Selenium:
                    return new SeleniumAutomation(pr, rm);
                case AutomationType.Pupeteer:
                    return new PupeteerAutomation(pr, rm);
                default: return null;
            }
        }
    }
}
