using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PR.Lib;

namespace PR.Utility {
    class Recorder {
        public AWorld world;
        private Boolean recording = false;
        private Pattern pattern;
        private int highestSegmentNumber;
        private string highestSegmentArm;
        private Dictionary<string, int> highestSegmentNumbers;

        public Recorder(AWorld world) {
            this.world = world;
        }

        public Boolean Start(string name) {
            if (recording) {
                return false;
            }
            
            pattern = new Pattern(name);
            highestSegmentNumber = -1;
            highestSegmentNumbers = new Dictionary<string, int>();

            recording = true;
            return true;
        }

        public Pattern Stop() {
            if (!recording) {
                return null;
            }

            recording = false;
            return pattern;
        }

        public void Add(string node, string arm) {
            // Initialize arm information
            if (!pattern.arms.ContainsKey(arm)) {
                pattern.arms.Add(arm, new Arm());
                highestSegmentNumbers.Add(arm, -1);
            }

            // Construct the segment
            Segment segment = new Segment();
            segment.nodes.Add(node, Priority.FIRST);
            for (int d = 1; d <= world.GetMaxDepth(); d++) {
                List<string> depthNodes = world.GetAdjacentNodes(node, d);
                foreach (string depthNode in depthNodes) {
                    segment.nodes.Add(depthNode, (Priority)(d + 1));
                }
            }

            // Calculate segment Number
            int segmentNumber = 0;
            if (highestSegmentNumber != -1) {
                if (highestSegmentNumbers[arm] == highestSegmentNumber) {
                    highestSegmentNumber++;
                    highestSegmentNumbers[arm] = highestSegmentNumber;
                } else if (highestSegmentNumbers[arm] == highestSegmentNumber - 1) {
                    highestSegmentNumbers[arm] = highestSegmentNumber;
                } else {
                    highestSegmentNumbers[arm] = highestSegmentNumber - 1;
                }
                segmentNumber = highestSegmentNumbers[arm];
            }
            if (highestSegmentNumber == -1) {
                highestSegmentNumber = 0;
                highestSegmentNumbers[arm] = 0;
            }

            // If needed pad the new segment position
            for (int i = pattern.arms[arm].segments.Count; i < segmentNumber; i++) {
                pattern.arms[arm].segments.Add(null);
            }

            // Add the segment
            pattern.arms[arm].segments.Add(segment);

        }
    }
}
