using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PR.Lib;

namespace PR.Utility {
    class PatternManager {

        private string patternLocation = "";
        private const string registryName = "_Registry.txt";
        Parser parser = new Parser();
        List<string> patternNames = new List<string>();


        public PatternManager(string patternLocation) {
            this.patternLocation = patternLocation;
            LoadRegistry();
        }

        private string GetRegistryPath() {
            return patternLocation + registryName;
        }

        public Pattern GetPattern(string patternName) {
            if (!patternNames.Contains(patternName)) {
                return null;
            }

            return parser.ParsePatternFile(patternLocation + patternName + ".txt");
        }

        public Dictionary<string, Pattern> GetAllPatterns() {
            Dictionary<string, Pattern> allPatterns = new Dictionary<string, Pattern>();

            foreach (string patternName in patternNames) {
                allPatterns.Add(patternName, parser.ParsePatternFile(patternLocation + patternName + ".txt"));
            }

            return allPatterns;
        }

        public List<string> GetPatternNames() {
            return patternNames;
        }

        /**
         * Attempt to Add a new Pattern.
         * False -> Pattern not added, either null or name already exists
         * True -> Pattern added
         */
        public bool AddPattern(Pattern pattern) {
            if (pattern == null || patternNames.Contains(pattern.name)) {
                return false;
            }
            
            File.WriteAllText(patternLocation + pattern.name + ".txt", pattern.ToString());
            FileStream fAppend = File.Open(GetRegistryPath(), FileMode.Append);
            StreamWriter sw = new StreamWriter(fAppend);
            sw.WriteLine(pattern.name);
            sw.Close();
            patternNames.Add(pattern.name);
            return true;
        }

        public void DeletePattern(string patternName) {
            if (!patternNames.Contains(patternName)) {
                return;
            }

            patternNames.Remove(patternName);
            File.Delete(patternLocation + patternName + ".txt");
            File.WriteAllText(GetRegistryPath(), ToString());
        }

        private void LoadRegistry() {
            if (File.Exists(GetRegistryPath())) {
                System.IO.StreamReader file = new System.IO.StreamReader(GetRegistryPath());
                string line = null;
                while ((line = file.ReadLine()) != null) {
                    patternNames.Add(line);
                }
                file.Close();
            } else {
                File.CreateText(GetRegistryPath()).Close();
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach(string name in patternNames) {
                sb.Append($"{name}\n");
            }
            return sb.ToString();
        }

    }
}
