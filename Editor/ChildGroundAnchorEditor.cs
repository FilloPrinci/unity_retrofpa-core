using UnityEditor;
using UnityEngine;

namespace FilloPrinci.RetroFpa.Editor
{
    /// <summary>
    /// Custom editor for <see cref="ChildGroundAnchor"/>: adds the button
    /// that applies it in the Editor (with Undo), so the anchored children
    /// are saved with the scene.
    /// </summary>
    [CustomEditor(typeof(ChildGroundAnchor))]
    public class ChildGroundAnchorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Anchor Children To Ground"))
            {
                Apply();
            }
        }

        private void Apply()
        {
            var anchor = (ChildGroundAnchor)target;
            Transform parent = anchor.transform;

            var children = new Object[parent.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = parent.GetChild(i);
            }

            Undo.RecordObjects(children, "Anchor Children To Ground");
            int missed = anchor.Anchor();

            // Children that are prefab instances only keep the new values as
            // overrides if told so explicitly.
            foreach (Object child in children)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(child);
            }

            if (missed > 0)
            {
                Debug.LogWarning($"[ChildGroundAnchor] {missed} of {children.Length} children found no ground below them and were left in place. The ground needs a collider on a layer in Ground Mask.", anchor);
            }
        }
    }
}
