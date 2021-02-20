using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR.Lib {
    /*
     * Segment - Small portion of movement in the Arm. If another Arm has movement and the current
     * Arm does not, the segment for the current Arm will be null.
     * KeySegment - Each segment in this arm that is not null (implying movement).
     * SequenceId - The ID of the segment normalized across all segments in all Arms of the Pattern.
     * Segment SequenceId -
     * KeySegment SequenceId -
     * KeySegment SuccessiveId - Ordered ID of KeySegments. Similar to SequenceID but skipping over
     * null segements for this Arm. Useful for judging relative movement distance of different segments.
     * 
     */

    class Arm {
        /*
         * The Ordered list of segments in this Arm, normalized across all segments in all arms. This
         * list often has empty/null segments meaning that other arms of the pattern are moving while
         * arm remains still.
         * 
         * Index is Segment SequenceId / seqId
         * 
         * Global Ordering IDs = sequenceId / seqId
         */
        public List<Segment> segments;
        /*
         * KeySegment Sequence Ids, in their Successive order.
         * 
         * KeySegments are all of the segments in this arm that contain new movement. KeySegments are
         * never null.
         * 
         * Index is Segment SuccessiveId / sucId
         * 
         * Local Ordering IDs = successiveId / sucId
         */
        private List<int> keySegmentSeqIds;
        /*
         * Segment SequenceId --> KeySegment SequenceId, of the KeySegment assigned to the Segment
         */
        private Dictionary<int, int> keySegmentSeqIdLookup;
        /*
         * KeySegment SequenceId --> SuccessiveId
         */
        private Dictionary<int, int> KeySegmentSucIdLookup;

        public Arm() {
            segments = new List<Segment>();
        }

        public void InitForRecognizer() {
            keySegmentSeqIds = new List<int>();
            keySegmentSeqIdLookup = new Dictionary<int, int>();
            KeySegmentSucIdLookup = new Dictionary<int, int>();

            GenerateKeySegmentNumberLookup();
        }

        public int GetTotalNumberOfKeySegments() {
            return keySegmentSeqIds.Count;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            foreach (Segment segment in segments) {
                sb.Append($"S{segment}\n");
            }
            
            return sb.ToString();
        }

        public int GetCoordinationDistance(int sequenceId1, int sequenceId2) {
            // Verify sequenceIds within Range
            int keySegmentSucId1 = 0;
            int keySegmentSucId2 = 0;

            if (sequenceId1 >= segments.Count) {
                sequenceId1 = segments.Count;
            }

            if (sequenceId2 >= segments.Count) {
                sequenceId2 = segments.Count;
            }

            // Get Distance
            if (sequenceId1 >= 0) {
                keySegmentSucId1 = KeySegmentSucIdLookup[keySegmentSeqIdLookup[sequenceId1]];
            }
            if (sequenceId2 >= 0) {
                keySegmentSucId2 = KeySegmentSucIdLookup[keySegmentSeqIdLookup[sequenceId2]];
            }

            return Math.Abs(keySegmentSucId1 - keySegmentSucId2);
        }

        private void GenerateKeySegmentNumberLookup() {
            for (int s = 0, lastKeySegmentID = 0; s < segments.Count; s++) {
                if (segments[s] != null) {
                    keySegmentSeqIdLookup.Add(s, s);
                    lastKeySegmentID = s;

                    KeySegmentSucIdLookup.Add(s, keySegmentSeqIds.Count);
                    keySegmentSeqIds.Add(s);
                } else {
                    keySegmentSeqIdLookup.Add(s, lastKeySegmentID);
                }
            }
        }
        
    }
}
