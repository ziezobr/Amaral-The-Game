using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PontoCego.WorldEnvironment;

namespace PontoCego
{
    /// <summary>
    /// Stylized synthesized ambience, mixed from the shared world clock. The supplied menu and
    /// chase songs are separate sources and are never replaced or modified by this component.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldAmbience2D : MonoBehaviour
    {
        public float DayGain { get; private set; }
        public float NightGain { get; private set; }
        public bool Paused { get; private set; }
        public int SpatialEmitterCount => emitters.Count;
        public AudioSource DaySource { get; private set; }
        public AudioSource NightSource { get; private set; }
        AudioSource wind, owl, rain;
        AudioClip birdsClip, cricketsClip, windClip, owlClip, waterClip, humClip, fireClip, rainClip;
        PlayerInputReader input;
        Camera view;
        float nextAnchors;
        readonly List<Anchor> anchors = new List<Anchor>();
        readonly List<AudioSource> emitters = new List<AudioSource>();
        readonly List<int> emitterKinds = new List<int>();
        readonly HashSet<Transform> anchored = new HashSet<Transform>();
        struct Anchor { public Transform at; public int kind; public Vector3 offset; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Install();
        }
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) { Install(); }
        static void Install()
        {
            if (!FindAnyObjectByType<PlayerMotor>() || FindAnyObjectByType<WorldAmbience2D>()) return;
            new GameObject("Áudio / paisagem sonora dinâmica 2D").AddComponent<WorldAmbience2D>();
        }
        void Start()
        {
            input = FindAnyObjectByType<PlayerInputReader>();
            view = Camera.main;
            birdsClip = Synthesize("Dia / pássaros", 8, 0);
            cricketsClip = Synthesize("Noite / grilos", 7, 1);
            windClip = Synthesize("Ambiente / vento", 8, 2);
            owlClip = Synthesize("Noite / coruja distante", 16, 3);
            waterClip = Synthesize("Local / água corrente", 6, 4);
            humClip = Synthesize("Local / iluminação elétrica", 4, 5);
            fireClip = Synthesize("Local / fogueira", 6, 6);
            rainClip = Synthesize("Clima / chuva", 8, 7);
            DaySource = Loop("Pássaros", birdsClip);
            NightSource = Loop("Grilos", cricketsClip);
            wind = Loop("Vento", windClip);
            owl = Loop("Coruja", owlClip);
            rain = Loop("Chuva", rainClip);
            CacheAnchors();
            RefreshEmitters();
        }
        AudioSource Loop(string sourceName, AudioClip clip, bool spatial = false)
        {
            var go = new GameObject(sourceName); go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false; source.loop = true; source.clip = clip;
            source.spatialBlend = spatial ? 1 : 0;
            source.dopplerLevel = 0; source.volume = 0; source.priority = spatial ? 220 : 180;
            source.rolloffMode = AudioRolloffMode.Logarithmic; source.minDistance = 1.5f; source.maxDistance = 18;
            source.Play();
            return source;
        }

