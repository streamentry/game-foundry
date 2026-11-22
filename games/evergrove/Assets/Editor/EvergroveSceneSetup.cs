using System;
using System.IO;
using Evergrove.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Evergrove.UnityClient.Editor
{
    /// <summary>
    /// A predictable one-click editor entrypoint; no scene, licensed prefabs,
    /// generated binary files or private Unity credentials are committed.
    /// </summary>
    public static class EvergroveSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/Evergrove.unity";

        [MenuItem("Evergrove/Create or Open Playtest Scene")]
        public static void CreateScene()
        {
            if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
            Scene scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!scene.IsValid()) throw new InvalidOperationException("Unable to open a Unity scene.");
            if (!File.Exists(ScenePath) && !EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Unable to write the Evergrove playtest scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.Refresh();
            Debug.Log("Evergrove playtest scene ready: " + ScenePath);
        }

        // Shared by the local Editor smoke and the GameCI build path. This
        // deliberately uses REAL Unity JsonUtility and imported TextAssets;
        // .NET System.Text.Json checks cannot validate Unity serialization.
        public static void ValidateImportedProject()
        {
            if (Application.unityVersion != "6000.3.0f1")
                throw new InvalidOperationException(
                    "Exact pinned Unity Editor 6000.3.0f1 required; installed: " +
                    Application.unityVersion);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                throw new InvalidOperationException("Playtest scene asset is missing.");
            if (PuzzleCatalog.Count != 12)
                throw new InvalidOperationException("Expected twelve authored streams.");
            for (int i = 0; i < PuzzleCatalog.Count; i++)
            {
                Puzzle board = PuzzleCatalog.Get(i);
                if (!PipeRules.IsComplete(board, board.Solution) ||
                    PipeRules.CountSolutions(board) != 1)
                    throw new InvalidOperationException("Invalid puzzle: chapter " + (i + 1));
            }

            WorldModel model = WorldModel.New();
            if (!model.Rotate(0) || !model.Undo())
                throw new InvalidOperationException("World interaction smoke test failed.");
            AssertUnitySnapshotRoundTrip(model, "fresh v4");
            // Exercise nested choice arrays, transformed grids and staged
            // hint fields, not only the zero-choice first chapter snapshot.
            Puzzle first = model.CurrentPuzzle;
            for (int i = 0; i < first.CellCount; i++)
            {
                int turns = 0;
                while (model.Rotations[i] != first.Solution[i])
                {
                    if (!model.Rotate(i) || ++turns > 4)
                        throw new InvalidOperationException("Cannot solve Editor smoke fixture.");
                }
            }
            if (!model.Choose(VillageChoice.ShadeTree) ||
                model.AdvanceGentleHint() != 1)
                throw new InvalidOperationException("Could not create branched v4 snapshot.");
            AssertUnitySnapshotRoundTrip(model, "branched v4 with staged hint");

            int loadedLocales = 0;
            foreach (GameLanguage language in GameLocalization.Available)
            {
                if (language.Code == GameLocalization.Fallback) continue;
                TextAsset asset = Resources.Load<TextAsset>(
                    "Evergrove/Locales/" + language.Code);
                if (asset == null)
                    throw new InvalidOperationException(
                        "Unity Resources missing locale: " + language.Code);
                TranslationBundle bundle = JsonUtility.FromJson<TranslationBundle>(asset.text);
                if (bundle == null ||
                    !string.Equals(bundle.code, language.Code, StringComparison.Ordinal) ||
                    !GameLocalization.Register(bundle))
                    throw new InvalidOperationException(
                        "Unity JsonUtility cannot register imported locale: " + language.Code);
                loadedLocales++;
            }
            GameLocalization.SetCurrent(GameLocalization.Fallback);
            if (loadedLocales != GameLocalization.Available.Count - 1)
                throw new InvalidOperationException("Imported locale count mismatch.");
            Debug.Log("EVERGROVE_UNITY_IMPORT_CHECK_OK editor=" + Application.unityVersion +
                " puzzles=" + PuzzleCatalog.Count + " importedLocales=" + loadedLocales +
                " snapshotSchema=" + WorldModel.Version);
        }

        private static void AssertUnitySnapshotRoundTrip(WorldModel before, string label)
        {
            WorldSnapshot exported = before.Export();
            string json = JsonUtility.ToJson(exported);
            WorldSnapshot snapshot = JsonUtility.FromJson<WorldSnapshot>(json);
            WorldModel restored;
            if (!WorldModel.TryRestore(snapshot, out restored) ||
                restored.Round != before.Round ||
                restored.BoardVariant != before.BoardVariant ||
                restored.HintStep != before.HintStep ||
                restored.HintTile != before.HintTile ||
                restored.Choices.Length != before.Choices.Length ||
                restored.Rotations.Length != before.Rotations.Length)
                throw new InvalidOperationException(
                    "Unity JsonUtility snapshot round-trip failed: " + label);
            for (int i = 0; i < before.Rotations.Length; i++)
                if (restored.Rotations[i] != before.Rotations[i])
                    throw new InvalidOperationException("Unity snapshot changed tile " + i);
            for (int i = 0; i < before.Choices.Length; i++)
                if (restored.Choices[i] != before.Choices[i])
                    throw new InvalidOperationException("Unity snapshot changed choice " + i);
        }

        [MenuItem("Evergrove/Verify C# Game and Scene")]
        public static void VerifyProject()
        {
            try
            {
                CreateScene();
                ValidateImportedProject();
                Debug.Log("Evergrove Unity Editor compilation, scene setup and puzzle smoke passed.");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                else throw;
            }
        }
    }
}
