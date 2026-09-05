using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FilloPrinci.RetroFpa.Editor
{
    /// <summary>
    /// Custom editor for <see cref="DialogueData"/>: replaces the raw,
    /// error-prone "Next Node Index" integer fields with dropdowns listing
    /// every node (by index + a text preview) plus "End Dialogue", so
    /// authors never have to count node indices by hand.
    /// </summary>
    [CustomEditor(typeof(DialogueData))]
    public class DialogueDataEditor : UnityEditor.Editor
    {
        private DialogueData Data => (DialogueData)target;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty nodesProp = serializedObject.FindProperty(nameof(DialogueData.Nodes));

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < Data.Nodes.Count; i++)
            {
                DrawNode(nodesProp, i);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(Data);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Node"))
            {
                Undo.RecordObject(Data, "Add Dialogue Node");
                Data.Nodes.Add(new DialogueNode());
                EditorUtility.SetDirty(Data);
            }
        }

        private void DrawNode(SerializedProperty nodesProp, int index)
        {
            DialogueNode node = Data.Nodes[index];
            SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Node {index}", EditorStyles.boldLabel);

            node.SpeakerName = EditorGUILayout.TextField("Speaker", node.SpeakerName);
            EditorGUILayout.PropertyField(nodeProp.FindPropertyRelative(nameof(DialogueNode.Text)), new GUIContent("Text"));

            if (node.Choices.Count == 0)
            {
                node.NextNodeIndex = DrawNodeDropdown("Next Node", node.NextNodeIndex);
            }
            else
            {
                DrawChoices(nodeProp, node);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Choice"))
            {
                Undo.RecordObject(Data, "Add Dialogue Choice");
                node.Choices.Add(new DialogueChoice());
                EditorUtility.SetDirty(Data);
            }
            if (GUILayout.Button("Remove Node"))
            {
                Undo.RecordObject(Data, "Remove Dialogue Node");
                Data.Nodes.RemoveAt(index);
                EditorUtility.SetDirty(Data);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawChoices(SerializedProperty nodeProp, DialogueNode node)
        {
            SerializedProperty choicesProp = nodeProp.FindPropertyRelative(nameof(DialogueNode.Choices));

            EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
            for (int c = 0; c < node.Choices.Count; c++)
            {
                DialogueChoice choice = node.Choices[c];
                SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(c);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(
                    choiceProp.FindPropertyRelative(nameof(DialogueChoice.ChoiceText)),
                    new GUIContent("Choice Text"));

                choice.NextNodeIndex = DrawNodeDropdown("Next Node", choice.NextNodeIndex);

                if (GUILayout.Button("Remove Choice"))
                {
                    Undo.RecordObject(Data, "Remove Dialogue Choice");
                    node.Choices.RemoveAt(c);
                    EditorUtility.SetDirty(Data);
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndVertical();
            }
        }

        private int DrawNodeDropdown(string label, int currentValue)
        {
            var options = new List<string> { "End Dialogue" };
            var values = new List<int> { -1 };

            for (int i = 0; i < Data.Nodes.Count; i++)
            {
                string preview = Data.Nodes[i].SpeakerName;
                options.Add($"Node {i}: {(string.IsNullOrEmpty(preview) ? "(no speaker)" : preview)}");
                values.Add(i);
            }

            int currentSelection = values.IndexOf(currentValue);
            if (currentSelection < 0)
            {
                currentSelection = 0;
            }

            int selected = EditorGUILayout.Popup(label, currentSelection, options.ToArray());
            return values[selected];
        }
    }
}
