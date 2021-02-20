using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PR.Lib;

namespace PR.Utility {
    class Recognizer {
        // Unique Identifier for this instance of the Recognizer class
        private string ID;
        public bool logInfo = false;

        private Dictionary<string, ICompletionListener> completionListeners;

        private Dictionary<string, Pattern> patterns;
        private Dictionary<string, int> patternMaxScore; // The highest available score for each pattern
        private Dictionary<string, List<Attempt>> allPatterns_Attempts; // All attempts for each Pattern
        private Dictionary<string, Attempt> allPatterns_BestAttemptsRawOverall; // Current Strongest Overall attempt per Pattern
        private Dictionary<string, Attempt> allPatterns_BestAttemptsRelative; // Current Strongest Attempt for current percentage of completion
        private string strongestPattern; // current pattern with highest match
        private bool patternStrongEnough = false; // If the strongest pattern is complete

        // MOVE/STEP SCORING
        private Scoring scoring = null;

        private enum State { ON, PAUSED, OFF }
        private State state = State.OFF;

        public Recognizer(string ID = "default") {
            this.ID = ID;
            completionListeners = new Dictionary<string, ICompletionListener>();

            patterns = new Dictionary<string, Pattern>();
            patternMaxScore = new Dictionary<string, int>();
            allPatterns_Attempts = new Dictionary<string, List<Attempt>>();
            allPatterns_BestAttemptsRawOverall = new Dictionary<string, Attempt>();
            allPatterns_BestAttemptsRelative = new Dictionary<string, Attempt>();

            scoring = new Scoring();
        }

        public Recognizer(Scoring scoring, string ID = "default") {
            this.ID = ID;
            patterns = new Dictionary<string, Pattern>();
            patternMaxScore = new Dictionary<string, int>();
            allPatterns_Attempts = new Dictionary<string, List<Attempt>>();
            allPatterns_BestAttemptsRawOverall = new Dictionary<string, Attempt>();
            allPatterns_BestAttemptsRelative = new Dictionary<string, Attempt>();

            this.scoring = scoring;
        }

