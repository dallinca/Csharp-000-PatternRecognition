using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Utility {
    interface ICompletionListener {
        void PatternCompleted(Recognizer recognizer, string patternID, float completionPercentage);
        string GetCompletionListenerID();
    }
}
