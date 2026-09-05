using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FilloPrinci.RetroFpa.Editor
{
    /// <summary>
    /// Dock window tying together the project's content validators, so
    /// authors don't need to remember menu items or dig through the Console.
    /// </summary>
    public class RetroFpaWindow : EditorWindow
    {
        private readonly List<string> results = new();
        private Vector2 scroll;

        [MenuItem("Window/Retro FPA/Dashboard")]
        private static void Open()
        {
            GetWindow<RetroFpaWindow>("Retro FPA");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Validators", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Validate Dialogues"))
                {
                    ShowResults(DialogueValidator.ValidateAll());
                }

                if (GUILayout.Button("Validate Items"))
                {
                    ShowResults(ItemValidator.ValidateAll());
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (string message in results)
            {
                EditorGUILayout.HelpBox(message, MessageType.Warning);
            }
            EditorGUILayout.EndScrollView();
        }

        private void ShowResults(List<string> messages)
        {
            results.Clear();
            results.AddRange(messages);
            if (results.Count == 0)
            {
                results.Add("No issues found.");
            }

            Repaint();
        }
    }
}
