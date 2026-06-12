using System;
using System.Collections.Generic;

namespace Dialogues
{
    [Serializable]
    public class DialogueLine
    {
        public string author;
        public string text;
    }

    [Serializable]
    public class DialogueContainer
    {
        public List<DialogueLine> lines;
    }
}