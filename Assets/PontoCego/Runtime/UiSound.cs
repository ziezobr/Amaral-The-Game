using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PontoCego
{
    /// <summary>Short, quiet interface cues. This source remains usable while the simulation is paused.</summary>
    [DisallowMultipleComponent]
    public sealed class UiSound : MonoBehaviour
    {
        static UiSound instance;
        AudioSource source;
        AudioClip click, open, notify;
        float lastClick = -10, lastOpen = -10, lastNotify = -10;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { instance = null; }

        static UiSound Ensure()
        {
            if (!instance) new GameObject("Áudio / interface").AddComponent<UiSound>();
            return instance;
        }

        void Awake()
        {
            if (instance && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.ignoreListenerPause = true;
            source.priority = 80;
            click = CreateCue("UI / clique suave", .085f, 0);
            open = CreateCue("UI / abrir", .18f, 1);
            notify = CreateCue("UI / notificação", .3f, 2);
        }

        public static void PlayClick()
        {
            var sound = Ensure();
            if (Time.unscaledTime - sound.lastClick < .045f) return;
            sound.lastClick = Time.unscaledTime;
            sound.Play(sound.click, .17f);
        }
        public static void PlayOpen()
        {
            var sound = Ensure();
            if (Time.unscaledTime - sound.lastOpen < .09f) return;
            sound.lastOpen = Time.unscaledTime;
            sound.Play(sound.open, .16f);
        }
        public static void PlayNotify()
        {
            var sound = Ensure();
            if (Time.unscaledTime - sound.lastNotify < .16f) return;
            sound.lastNotify = Time.unscaledTime;
            sound.Play(sound.notify, .19f);
        }
        void Play(AudioClip clip, float gain)
        {
            if (source && clip) source.PlayOneShot(clip, gain * Mathf.Clamp01(GamePreferences.Effects));
        }

        static AudioClip CreateCue(string clipName, float seconds, int style)
        {
            const int rate = 22050;
            var samples = new float[Mathf.CeilToInt(seconds * rate)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate, progress = t / seconds;
                float envelope = Mathf.Sin(progress * Mathf.PI) * Mathf.Exp(-progress * 4);
                float hz = style == 0 ? 660 : style == 1 ? 420 + 360 * progress : progress < .5f ? 660 : 880;
                float tone = Mathf.Sin(t * Mathf.PI * 2 * hz);
                samples[i] = envelope * (tone * .6f + Mathf.Sin(t * Mathf.PI * 4 * hz) * .1f);
            }
            var clip = AudioClip.Create(clipName, samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        void OnDestroy()
        {
            if (instance != this) return;
            instance = null;
            if (click) Destroy(click);
            if (open) Destroy(open);
            if (notify) Destroy(notify);
        }
    }
}
