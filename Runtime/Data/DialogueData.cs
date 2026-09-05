using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace FilloPrinci.RetroFpa
{
    /// <summary>One selectable reply on a <see cref="DialogueNode"/>.</summary>
    [Serializable]
    public class DialogueChoice
    {
        public LocalizedString ChoiceText;

        [Tooltip("Index into the DialogueData's Nodes list. -1 ends the dialogue.")]
        public int NextNodeIndex = -1;
    }

    /// <summary>One line of dialogue, optionally branching into player choices.</summary>
    [Serializable]
    public class DialogueNode
    {
        [Tooltip("Character name shown alongside the line. Not localized (yet) — usually a proper noun.")]
        public string SpeakerName;

        public LocalizedString Text;

        [Tooltip("Used when Choices is empty: index of the next node to auto-advance to. -1 ends the dialogue.")]
        public int NextNodeIndex = -1;

        public List<DialogueChoice> Choices = new();
    }

    /// <summary>
    /// A dialogue as a flat, index-linked list of nodes — deliberately a
    /// lightweight custom format (not a third-party dialogue plugin/graph
    /// tool). The value of this system is meant to be in its authoring tool
    /// (an Editor wizard/validator, built separately), not in the runtime
    /// data structure itself.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueData", menuName = "Retro FPA/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public List<DialogueNode> Nodes = new();
    }
}
