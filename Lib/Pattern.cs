using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Lib {
    class Pattern {
        public string name;
        public Dictionary<string, Arm> arms;

        public Pattern() {
            arms = new Dictionary<string, Arm>();
        }
        public Pattern(string name) {
            this.name = name;
            arms = new Dictionary<string, Arm>();
        }

        public void InitForRecognizer() {
            foreach(KeyValuePair<string, Arm> arm in arms) {
                arm.Value.InitForRecognizer();
            }
        }

        public int GetTotalNumberOfKeySegments() {
            int totalKeySegments = 0;
            foreach (KeyValuePair<string, Arm> entry in arms) {
                totalKeySegments += entry.Value.GetTotalNumberOfKeySegments();
            }

            return totalKeySegments;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append($"N|{name}\n");
            foreach(KeyValuePair<string, Arm> entry in arms) {
                sb.Append($"A|{entry.Key}\n");
                sb.Append(entry.Value);
            }

            return sb.ToString();
        }
    }
}
