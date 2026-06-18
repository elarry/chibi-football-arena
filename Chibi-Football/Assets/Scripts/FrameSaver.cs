using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class FrameSaver : MonoBehaviour
{
    const string k_SaveFramesFlag = "--save-frames";
    const string k_FramesFpsFlag = "--frames-fps";

    string m_OutputDir;
    int m_TargetFps = 30;
    int m_FrameCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        var args = Environment.GetCommandLineArgs();
        int idx = Array.IndexOf(args, k_SaveFramesFlag);
        if (idx < 0 || idx >= args.Length - 1) return;

        var go = new GameObject("FrameSaver");
        DontDestroyOnLoad(go);
        var saver = go.AddComponent<FrameSaver>();
        saver.m_OutputDir = args[idx + 1].Trim();

        int fpsIdx = Array.IndexOf(args, k_FramesFpsFlag);
        if (fpsIdx >= 0 && fpsIdx < args.Length - 1)
            int.TryParse(args[fpsIdx + 1], out saver.m_TargetFps);
    }

    void Start()
    {
        try
        {
            Directory.CreateDirectory(m_OutputDir);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FrameSaver] Failed to create output directory '{m_OutputDir}': {e.Message}");
            return;
        }
        StartCoroutine(CaptureFrames());
    }

    IEnumerator CaptureFrames()
    {
        float interval = 1f / m_TargetFps;
        float nextCapture = Time.unscaledTime;

        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (Time.unscaledTime < nextCapture) continue;

            try
            {
                // Ensure we read from the screen, not an active render texture
                var prevRT = RenderTexture.active;
                RenderTexture.active = null;

                var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                tex.Apply();

                RenderTexture.active = prevRT;

                File.WriteAllBytes(
                    Path.Combine(m_OutputDir, $"frame_{m_FrameCount:D06}.png"),
                    tex.EncodeToPNG());
                Destroy(tex);

                m_FrameCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"[FrameSaver] Frame capture error: {e.Message}");
            }

            nextCapture += interval;
        }
    }
}
