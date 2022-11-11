using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrenziedStudio.Main
{
    /// <summary>
    /// Methods to do with interacting with InterpretedProject
    /// and the current project script (not replacement/modification).
    /// </summary>
    public abstract class MethodManager
    {
        public InterpretedProject _project { get; set; }
        public ReplacementManager _replacementManager { get; set; }
        public Automation _automationType { get; set; }

        public MethodManager(InterpretedProject pr, ReplacementManager rm, Automation at)
        {
            _project = pr;
            _replacementManager = rm;
            _automationType = at;
        }

        public virtual void ForLoop(ref LoopType lType, List<Tuple<string, object>> LocalSettings) { }
        public virtual void WhileLoop(ref LoopType lType, bool Try) { }

        /// <summary>
        /// If InternalLoopCount has reached InternalLoopTimeout,
        /// aka loop has timed out, reset the loop and quit browser.
        /// </summary>
        public virtual void ResetLoop(ref LoopType lType) { }

        /// <summary>
        /// Go back to first line of while to redo loop as 
        /// InternalLoopCount has not reached InternalLoopTimeout
        /// </summary>
        /// <param name="line"></param>
        /// <param name="CurrentTruths"></param>
        /// <returns></returns>
        public virtual bool RestartLoop(ref string line, ref List<bool> CurrentTruths) { return false; }

        public virtual void IfCondition(ref List<bool> CurrentTruths) { }
    }
}
