using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PR.Lib;

namespace PR.Lib {
    class Parser {

        public bool logInfo = false;

        private void LogInfo(StringBuilder sb) {
            LogInfo(sb);
        }

        private void LogInfo(string str) {
            if (!logInfo) {
                return;
            }

            Console.WriteLine(str);
        }

        public Pattern ParsePatternFile(string fileName) {
            LogInfo("Attempting parse of file: " + fileName);

            Pattern pattern = new Pattern();
            string line;
            int count = 0;
            string armName = "";

            // Read the file and display it line by line  
            System.IO.StreamReader file = new System.IO.StreamReader(@fileName);
            while ((line = file.ReadLine()) != null) {
                string[] sections = line.Split('|');

                if (sections.Length == 0) {
                    continue;
                }

                if (sections[0] == "N") {
                    // Fault Check
                    if (sections.Length < 2) {
                        LogInfo($"Line {count} does not have a Pattern Name");
                        return null;
                    } else if (pattern.name != null) {
                        LogInfo($"Line {count} is trying to set a Pattern Name, while a name has already been set");
                        return null;
                    }

                    // Add Name
                    pattern.name = sections[1];
                } else if (sections[0] == "A") {
                    // Fault Check
                    if (sections.Length < 2) {
                        LogInfo($"Line {count} does not have an Arm Name");
                        return null;
                    } else if (pattern.arms.ContainsKey(sections[1])) {
                        LogInfo($"Line {count} is trying to set an Arm Name, while a name has already been used for another Arm");
                        return null;
                    }

                    // Add Arm
                    pattern.arms.Add(sections[1], new Arm());
                    armName = sections[1];
                } else if (sections[0] == "S") {
                    // Fault Check
                    if (sections.Length > 1 && sections[1] == "") {
                        LogInfo($"Line {count} is trying to add a Segment. There is a space designated for the FIRST priority, but no node ID is present");
                        return null;
                    } else if (armName == "") {
                        LogInfo($"Line {count} is trying to add a Segment, while there is not an Arm in the Pattern yet");
                        return null;
                    }

                    Arm arm;
                    pattern.arms.TryGetValue(armName, out arm);
                    if (arm == null) {
                        LogInfo($"Line {count} is trying to add a Segment, but the stored arm name did not resolve to an object");
                        return null;
                    }

                    // Add Segment
                    arm.segments.Add(GetSegment(sections));
                }
                count++;
            }

            file.Close();
            return pattern;
        }

        private Segment GetSegment(String[] sections) {
            Segment segment = new Segment();

            for (int s = 1; s < sections.Length; s++) {
                if (sections[s] == "") {
                    break;
                }

                string[] nodes = sections[s].Split('=');
                for (int n = 0; n < nodes.Length; n++) {
                    if (nodes[n] == "") {
                        continue;
                    }

                    segment.nodes.Add(nodes[n], (Priority)s);
                }
            }

            return segment;
        }
    }
}
