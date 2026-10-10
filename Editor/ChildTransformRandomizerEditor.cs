using UnityEditor;
using UnityEngine;

namespace FilloPrinci.RetroFpa.Editor
{
    /// <summary>
    /// Custom editor for <see cref="ChildTransformRandomizer"/>: adds the
    /// buttons that apply it in the Editor (with Undo), so the randomized
    /// children are saved with the scene.
    /// </summary>
    [CustomEditor(typeof(ChildTransformRandomizer))]
    public class ChildTransformRandomizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Randomize (New Seed)"))
                {
                    serializedObject.Update();
                    serializedObject.FindProperty("seed").intValue = Random.Range(int.MinValue, int.MaxValue);
                    serializedObject.ApplyModifiedProperties();
                    Apply();
                }

                if (GUILayout.Button("Apply Current Seed"))
                {
                    Apply();
                }
            }
        }

        private void Apply()
        {
            var randomizer = (ChildTransformRandomizer)target;
            Transform parent = randomizer.transform;

            var children = new Object[parent.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = parent.GetChild(i);
            }

            Undo.RecordObjects(children, "Randomize Children");
            randomizer.Randomize();

            // Children that are prefab instances only keep the new values as
            // overrides if told so explicitly.
            foreach (Object child in children)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(child);
            }
        }
    }
}