        /// <summary>Equal-power crossfade: energy remains consistent during dawn and dusk.</summary>
        public static Vector2 MixForDaylight(float daylight)
        {
            float angle = Mathf.Clamp01(daylight) * Mathf.PI * .5f;
            return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        void Update()
        {
            if (!DaySource) return;
            bool pause = Time.timeScale <= 0 || (input && input.Paused);
            if (pause != Paused) SetPaused(pause);
            if (Paused) return;
            float daylight = DayNightCycle.Instance ? DayNightCycle.Instance.Daylight : 1;
            Vector2 mix = MixForDaylight(daylight);
            DayGain = Mathf.MoveTowards(DayGain, mix.x, Time.unscaledDeltaTime * .7f);
            NightGain = Mathf.MoveTowards(NightGain, mix.y, Time.unscaledDeltaTime * .7f);
            float effects = Mathf.Clamp01(GamePreferences.Effects);
            var weather = WorldWeather2D.Instance;
            float rainfall = weather ? weather.Rain : 0, windStrength = weather ? weather.Wind : .15f;
            bool sheltered = weather && input && weather.IsSheltered(input.transform.position);
            float exposure = sheltered ? .24f : 1;
            effects *= exposure;
            DaySource.volume = .095f * DayGain * effects * (1 - rainfall * .75f);
            NightSource.volume = .065f * NightGain * effects;
            wind.volume = Mathf.Lerp(.035f, .16f, windStrength) * effects;
            rain.volume = rainfall * .24f * effects;
            owl.volume = .05f * NightGain * effects;
            for (int i = 0; i < emitters.Count; i++)
                emitters[i].volume = effects * (emitterKinds[i] == 1 ? .018f * NightGain : emitterKinds[i] == 2 ? .10f : .12f);
            if (Time.unscaledTime >= nextAnchors) { nextAnchors = Time.unscaledTime + 2; RefreshEmitters(); }
        }
        void SetPaused(bool value)
        {
            Paused = value;
            Toggle(DaySource, value); Toggle(NightSource, value); Toggle(wind, value); Toggle(owl, value); Toggle(rain, value);
            foreach (var source in emitters) Toggle(source, value);
        }
        static void Toggle(AudioSource source, bool value)
        {
            if (!source || !source.gameObject.activeInHierarchy) return;
            if (value) source.Pause(); else source.UnPause();
        }

        void CacheAnchors()
        {
            // One scene scan, not a FindObjects call in the mixer update.
            foreach (var lamp in FindObjectsByType<WorldLamp2D>()) AddAnchor(lamp.transform, 1, Vector3.zero);
            foreach (var collider in FindObjectsByType<Collider2D>())
            {
                string n = collider.name.ToLowerInvariant();
                if (n.Contains("lago") || n.Contains("água") || n.Contains("fonte"))
                    AddAnchor(collider.transform, 0, collider.bounds.center - collider.transform.position);
                else if (n.Contains("fogueira")) AddAnchor(collider.transform, 2, Vector3.zero);
            }
        }
        void AddAnchor(Transform at, int kind, Vector3 offset)
        {
            if (!at || !anchored.Add(at)) return;
            anchors.Add(new Anchor { at = at, kind = kind, offset = offset });
        }
        void RefreshEmitters()
        {
            if (!view) view = Camera.main;
            if (!view || !waterClip) return;
            Vector3 listener = view.transform.position;
            // Eight pooled positional sources is the hard ceiling, regardless of lamp count.
            var chosen = new HashSet<int>();
            int slot = 0;
            for (int group = 0; group < 2; group++) for (int j = 0; j < 4; j++)
            {
                float closest = 30 * 30; int found = -1;
                for (int i = 0; i < anchors.Count; i++)
                {
                    var a = anchors[i];
                    if (!a.at || !a.at.gameObject.activeInHierarchy || chosen.Contains(i) || (a.kind == 1) != (group == 1)) continue;
                    Vector3 point = a.at.position + a.offset; point.z = listener.z;
                    float distance = (point - listener).sqrMagnitude;
                    if (distance < closest) { closest = distance; found = i; }
                }
                if (found < 0) break;
                chosen.Add(found); var anchor = anchors[found];
                if (emitters.Count <= slot)
                {
                    emitters.Add(Loop("Fonte local " + slot, waterClip, true));
                    emitterKinds.Add(0);
                }
                var source = emitters[slot];
                AudioClip target = anchor.kind == 1 ? humClip : anchor.kind == 2 ? fireClip : waterClip;
                source.gameObject.SetActive(true);
                if (source.clip != target) { source.clip = target; source.Play(); if (Paused) source.Pause(); }
                else if (!source.isPlaying && !Paused) source.Play();
                Vector3 position = anchor.at.position + anchor.offset; position.z = listener.z;
                source.transform.position = position;
                emitterKinds[slot] = anchor.kind; slot++;
            }
            for (int i = slot; i < emitters.Count; i++) emitters[i].gameObject.SetActive(false);
        }

        static AudioClip Synthesize(string clipName, int seconds, int kind)
        {
            const int rate = 22050;
            var data = new float[rate * seconds];
            var random = new System.Random(4109 + kind);
            float slow = 0, phase = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)rate;
                float noise = (float)random.NextDouble() * 2 - 1;
                slow = Mathf.Lerp(slow, noise, .035f);
                float signal = 0;
                if (kind == 0)
                {
                    float burst = Mathf.Repeat(t + .37f, 2.1f);
                    float phrase = burst < .44f ? Mathf.Pow(Mathf.Sin(burst / .44f * Mathf.PI), 2) : 0;
                    phase += (1900 + 650 * Mathf.Sin(t * 24)) * Mathf.PI * 2 / rate;
                    signal = Mathf.Sin(phase) * phrase * .36f;
                }
                else if (kind == 1)
                {
                    float trill = Mathf.Pow(Mathf.Max(0, Mathf.Sin(t * 37)), 6);
                    float phrase = .35f + .65f * Mathf.Pow(Mathf.Sin(t * 1.7f), 2);
                    signal = Mathf.Sin(t * 2 * Mathf.PI * 3700) * trill * phrase * .24f;
                }
                else if (kind == 2) signal = slow * (.27f + .07f * Mathf.Sin(t * Mathf.PI * .5f));
                else if (kind == 3)
                {
                    float call = Mathf.Repeat(t - 4 + seconds, seconds);
                    float envelope = call < 1.8f ? Mathf.Sin(call / 1.8f * Mathf.PI) : 0;
                    float pulse = .4f + .6f * Mathf.Pow(Mathf.Max(0, Mathf.Sin(call * 8)), 2);
                    signal = Mathf.Sin(t * 2 * Mathf.PI * 330) * envelope * pulse * .42f;
                }
                else if (kind == 4) signal = slow * .6f + noise * .035f + Mathf.Sin(t * 2 * Mathf.PI * 245) * .012f;
                else if (kind == 5) signal = Mathf.Sin(t * 2 * Mathf.PI * 120) * .13f + slow * .02f;
                else if (kind == 6) signal = slow * .45f + (noise > .995f ? noise * .45f : 0);
                else signal = slow * .34f + noise * .08f;
                // Short edge windows prevent clicks at loop seams.
                float edge = Mathf.Clamp01(Mathf.Min(i, data.Length - 1 - i) / (rate * .02f));
                data[i] = signal * edge;
            }
            var clip = AudioClip.Create(clipName, data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
        void OnDestroy()
        {
            foreach (var clip in new[] { birdsClip, cricketsClip, windClip, owlClip, waterClip, humClip, fireClip, rainClip })
                if (clip) Destroy(clip);
        }
    }
}


