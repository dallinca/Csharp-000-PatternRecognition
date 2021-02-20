using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Lib {
    class Attempt {
        public string PATTERN;

        public List<AttemptSegment> attemptSegments;
        public int score;
        public int availableScore;

        private float overallPercentage;

        public Dictionary<string, int> lastArmSegments;

        public Attempt(Pattern pattern) {
            PATTERN = pattern.name;

            attemptSegments = new List<AttemptSegment>();
            score = 0;
            availableScore = 0;

            lastArmSegments = new Dictionary<string, int>();
            foreach (string arm in pattern.arms.Keys) {
                lastArmSegments.Add(arm, -1);
            }
        }

        public float GetRelativePercentage() {
            return ((float)score) / ((float)availableScore);
        }

        public float SetOverallPercentage(float newOverallPercentage) {
            overallPercentage = newOverallPercentage;
            return overallPercentage;
        }

        public float GetOverallPercentage() {
            return overallPercentage;
        }

        public Boolean AddAttemptSegment(AttemptSegment attemptSegment) {
            if (attemptSegment == null) {
                return false;
            }

            attemptSegments.Add(attemptSegment);
            score += attemptSegment.GetScore();
            availableScore += attemptSegment.GetAvailableScore();
            return true;
        }
    }
}
