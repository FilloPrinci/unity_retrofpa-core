using UnityEditor;
using UnityEngine;

namespace FilloPrinci.RetroFpa.Editor
{
    /// <summary>
    /// Custom editor for <see cref="VisualStyleProfile"/>: groups fog color
    /// and skybox horizon/zenith colors together at the top of the
    /// Inspector (instead of under separate "Fog"/"Skybox" headers), with a
    /// button to copy the fog color onto the skybox horizon - the two are
    /// meant to match so geometry fades into the sky instead of cutting
    /// against a mismatched horizon.
    /// </summary>
    [CustomEditor(typeof(VisualStyleProfile))]
    public class VisualStyleProfileEditor : UnityEditor.Editor
    {
        private static readonly string[] FogAndSkyFieldNames =
        {
            "fogColor", "skyboxHorizonColor", "skyboxZenithColor",
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty fogColorProp = serializedObject.FindProperty("fogColor");
            SerializedProperty horizonColorProp = serializedObject.FindProperty("skyboxHorizonColor");
            SerializedProperty zenithColorProp = serializedObject.FindProperty("skyboxZenithColor");

            EditorGUILayout.LabelField("Fog & Sky", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Skybox Horizon should usually match Fog Color, so geometry fades " +
                "into the sky at the horizon instead of cutting against it.",
                MessageType.None);

            EditorGUILayout.PropertyField(fogColorProp);
            EditorGUILayout.PropertyField(horizonColorProp);
            EditorGUILayout.PropertyField(zenithColorProp);

            if (GUILayout.Button("Sync Skybox Horizon <- Fog Color"))
            {
                horizonColorProp.colorValue = fogColorProp.colorValue;
            }

            EditorGUILayout.Space();
            string[] excluded = { "m_Script", FogAndSkyFieldNames[0], FogAndSkyFieldNames[1], FogAndSkyFieldNames[2] };
            DrawPropertiesExcluding(serializedObject, excluded);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
