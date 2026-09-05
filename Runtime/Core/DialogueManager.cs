using System;
using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Runs a <see cref="DialogueData"/> node by node. A future dialogue UI
    /// subscribes to the static events below to display the current line
    /// and drive advancement/choice selection — this manager has no UI code
    /// of its own, keeping it decoupled.
    /// </summary>
    public class DialogueManager : PersistentSingleton<DialogueManager>
    {
        /// <summary>Raised when a dialogue starts.</summary>
        public static event Action<DialogueData> DialogueStarted;

        /// <summary>Raised whenever the current node changes (including the first node of a new dialogue).</summary>
        public static event Action<DialogueNode> NodeChanged;

        /// <summary>Raised when the dialogue ends (its last node was reached, or it was interrupted).</summary>
        public static event Action DialogueEnded;

        public DialogueData CurrentDialogue { get; private set; }
        public DialogueNode CurrentNode { get; private set; }
        public bool IsActive => CurrentDialogue != null;

        /// <summary>Starts <paramref name="dialogue"/> from its first node, ending whatever dialogue was already active.</summary>
        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null || dialogue.Nodes.Count == 0)
            {
                Debug.LogWarning("[DialogueManager] Tried to start a null/empty dialogue.", this);
                return;
            }

            if (IsActive)
            {
                EndDialogue();
            }

            CurrentDialogue = dialogue;
            DialogueStarted?.Invoke(dialogue);
            GoToNode(0);
        }

        /// <summary>Advances past the current node using its NextNodeIndex. Only valid when the current node has no choices.</summary>
        public void Advance()
        {
            if (!IsActive)
            {
                return;
            }

            if (CurrentNode.Choices.Count > 0)
            {
                Debug.LogWarning("[DialogueManager] Current node has choices; call SelectChoice instead of Advance.", this);
                return;
            }

            GoToNodeOrEnd(CurrentNode.NextNodeIndex);
        }

        /// <summary>Picks choice <paramref name="choiceIndex"/> on the current node and moves to the node it points to.</summary>
        public void SelectChoice(int choiceIndex)
        {
            if (!IsActive || choiceIndex < 0 || choiceIndex >= CurrentNode.Choices.Count)
            {
                return;
            }

            GoToNodeOrEnd(CurrentNode.Choices[choiceIndex].NextNodeIndex);
        }

        /// <summary>Ends the current dialogue, if any.</summary>
        public void EndDialogue()
        {
            if (!IsActive)
            {
                return;
            }

            CurrentDialogue = null;
            CurrentNode = null;
            DialogueEnded?.Invoke();
        }

        private void GoToNodeOrEnd(int index)
        {
            if (index < 0 || index >= CurrentDialogue.Nodes.Count)
            {
                EndDialogue();
                return;
            }

            GoToNode(index);
        }

        private void GoToNode(int index)
        {
            CurrentNode = CurrentDialogue.Nodes[index];
            NodeChanged?.Invoke(CurrentNode);
        }
    }
}
