using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Lib {
    class Segment {
        public Dictionary<string, Priority> nodes;
        public Dictionary<Priority, List<string>> inverseNodesLookup;

        public Segment() {
            nodes = new Dictionary<string, Priority>();
        }

        public override string ToString() {
            InverseNodes();
            StringBuilder sb = new StringBuilder();

            for (int p = 1; p <= 5; p++) {
                if (!inverseNodesLookup.ContainsKey((Priority)p)) {
                    continue;
                }

                bool addedFirst = false;
                sb.Append("|");
                foreach (string nodeName in inverseNodesLookup[(Priority)p]) {
                    if (addedFirst) {
                        sb.Append("=");
                    }
                    sb.Append(nodeName);
                    addedFirst = true;
                }
            }

            return sb.ToString();
        }

        private void InverseNodes() {
            if (inverseNodesLookup != null) {
                return;
            }

            inverseNodesLookup = new Dictionary<Priority, List<string>>();
            foreach(KeyValuePair<string, Priority> entry in nodes) {
                if (!inverseNodesLookup.ContainsKey(entry.Value)) {
                    inverseNodesLookup.Add(entry.Value, new List<string>());
                }
                inverseNodesLookup[entry.Value].Add(entry.Key);
            }
        }
    }
}
