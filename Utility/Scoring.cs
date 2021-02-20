using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PR.Lib;

namespace PR.Utility {
    class Scoring {
        protected Dictionary<Priority, int> priorityScores;
        protected int coordinationTolerance = 1;
        protected int coordinationMaxScore = 4;
        protected float patternCompletedPercentage = 0.8f;

        public Scoring() {
            priorityScores = new Dictionary<Priority, int>();
            priorityScores.Add(Priority.FIRST, 5);
            priorityScores.Add(Priority.SECOND, 3);
            priorityScores.Add(Priority.THIRD, 1);
            priorityScores.Add(Priority.FOURTH, 0);
            priorityScores.Add(Priority.FIFTH, 0);
        }

        public Scoring(int coordinationTolerance, int coordinationMaxScore, float patternCompletedPercentage, Dictionary<Priority, int> priorityScores) {
            this.coordinationTolerance = coordinationTolerance;
            this.coordinationMaxScore = coordinationMaxScore;
            this.patternCompletedPercentage = patternCompletedPercentage;
            this.priorityScores = priorityScores;
        }

        public Dictionary<Priority, int> PriorityScores() {
            return priorityScores;
        }

        public int CoordinationTolerance() {
            return coordinationTolerance;
        }

        public int CoordinationMaxScore() {
            return coordinationMaxScore;
        }

        public float PatternCompletedPercentage() {
            return patternCompletedPercentage;
        }
    }
}
