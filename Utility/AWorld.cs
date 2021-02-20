using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Utility {
    abstract class AWorld {
        /**
         * Nodes with a list of their neighboring nodes.
         */
        protected Dictionary<string, List<string>> nodes;

        // GENERIC
        private int maxDepth = 2;

        // GENERIC
        public AWorld() {
            nodes = new Dictionary<string, List<string>>();
            InitWorld();
        }

        abstract protected void InitWorld(); 

        /**
         * Get a list of nodes adjacent to the specified node up to the
         * specified depth. Default depth of 1.
         */
        public List<string> GetAdjacentNodes(string node, int depth = 1) {
            if (!nodes.ContainsKey(node)) {
                return null;
            }
            if (depth < 1) {
                return null;
            }

            HashSet<string> innerNodeSet = new HashSet<string>();
            innerNodeSet.Add(node);
            for (int d = 1; d < depth; d++) {
                innerNodeSet = ExpandSetDepth(innerNodeSet);
            }

            HashSet<string> completeNodeSet = ExpandSetDepth(innerNodeSet);
            return FindOuterSet(completeNodeSet, innerNodeSet).ToList();
        }

        // GENERIC
        private HashSet<string> ExpandSetDepth(HashSet<string> nodeSet) {
            HashSet<string> newNodeSet = new HashSet<string>();
            foreach (string node in nodeSet) {
                List<string> tempNodes = nodes[node];
                foreach (string tempNode in tempNodes) {
                    newNodeSet.Add(tempNode);
                }
                newNodeSet.Add(node);
            }
            return newNodeSet;
        }

        // GENERIC
        private HashSet<string> FindOuterSet(HashSet<string> completeSet, HashSet<string> innerSet) {
            HashSet<string> outerSet = new HashSet<string>();
            foreach (string completeSetNode in completeSet) {
                if (innerSet.Contains(completeSetNode)) {
                    continue;
                }
                outerSet.Add(completeSetNode);
            }

            return outerSet;
        }

        // GENERIC
        public int GetMaxDepth() {
            return maxDepth;
        }

        protected bool SetMaxDepth(int newMaxDepth) {
            if (newMaxDepth < 0 || newMaxDepth > 4) {
                return false;
            }
            maxDepth = newMaxDepth;
            return true;
        }

    }
}