        public void PrintAttempts() {
            if (!logInfo) {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Number Of Patterns: " + patterns.Count + "\n");
            sb.Append("Strongest Pattern: " + strongestPattern + "\n");
            sb.Append("Is Pattern StrongEnough: " + patternStrongEnough + "\n");
            foreach (KeyValuePair<string, List<Attempt>> attempts in allPatterns_Attempts) {
                sb.Append(attempts.Key + "\n");
                sb.Append("Number of Attempts: " + attempts.Value.Count + "\n");
                foreach (Attempt attempt in attempts.Value) {
                    sb.Append("Attempt Length:" + attempt.attemptSegments.Count + "\n");
                    sb.Append("Score: " + attempt.score + "\n");
                    sb.Append("Overall & Relative Completion __ " + attempt.GetOverallPercentage() * 100 + "% __ " + attempt.GetRelativePercentage() * 100 + "%\n");
                    sb.Append("\t");
                    foreach (KeyValuePair<string, int> arm in attempt.lastArmSegments) {
                        sb.Append(arm.Key + ":" + arm.Value + "  ");
                    }
                    sb.Append("\n");
                }
            }
            Console.WriteLine(sb);
        }

        public bool AddCompletionListener(ICompletionListener completionListener) {
            if (completionListeners.ContainsKey(completionListener.GetCompletionListenerID())) {
                return false; // ID already exists in the dictionary
            }

            completionListeners.Add(completionListener.GetCompletionListenerID(), completionListener);
            return true;
        }

        public bool RemoveCompletionListener(ICompletionListener completionListener) {
            return RemoveCompletionListener(completionListener.GetCompletionListenerID());
        }

        public bool RemoveCompletionListener(string completionListener) {
            if (!completionListeners.ContainsKey(completionListener)) {
                return false;
            }

            completionListeners.Remove(completionListener);
            return true;
        }

        public Boolean AddPattern(Pattern pattern) {
            if (null == pattern) {
                return false;
            }
            if (pattern.name == null || pattern.name == "") {
                return false;
            }
            if (patterns.ContainsKey(pattern.name)) {
                return false;
            }

            pattern.InitForRecognizer();
            patterns.Add(pattern.name, pattern);
            patternMaxScore.Add(pattern.name, CalcPatternMaxScore(pattern));
            allPatterns_Attempts.Add(pattern.name, new List<Attempt>());
            allPatterns_BestAttemptsRawOverall.Add(pattern.name, null);
            allPatterns_BestAttemptsRelative.Add(pattern.name, null);

            return true;
        }

        private int CalcPatternMaxScore(Pattern pattern) {
            return pattern.GetTotalNumberOfKeySegments() * CalcKeySegmentMaxScore();
        }

        private int CalcKeySegmentMaxScore() {
            return scoring.PriorityScores()[Priority.FIRST] + scoring.CoordinationMaxScore();
        }

        private float CalcAttemptOverallPercentage(Attempt attempt) {
            return (float)attempt.score / (float)patternMaxScore[attempt.PATTERN];
        }

        public Boolean Start() {
            if (State.OFF != state) {
                return false;
            }

            ClearAttempts();

            state = State.ON;
            return true;
        }

        public Boolean Pause() {
            if (State.ON != state) {
                return false;
            }

            state = State.PAUSED;
            return true;
        }

        public Boolean UnPause() {
            if (State.PAUSED != state) {
                return false;
            }

            state = State.ON;
            return true;
        }

        public Boolean Stop() {
            if (State.ON != state) {
                return false;
            }

            state = State.OFF;
            return true;
        }

        public Boolean AddStep(string node, string arm) {
            if (state != State.ON) {
                return false;
            }
            if (patterns == null || patterns.Count == 0) {
                return false;
            }
            if (node == null || node == "") {
                return false;
            }
            if (arm == null || arm == "") {
                return false;
            }

            AddStepPrivate(node, arm);

            PrintAttempts();
            return true;
        }

        private void AddStepPrivate(string node, string arm) {
            // Update Attampts with new Step
            foreach (KeyValuePair<string, Pattern> pattern in patterns) {
                foreach (Attempt attempt in allPatterns_Attempts[pattern.Key]) {
                    AddStepToAttempt(pattern.Value, arm, node, attempt);
                }

                // Generate new Attempt
                allPatterns_Attempts[pattern.Key].Add(AddStepToAttempt(pattern.Value, arm, node, new Attempt(pattern.Value)));
            }

            // Prune Bad Attempts
            PruneAttempts();

            // Update Best attempts
            UpdateBestAttempts();

            // Check if a pattern was completed
            if (PatternCompleted()) {
                NotifyPatternCompletion();
                return;
            }     
        }

        private Attempt AddStepToAttempt(Pattern pattern, string arm, string node, Attempt attempt) {
            // Ref Objects
            Arm armObj = pattern.arms[arm];

            // Get next segment
            int nextSegmentId = attempt.lastArmSegments[arm] + 1;
            // Check segments for node presence
            for (int s = nextSegmentId; s < armObj.segments.Count; s++) {
                if (armObj.segments[s].nodes.ContainsKey(node)) {
                    int score = 0;
                    int availableScore = 0;

                    // Add Node Score
                    Priority priority = armObj.segments[s].nodes[node];
                    score += scoring.PriorityScores()[priority];
                    availableScore += scoring.PriorityScores()[Priority.FIRST];

                    // Add Coordination Score
                    if (pattern.arms.Count == 1) { // Can only coordinate if more than one arm exists
                        score += scoring.CoordinationMaxScore();
                    } else {
                        int armsCoordinated = 0;
                        int armsToCoordinate = pattern.arms.Count - 1;
                        foreach (KeyValuePair<string, int> lastArmSegment in attempt.lastArmSegments) {
                            // Skip over current arm
                            if (lastArmSegment.Key == arm) {
                                continue;
                            }

                            // Check
                            if (pattern.arms[lastArmSegment.Key].GetCoordinationDistance(s, lastArmSegment.Value) <= scoring.CoordinationTolerance()) {
                                armsCoordinated++;
                            }
                        }
                        score += scoring.CoordinationMaxScore() * armsCoordinated / armsToCoordinate;
                    }
                    availableScore += scoring.CoordinationMaxScore();
                    
                    // Add the new Attempt Segment and update the last Arm Segment to the new one used
                    attempt.AddAttemptSegment(new AttemptSegment(node, arm, score, availableScore));
                    attempt.lastArmSegments[arm] = s;
                    return attempt;
                }
            }

            // New Node is not in the pattern -- add failed attempt segment
            attempt.AddAttemptSegment(new AttemptSegment(node, arm, -CalcKeySegmentMaxScore(), 0));
            return attempt;
        }

        private void UpdateBestAttempts() {
            foreach (KeyValuePair<string, List<Attempt>> attempts in allPatterns_Attempts) {

                foreach (Attempt attempt in attempts.Value) {
                    attempt.SetOverallPercentage(CalcAttemptOverallPercentage(attempt));
                }

                // Find the best attempts for the pattern
                Attempt bestRawOverallAttempt = null;
                Attempt bestRelativeAttempt = null;
                foreach (Attempt attempt in attempts.Value) {
                    if (bestRawOverallAttempt == null) {
                        bestRawOverallAttempt = attempt;
                        bestRelativeAttempt = attempt;
                        continue;
                    }

                    if (attempt.score > bestRawOverallAttempt.score) {
                        bestRawOverallAttempt = attempt;
                    }
                    if (attempt.GetRelativePercentage() > bestRelativeAttempt.GetRelativePercentage()) {
                        bestRelativeAttempt = attempt;
                    }
                }

                // Log the best attempts for the pattern
                allPatterns_BestAttemptsRawOverall[attempts.Key] = bestRawOverallAttempt;
                allPatterns_BestAttemptsRelative[attempts.Key] = bestRelativeAttempt;
            }

            Attempt bestOverallAttempt = null;
            foreach (KeyValuePair<string, Attempt> bestPatternAttempt in allPatterns_BestAttemptsRawOverall) {
                float currentPercentage = bestPatternAttempt.Value.GetOverallPercentage();

                if (bestOverallAttempt == null || currentPercentage > bestOverallAttempt.GetOverallPercentage()) {
                    bestOverallAttempt = bestPatternAttempt.Value;
                    strongestPattern = bestPatternAttempt.Key;
                }
            }

            if (bestOverallAttempt != null && bestOverallAttempt.GetOverallPercentage() > scoring.PatternCompletedPercentage()) {
                patternStrongEnough = true;
            } else {
                patternStrongEnough = false;
            }
        }

        private Boolean PatternCompleted() {
            if (patternStrongEnough) {
                return true;
            }
            return false;
        }

        private void PruneAttempts() {
            List<int> indexesToRemove = new List<int>();
            foreach (KeyValuePair<string, List<Attempt>> attempts in allPatterns_Attempts) {
                // Find the indexes to remove
                for (int i = 0; i < attempts.Value.Count; i++) {
                    if (attempts.Value[i].score < 0) {
                        indexesToRemove.Add(i);
                    }
                }
                // Remove the found indexes
                for (int i = indexesToRemove.Count - 1; i >= 0; i--) {
                    attempts.Value.RemoveAt(indexesToRemove[i]);
                }
                indexesToRemove.Clear();
            }
        }

        private void ClearAttempts() {
            foreach (KeyValuePair<string, List<Attempt>> attempts in allPatterns_Attempts) {
                attempts.Value.Clear();
            }
            
            foreach (string patternName in patterns.Keys) {
                allPatterns_BestAttemptsRawOverall[patternName] = null;
                allPatterns_BestAttemptsRelative[patternName] = null;
            }
            
            strongestPattern = null;
        }

        private void NotifyPatternCompletion() {
            foreach (KeyValuePair<String, ICompletionListener> completionListener in completionListeners) {
                completionListener.Value.PatternCompleted(this, strongestPattern, allPatterns_BestAttemptsRawOverall[strongestPattern].GetOverallPercentage());
            }
        }
    }
}
