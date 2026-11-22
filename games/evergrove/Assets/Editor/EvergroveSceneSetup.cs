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

        [MenuItem("Evergrove/Verify C# Game and Scene")]
        public static void VerifyProject()
        {
            try
            {
                CreateScene();
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                    throw new InvalidOperationException("Playtest scene asset is missing.");
                if (PuzzleCatalog.Count != 12)
                    throw new InvalidOperationException("Expected the twelve authored streams.");
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
