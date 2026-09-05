using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FilloPrinci.RetroFpa.Editor
{
    /// <summary>Scans every <see cref="DialogueData"/> asset in the project for structural issues.</summary>
    internal static class DialogueValidator
    {
        [MenuItem("Retro FPA/Validate/Dialogues")]
        private static void ValidateAllMenuItem() => ValidateAll();

        public static List<string> ValidateAll()
        {
            var messages = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:DialogueData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
                ValidateDialogue(dialogue, path, messages);
            }

            return messages;
        }

        private static void ValidateDialogue(DialogueData dialogue, string path, List<string> messages)
        {
            if (dialogue.Nodes.Count == 0)
            {
                Report(messages, dialogue, $"{path}: has no nodes.");
                return;
            }

            HashSet<int> reachable = FindReachableNodes(dialogue);

            for (int i = 0; i < dialogue.Nodes.Count; i++)
            {
                DialogueNode node = dialogue.Nodes[i];

                if (!reachable.Contains(i))
                {
                    Report(messages, dialogue, $"{path}: node {i} is unreachable from node 0.");
                }

                if (node.Choices.Count == 0)
                {
                    ValidateTarget(dialogue, path, messages, node.NextNodeIndex, $"node {i}'s Next Node Index");
                }
                else
                {
                    for (int c = 0; c < node.Choices.Count; c++)
                    {
                        ValidateTarget(dialogue, path, messages, node.Choices[c].NextNodeIndex, $"node {i}, choice {c}'s Next Node Index");
                    }
                }
            }
        }

        private static HashSet<int> FindReachableNodes(DialogueData dialogue)
        {
            var reachable = new HashSet<int>();
            var toVisit = new Queue<int>();
            toVisit.Enqueue(0);

            while (toVisit.Count > 0)
            {
                int index = toVisit.Dequeue();
                if (index < 0 || index >= dialogue.Nodes.Count || !reachable.Add(index))
                {
                    continue;
                }

                DialogueNode node = dialogue.Nodes[index];
                if (node.Choices.Count == 0)
                {
                    toVisit.Enqueue(node.NextNodeIndex);
                }
                else
                {
                    foreach (DialogueChoice choice in node.Choices)
                    {
                        toVisit.Enqueue(choice.NextNodeIndex);
                    }
                }
            }

            return reachable;
        }

        private static void ValidateTarget(DialogueData dialogue, string path, List<string> messages, int target, string label)
        {
            if (target != -1 && (target < 0 || target >= dialogue.Nodes.Count))
            {
                Report(messages, dialogue, $"{path}: {label} ({target}) is out of range.");
            }
        }

        private static void Report(List<string> messages, Object context, string message)
        {
            messages.Add(message);
            Debug.LogWarning($"[DialogueValidator] {message}", context);
        }
    }
}
