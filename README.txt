This is a Generic Pattern Recognizer

Setting up:
1) Implement Abstract Class PR.Utility.AWorld.cs
The World is defined as a set of named (string) nodes with each node
having a list of their neighboring nodes names (string) as well as a
maxDepth.
- The max depth default should be 2 and can not be lower than 0 or higher than 4.
- Max depth 0 is the most difficult - means only the target node can give credit
- Max depth 4 is the easiest - means neighboring nodes up to 4 adjancencies away from the target node can give credit
- The adjancent nodes at any depth are calculated for you based on the provide list of neighboring nodes provided (depth of 1)

	Using PR.Utility;
    class World1 : PR.Utility.AWorld {
        protected override void InitWorld() {
			SetMaxDepth(2);
            string node;
            List<string> adjacentNodes;
            nodes.Add(node, adjacentNodes);
		}
    }

2) Use PR.Utility.Recorder.cs Utility class 
Record Patterns:
- Provide a World, as defined in step one, to the recorder
- General Pattern makeup
	- A Pattern has 1 or more arms
	- An Arm has 1 or more segemnts
	- A Segment has 1 FirstPriority Node and 0 or more Second-Fifth priority nodes
- When adding a step to the pattern you specify
	- the FirstPriority Node
		- Based on the MaxDepth of the world the Second-Fifth priority nodes are calculated for you
	- The Arm that should hit the Node
- Note that the order the nodes are add is the same order they are expected for pattern completion
- When you start the recording, you must specify a name. Make sure this is unique against all previous patterns
- Stopping the Recording will return the completed Pattern.
- A Single world can be used to record any number of patterns.

	Using PR.Utility;
	Using PR.Lib;
    AWorld world1 = new World1();
    Recorder recorder = new Recorder(world1);
	recorder.Start("PatternName");
	recorder.Add("node1", "Arm1");
	recorder.Add("node2", "Arm1");
	recorder.Add("node3", "Arm1");
	recorder.Add("x2y1", "Arm2");
	recorder.Add("x3y1", "Arm2");
	recorder.Add("x3y2", "Arm2");
	recorder.Add("x3y3", "Arm2");
	recorder.Add("node6", "Arm1");
	recorder.Add("node7", "Arm1");
	recorder.Add("node8", "Arm1");
	recorder.Add("x3y7", "Arm2");
	recorder.Add("x2y8", "Arm2");
	Pattern pattern = recorder.Stop();	

3) Use PR.Utility.PatternManager.cs
Save the newly created pattern using the PatternManager

	Using PR.Utility;
    PatternManager pm = new PatternManager(patternFolderPath);
	pm.AddPattern(pattern); // Pattern Name must be unique

May Also view all current PatternNames, Retreive single or all pattern
objects, and delete patterns

	List<string> patternNames = pm.GetPatternNames();
	Dictionary<string, Pattern> patterns = pm.GetAllPatterns();
	Pattern patternX = pm.GetPattern("patternX");
	pm.DeletePattern("PatternY");

4) Use PR.Utility.Recognizer.cs class
Provide new input to test reproduction of predfined patterns.

	a) Optional - Give a custom PR.Scoring.cs setting class
    b) Recognizer recognizer = new Recognizer();
    recognizer.AddPattern(pattern);
	c) Use the PR.ICompletionListener.cs to listen to completion
	events of the PR.Recognizer.cs class
	
    interface ICompletionListener {
        void PatternCompleted(Recognizer recognizer, string patternID, float completionPercentage);
        string GetCompletionListenerID();
    }

    Pattern pattern1;
    Pattern pattern2;
    Recognizer recognizer = new Recognizer();
    recognizer.AddPattern(pattern1);
    recognizer.AddPattern(pattern2);
    recognizer.Start();
    recognizer.AddStep("x8y1", "RightArm"); recognizer.PrintAttempts();
    recognizer.AddStep("x2y1", "LeftArm"); recognizer.PrintAttempts();
    recognizer.AddStep("x7y1", "RightArm"); recognizer.PrintAttempts();
    recognizer.AddStep("x3y1", "LeftArm"); recognizer.PrintAttempts();
    recognizer.Pause();
    recognizer.UnPause();
    recognizer.AddStep("x7y2", "RightArm"); recognizer.PrintAttempts();
    recognizer.AddStep("x7y7", "RightArm"); recognizer.PrintAttempts();
    recognizer.AddStep("x3y7", "LeftArm"); recognizer.PrintAttempts();
    recognizer.AddStep("x8y8", "RightArm"); recognizer.PrintAttempts();
    recognizer.AddStep("x2y8", "LeftArm"); recognizer.PrintAttempts();
    recognizer.Stop();

	