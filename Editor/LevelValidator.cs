using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FilloPrinci.RetroFpa.Editor
{
    /// <summary>
    /// Checks the scenes in Build Settings: the persistent scene has the
    /// managers the runtime expects, and every level scene has what
    /// <see cref="LevelSceneManager"/> and the save system need (a
    /// <see cref="SpawnPoint"/>, an atmosphere, unique <see cref="SaveableId"/>s,
    /// scene-change triggers that point at a real scene/spawn point).
    /// Scenes are opened additively just for the scan and closed again;
    /// scenes you already have open are left untouched.
    /// </summary>
    internal static class LevelValidator
    {
        // The persistent scene must have LevelSceneManager (that's how it's found);
        // GameBootstrapper is needed to start the game at all; the rest are what
        // other systems quietly assume exist (most call Instance?. and just skip).
        private static readonly (Type type, string why)[] RequiredManagers =
        {
            (typeof(GameBootstrapper), "nothing will ever load the first level"),
            (typeof(GameManager), "pausing will not stop time and the game state stays at Boot"),
        };

        private static readonly (Type type, string why)[] RecommendedManagers =
        {
            (typeof(InventoryManager), "collecting/equipping items will fail"),
            (typeof(DialogueManager), "dialogue triggers will fail"),
            (typeof(SettingsManager), "settings won't be saved/applied"),
            (typeof(StyleManager), "no global visual style or level atmosphere"),
            (typeof(SaveManager), "saving/loading and 'stay collected' won't work"),
            (typeof(AudioManager), "no sound will play"),
        };

        private sealed class SceneInfo
        {
            public string path;
            public string name;
            public readonly HashSet<string> spawnIds = new();
        }

        [MenuItem("Retro FPA/Validate/Levels")]
        private static void ValidateAllMenuItem()
        {
            List<string> messages = ValidateAll();
            Debug.Log(messages.Count == 0
                ? "[LevelValidator] No issues found."
                : $"[LevelValidator] {messages.Count} issue(s) found - see warnings above.");
        }

        public static List<string> ValidateAll()
        {
            var messages = new List<string>();

            if (EditorApplication.isPlaying)
            {
                Report(messages, "Can't validate scenes while in Play Mode.");
                return messages;
            }

            List<string> buildPaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToList();

            var infos = new Dictionary<string, SceneInfo>();
            var persistentPath = (string)null;
            var saveableOwners = new Dictionary<string, string>();

            // Pass 1: scan every scene, remember what later checks need to look up.
            var levelPaths = new List<string>();
            foreach (string path in buildPaths)
            {
                bool wasOpen = SceneManager.GetSceneByPath(path).isLoaded;
                Scene scene = wasOpen ? SceneManager.GetSceneByPath(path) : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    var info = new SceneInfo { path = path, name = Path.GetFileNameWithoutExtension(path) };
                    infos[info.name] = info;

                    if (FindAll<LevelSceneManager>(scene).Any())
                    {
                        persistentPath = path;
                        ValidatePersistent(scene, path, messages);
                    }
                    else
                    {
                        levelPaths.Add(path);
                        foreach (SpawnPoint spawn in FindAll<SpawnPoint>(scene))
                        {
                            info.spawnIds.Add(spawn.Id);
                        }
                    }
                }
                finally
                {
                    if (!wasOpen)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }

            if (persistentPath == null)
            {
                Report(messages, "No scene in Build Settings contains a LevelSceneManager - there is no persistent scene, or it isn't in Build Settings.");
            }

            // Pass 2: validate each level scene (needs pass 1's spawn ids for trigger targets).
            foreach (string path in levelPaths)
            {
                bool wasOpen = SceneManager.GetSceneByPath(path).isLoaded;
                Scene scene = wasOpen ? SceneManager.GetSceneByPath(path) : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    ValidateLevel(scene, path, infos, saveableOwners, messages);
                }
                finally
                {
                    if (!wasOpen)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }

            ValidateScenesOutsideBuild(buildPaths, messages);
            return messages;
        }

        private static void ValidatePersistent(Scene scene, string path, List<string> messages)
        {
            foreach ((Type type, string why) in RequiredManagers)
            {
                if (!FindAll(scene, type).Any())
                {
                    Report(messages, $"{path}: no {type.Name} - {why}.");
                }
            }

            foreach ((Type type, string why) in RecommendedManagers)
            {
                if (!FindAll(scene, type).Any())
                {
                    Report(messages, $"{path}: no {type.Name} - {why}.");
                }
            }

            if (!FindAll<FirstPersonController>(scene).Any())
            {
                Report(messages, $"{path}: no FirstPersonController (Player) - there is nothing to place at a SpawnPoint.");
            }
        }

        private static void ValidateLevel(Scene scene, string path, Dictionary<string, SceneInfo> infos,
            Dictionary<string, string> saveableOwners, List<string> messages)
        {
            var spawnPoints = FindAll<SpawnPoint>(scene).ToList();
            if (spawnPoints.Count == 0)
            {
                Report(messages, $"{path}: no SpawnPoint - the player can't be placed after this level loads.");
            }

            foreach (IGrouping<string, SpawnPoint> group in spawnPoints.GroupBy(s => s.Id).Where(g => g.Count() > 1))
            {
                Report(messages, $"{path}: {group.Count()} SpawnPoints share the id '{group.Key}' - only one will ever be picked.");
            }

            var atmospheres = FindAll<SceneAtmosphere>(scene).ToList();
            if (atmospheres.Count == 0)
            {
                Report(messages, $"{path}: no SceneAtmosphere - this level keeps default fog/skybox instead of its own.");
            }
            else if (atmospheres.Count > 1)
            {
                Report(messages, $"{path}: {atmospheres.Count} SceneAtmosphere components - each level should have one.");
            }

            foreach (SceneAtmosphere atmosphere in atmospheres)
            {
                if (new SerializedObject(atmosphere).FindProperty("profile").objectReferenceValue == null)
                {
                    Report(messages, $"{path}: {PathOf(atmosphere)} has no Scene Atmosphere Profile assigned.");
                }
            }

            if (FindAll<SceneAmbientAudio>(scene).Count() > 1)
            {
                Report(messages, $"{path}: more than one SceneAmbientAudio - they'd fight over the music source.");
            }

            foreach (Type persistentType in FindAll<MonoBehaviour>(scene).Select(m => m.GetType()).Distinct().Where(IsPersistentSingleton))
            {
                Report(messages, $"{path}: contains a {persistentType.Name} - managers belong in the persistent scene; a duplicate is destroyed at runtime.");
            }

            if (FindAll<FirstPersonController>(scene).Any())
            {
                Report(messages, $"{path}: contains a Player (FirstPersonController) - the Player lives in the persistent scene; levels only need a SpawnPoint.");
            }

            foreach (SaveableId saveable in FindAll<SaveableId>(scene))
            {
                if (string.IsNullOrEmpty(saveable.Id))
                {
                    Report(messages, $"{path}: {PathOf(saveable)} has an empty SaveableId (use its context menu > Generate New Id).");
                }
                else if (saveableOwners.TryGetValue(saveable.Id, out string owner))
                {
                    Report(messages, $"{path}: {PathOf(saveable)} has the same SaveableId as {owner} - collecting one would hide both (copy-pasted object? regenerate its id).");
                }
                else
                {
                    saveableOwners[saveable.Id] = $"{path}: {PathOf(saveable)}";
                }
            }

            foreach (Collectible collectible in FindAll<Collectible>(scene))
            {
                if (collectible.GetComponent<SaveableCollectible>() == null)
                {
                    Report(messages, $"{path}: {PathOf(collectible)} is a Collectible without SaveableCollectible - it will reappear when the level is revisited or a save is loaded (fine if it should respawn).");
                }
            }

            foreach (SceneChangeTrigger trigger in FindAll<SceneChangeTrigger>(scene))
            {
                ValidateTarget(trigger, path, infos, messages);
            }

            foreach (InteractableSceneChangeTrigger trigger in FindAll<InteractableSceneChangeTrigger>(scene))
            {
                ValidateTarget(trigger, path, infos, messages);
            }
        }

        private static void ValidateTarget(Component trigger, string path, Dictionary<string, SceneInfo> infos, List<string> messages)
        {
            var so = new SerializedObject(trigger);
            string target = so.FindProperty("targetSceneName").stringValue;
            string spawnId = so.FindProperty("targetSpawnPointId").stringValue;
            string who = $"{PathOf(trigger)} ({trigger.GetType().Name})";

            if (string.IsNullOrEmpty(target))
            {
                Report(messages, $"{path}: {who} has no target scene name.");
            }
            else if (!infos.TryGetValue(target, out SceneInfo info))
            {
                Report(messages, $"{path}: {who} targets '{target}', which isn't an enabled scene in Build Settings.");
            }
            else if (!string.IsNullOrEmpty(spawnId) && !info.spawnIds.Contains(spawnId))
            {
                Report(messages, $"{path}: {who} targets spawn point '{spawnId}' in '{target}', which has none with that id (the first one is used instead).");
            }
        }

        // A scene with a SpawnPoint that LevelSceneManager can never load is almost
        // certainly a level someone forgot to add to Build Settings. Scene Template
        // source scenes are skipped: they aren't meant to be loaded.
        private static void ValidateScenesOutsideBuild(List<string> buildPaths, List<string> messages)
        {
            var templateScenes = new HashSet<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:SceneTemplateAsset"))
            {
                var template = AssetDatabase.LoadAssetAtPath<UnityEditor.SceneTemplate.SceneTemplateAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (template != null && template.templateScene != null)
                {
                    templateScenes.Add(AssetDatabase.GetAssetPath(template.templateScene));
                }
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (buildPaths.Contains(path) || templateScenes.Contains(path))
                {
                    continue;
                }

                bool wasOpen = SceneManager.GetSceneByPath(path).isLoaded;
                Scene scene = wasOpen ? SceneManager.GetSceneByPath(path) : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    if (FindAll<SpawnPoint>(scene).Any())
                    {
                        Report(messages, $"{path}: looks like a level (has a SpawnPoint) but isn't an enabled scene in Build Settings, so it can't be loaded.");
                    }
                }
                finally
                {
                    if (!wasOpen)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }
        }

        private static IEnumerable<T> FindAll<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));

        private static IEnumerable<Component> FindAll(Scene scene, Type type) =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren(type, true));

        private static bool IsPersistentSingleton(Type type)
        {
            for (Type t = type; t != null; t = t.BaseType)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PersistentSingleton<>))
                {
                    return true;
                }
            }

            return false;
        }

        private static string PathOf(Component component)
        {
            var parts = new List<string>();
            for (Transform t = component.transform; t != null; t = t.parent)
            {
                parts.Add(t.name);
            }

            parts.Reverse();
            return string.Join("/", parts);
        }

        private static void Report(List<string> messages, string message)
        {
            messages.Add(message);
            Debug.LogWarning($"[LevelValidator] {message}");
        }
    }
}
