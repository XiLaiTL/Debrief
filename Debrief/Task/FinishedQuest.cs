using System;
using System.Collections.Generic;

namespace Debrief
{
    [Serializable]
    public class FinishedQuest
    {
        public string Quest { get; }
        public List<string> Tasks { get; }
        
        public FinishedQuest(string quest, List<string> tasks)
        {
            Quest = quest;
            Tasks = tasks;
        }
    }
    
}