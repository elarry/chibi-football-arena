using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuildAutomation
{
    enum BuildType { Training, Evaluation, Inference }

    const string k_Scene = "Assets/Scenes/Soccer-Chibi.unity";
    const string k_TrainingName   = "football-training";
    const string k_EvaluationName = "football-evaluation";
    const string k_InferenceName  = "football-inference";
    const string k_TrainingOnlyTag = "TrainingOnly";
    const string k_SessionPaths = "BuildAutomation_Paths";
    const string k_SessionTags  = "BuildAutomation_OrigTags";

    static readonly List<(GameObject go, string tag)> s_TagsToRestore = new List<(GameObject, string)>();

    // ── Menu items ────────────────────────────────────────────────────────────

    [MenuItem("Build/Build All %&b")]
    static void MenuBuildAll()
    {
        RunBuild(BuildType.Training,    BuildTarget.StandaloneOSX);
        RunBuild(BuildType.Evaluation,  BuildTarget.StandaloneOSX);
        RunBuild(BuildType.Inference,   BuildTarget.StandaloneOSX);
        RunBuild(BuildType.Training,    BuildTarget.StandaloneLinux64);
        RunBuild(BuildType.Evaluation,  BuildTarget.StandaloneLinux64);
        RunBuild(BuildType.Inference,   BuildTarget.StandaloneLinux64);
    }

    [MenuItem("Build/MacOS/Build All MacOS")]
    static void MenuBuildAllMac()
    {
        RunBuild(BuildType.Training,   BuildTarget.StandaloneOSX);
        RunBuild(BuildType.Evaluation, BuildTarget.StandaloneOSX);
        RunBuild(BuildType.Inference,  BuildTarget.StandaloneOSX);
    }

    [MenuItem("Build/MacOS/Build Training MacOS")]
    static void MenuTrainingMac()    => RunBuild(BuildType.Training,   BuildTarget.StandaloneOSX);

    [MenuItem("Build/MacOS/Build Evaluation MacOS")]
    static void MenuEvaluationMac()  => RunBuild(BuildType.Evaluation, BuildTarget.StandaloneOSX);

    [MenuItem("Build/MacOS/Build Inference MacOS")]
    static void MenuInferenceMac()   => RunBuild(BuildType.Inference,  BuildTarget.StandaloneOSX);

    [MenuItem("Build/Linux/Build All Linux")]
    static void MenuBuildAllLinux()
    {
        RunBuild(BuildType.Training,   BuildTarget.StandaloneLinux64);
        RunBuild(BuildType.Evaluation, BuildTarget.StandaloneLinux64);
        RunBuild(BuildType.Inference,  BuildTarget.StandaloneLinux64);
    }

    [MenuItem("Build/Linux/Build Training Linux")]
    static void MenuTrainingLinux()   => RunBuild(BuildType.Training,   BuildTarget.StandaloneLinux64);

    [MenuItem("Build/Linux/Build Evaluation Linux")]
    static void MenuEvaluationLinux() => RunBuild(BuildType.Evaluation, BuildTarget.StandaloneLinux64);

    [MenuItem("Build/Linux/Build Inference Linux")]
    static void MenuInferenceLinux()  => RunBuild(BuildType.Inference,  BuildTarget.StandaloneLinux64);

    // ── Build runner ──────────────────────────────────────────────────────────

    static void RunBuild(BuildType type, BuildTarget target)
    {
        var baseName = type == BuildType.Training   ? k_TrainingName
                     : type == BuildType.Evaluation ? k_EvaluationName
                     : k_InferenceName;

        string platformDir, executableName;
        if (target == BuildTarget.StandaloneLinux64)
        {
            platformDir    = "Linux";
            executableName = baseName + ".x86_64";
        }
        else
        {
            platformDir    = "MacOS";
            executableName = baseName + ".app";
        }

        var absolutePath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", "..", "Builds", platformDir, executableName));

        if (Directory.Exists(absolutePath))
            Directory.Delete(absolutePath, true);

        Debug.Log($"[BuildAutomation] Starting {type} build → {absolutePath}");

        AssetDatabase.Refresh();
        ApplyTagModifications(type);
        try
        {
            var options = new BuildPlayerOptions
            {
                scenes = new[] { k_Scene },
                locationPathName = absolutePath,
                target = target,
                options = type == BuildType.Inference ? BuildOptions.None : BuildOptions.Development,
                extraScriptingDefines = type == BuildType.Training   ? new[] { "TRAINING_BUILD" }
                                      : type == BuildType.Evaluation ? new[] { "EVALUATION_BUILD" }
                                      : new string[0]
            };

            var report = BuildPipeline.BuildPlayer(options);
            var result = report.summary.result;

            if (result == BuildResult.Succeeded)
                Debug.Log($"[BuildAutomation] {type} build succeeded → {absolutePath}");
            else
                Debug.LogError($"[BuildAutomation] {type} build FAILED ({result}). Check the Console for errors.");
        }
        finally
        {
            RestoreTagModifications();
        }
    }

    // ── Tag helpers ───────────────────────────────────────────────────────────

    static void ApplyTagModifications(BuildType type)
    {
        s_TagsToRestore.Clear();

        GameObject[] objects;
        try
        {
            objects = GameObject.FindGameObjectsWithTag(k_TrainingOnlyTag);
        }
        catch (UnityException)
        {
            Debug.LogWarning($"[BuildAutomation] Tag '{k_TrainingOnlyTag}' not found. " +
                             "Add it in Project Settings → Tags & Layers and retag the extra SoccerField instances.");
            return;
        }

        if (objects.Length == 0)
        {
            Debug.LogWarning($"[BuildAutomation] No objects tagged '{k_TrainingOnlyTag}' found.");
            return;
        }

        var targetTag = type == BuildType.Inference ? "EditorOnly" : "Untagged";
        var paths    = new List<string>();
        var origTags = new List<string>();

        foreach (var go in objects)
        {
            var path = HierarchyPath(go);
            s_TagsToRestore.Add((go, go.tag));
            paths.Add(path);
            origTags.Add(go.tag);
            go.tag = targetTag;
        }

        // Persist across potential domain reload
        SessionState.SetString(k_SessionPaths, string.Join("|", paths));
        SessionState.SetString(k_SessionTags,  string.Join("|", origTags));

        // Write to disk — BuildPipeline reads the scene file, not in-memory state
        SaveScene();

        Debug.Log($"[BuildAutomation] {objects.Length} TrainingOnly objects → '{targetTag}' for {type} build.");
    }

    static void RestoreTagModifications()
    {
        bool restored = false;

        // Primary: use cached in-memory references
        foreach (var (go, originalTag) in s_TagsToRestore)
        {
            if (go != null)
            {
                go.tag = originalTag;
                restored = true;
            }
        }

        // Fallback: SessionState paths survive domain reloads
        if (!restored)
        {
            var pathsStr = SessionState.GetString(k_SessionPaths, "");
            var tagsStr  = SessionState.GetString(k_SessionTags,  "");
            if (!string.IsNullOrEmpty(pathsStr))
            {
                var paths = pathsStr.Split('|');
                var tags  = tagsStr.Split('|');
                for (int i = 0; i < paths.Length && i < tags.Length; i++)
                {
                    var go = GameObject.Find(paths[i]);
                    if (go != null)
                    {
                        go.tag = tags[i];
                        restored = true;
                    }
                }
            }
        }

        if (restored)
        {
            // Write restored state to disk so the scene doesn't reload with modified tags
            SaveScene();
            Debug.Log("[BuildAutomation] TrainingOnly object tags restored.");
        }

        SessionState.EraseString(k_SessionPaths);
        SessionState.EraseString(k_SessionTags);
        s_TagsToRestore.Clear();
    }

    static void SaveScene()
    {
        var scene = EditorSceneManager.GetSceneByPath(k_Scene);
        if (scene.IsValid() && scene.isLoaded)
            EditorSceneManager.SaveScene(scene);
    }

    static string HierarchyPath(GameObject go)
    {
        var path   = go.name;
        var parent = go.transform.parent;
        while (parent != null)
        {
            path   = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
}
