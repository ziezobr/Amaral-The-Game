using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using PontoCego.WorldEnvironment;

namespace PontoCego
{
    /// <summary>Opt-in release benchmark. No captures or disk writes occur during frame sampling.</summary>
    public sealed class WorldBenchmark2D : MonoBehaviour
    {
        const string Flag = "--ponto-world-benchmark";
        string output;
        float started;
        bool complete;
        readonly List<float> allFrames = new List<float>(4096);
        readonly List<Sample> samples = new List<Sample>();
        [Serializable] public sealed class Sample
        {
            public string location;
            public int frames, sceneryCached, sceneryCulled, spriteRenderers;
            public float medianMilliseconds, p95Milliseconds, worstMilliseconds;
            public double visibilityEvaluationMilliseconds;
        }
        [Serializable] public sealed class Report
        {
            public string version, utc, gpu, cpu, platform, note;
            public int width, height, processorCount, systemMemoryMB, graphicsMemoryMB, targetFPS, vSync, frames;
            public float sampleSeconds, medianMilliseconds, p95Milliseconds, worstMilliseconds;
            public Sample[] locations;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, Flag);
            if (i < 0 || i + 1 >= args.Length) return;
            var go = new GameObject("QA / benchmark 2D opt-in"); DontDestroyOnLoad(go);
            go.AddComponent<WorldBenchmark2D>().output = args[i + 1];
        }
        void Awake() { Application.runInBackground=true; started = Time.realtimeSinceStartup; }
        void Update()
        {
            var input = FindAnyObjectByType<PlayerInputReader>();
            if (input && input.Paused) input.SetPaused(false);
            if (!complete && Time.realtimeSinceStartup - started > 100)
            {
                Debug.LogError("PONTO_BENCHMARK_TIMEOUT");
                complete = true; Application.Quit(2);
            }
        }
        IEnumerator Start()
        {
            yield return null;
            GameSession.ExploreNeighborhood("01_Riberal");
            yield return new WaitForSecondsRealtime(3);
            var world = WorldHub.Instance;
            var player = FindAnyObjectByType<PlayerMotor>();
            if (!world || !player) { Debug.LogError("PONTO_BENCHMARK_NO_WORLD"); Application.Quit(2); yield break; }
            var input = player.GetComponent<PlayerInputReader>();
            input.SetPaused(false);
            input.Mobile(Vector2.zero, Vector2.down, false, false, false, false);
            player.GetComponent<StoryDialogue>()?.DismissForValidation();
            if (DayNightCycle.Instance) { DayNightCycle.Instance.frozen = true; DayNightCycle.Instance.SetHour(21); }
            if (WorldWeather2D.Instance) { WorldWeather2D.Instance.automaticWeather = false; WorldWeather2D.Instance.SetWeather(WeatherKind.Storm, true); }
            var points = new[] { new Vector2(-37, -25), new Vector2(90, 4), new Vector2(-52, -72) };
            var names = new[] { "Centro / conexão Riberal", "Leste / galpões e comércio", "Sul / acesso Mocambo" };
            for (int location = 0; location < points.Length; location++)
            {
                // QA-only relocation precedes warmup; sample windows contain no teleports or screenshots.
                Vector2 point = points[location];
                player.Body.position = point; player.transform.position = point; player.Body.linearVelocity = Vector2.zero;
                Physics2D.SyncTransforms();
                yield return new WaitForSecondsRealtime(2);
                var frames = new List<float>(1024);
                input.Mobile(location==0?Vector2.up:Vector2.zero,Vector2.up,false,false,false,false);
                float end = Time.realtimeSinceStartup + 8;
                while (Time.realtimeSinceStartup < end)
                {
                    yield return null;
                    float ms = Time.unscaledDeltaTime * 1000;
                    frames.Add(ms); allFrames.Add(ms);
                }
                input.Mobile(Vector2.zero,Vector2.down,false,false,false,false);
                var visibility = FindAnyObjectByType<WorldVisibility2D>();
                frames.Sort();
                samples.Add(new Sample { location = names[location], frames = frames.Count,
                    medianMilliseconds = Percentile(frames, .5f), p95Milliseconds = Percentile(frames, .95f),
                    worstMilliseconds = Percentile(frames, 1), sceneryCached = visibility ? visibility.CachedCount : 0,
                    sceneryCulled = visibility ? visibility.CulledCount : 0,
                    visibilityEvaluationMilliseconds = visibility ? visibility.LastEvaluationMilliseconds : 0,
                    spriteRenderers = FindObjectsByType<SpriteRenderer>().Length });
            }
            allFrames.Sort();
            var report = new Report { version = Application.version, utc = DateTime.UtcNow.ToString("o"),
                gpu = SystemInfo.graphicsDeviceName, cpu = SystemInfo.processorType, platform = Application.platform.ToString(),
                width = Screen.width, height = Screen.height, processorCount = SystemInfo.processorCount,
                systemMemoryMB = SystemInfo.systemMemorySize, graphicsMemoryMB = SystemInfo.graphicsMemorySize,
                targetFPS = Application.targetFrameRate, vSync = QualitySettings.vSyncCount, frames = allFrames.Count,
                sampleSeconds = 24, medianMilliseconds = Percentile(allFrames, .5f),
                p95Milliseconds = Percentile(allFrames, .95f), worstMilliseconds = Percentile(allFrames, 1),
                locations = samples.ToArray(),
                note = "Windows release player; three camera locations (first moves north through the center), nighttime storm; 2s warmup per location. Frame times include the configured frame cap/vSync. No screenshots during sampling. Results apply to this device and window size." };
            string directory = Path.GetDirectoryName(Path.GetFullPath(output));
            Directory.CreateDirectory(directory);
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            Debug.Log("PONTO_BENCHMARK_OK / " + output);
            complete = true; Application.Quit();
        }
        static float Percentile(List<float> sorted, float value)
        {
            if (sorted.Count == 0) return 0;
            return sorted[Mathf.Clamp(Mathf.CeilToInt((sorted.Count - 1) * value), 0, sorted.Count - 1)];
        }
    }
}
