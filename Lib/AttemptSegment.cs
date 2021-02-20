using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Lib {
    public class AttemptSegment {
        public string node;
        public string arm;
        int score;
        int availableScore;

        public AttemptSegment(string node, string arm, int score, int availableScore) {
            this.node = node;
            this.arm = arm;
            this.score = score;
            this.availableScore = availableScore;
        }

        public int GetScore() {
            return score;
        }

        public int GetAvailableScore() {
            return availableScore;
        }
    }
}
