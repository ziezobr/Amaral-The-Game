using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PontoCego
{
    /// <summary>Optional marker for static scenery added by future builders. Never attach to quest actors.</summary>
    public sealed class WorldStaticVisual2D : MonoBehaviour
    {
        public bool detail;
        public bool alwaysVisible;
    }

    /// <summary>
    /// Cached 2D scenery visibility and detail-distance filtering. Only rendering is suppressed;
    /// physics, lights, triggers, navigation and gameplay scripts keep their original state.
    /// Unity's normal renderer/frustum culling still operates inside this conservative guard band.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldVisibility2D : MonoBehaviour
    {
        [Min(.05f)] public float updateInterval = .2f;
        [Min(2)] public float sceneryMargin = 10;
        [Min(0)] public float detailMargin = 2;
        public int CachedCount => entries.Count;
        public int CulledCount { get; private set; }
        public double LastEvaluationMilliseconds { get; private set; }
        public Camera view;
        float nextEvaluation;
        readonly List<Entry> entries = new List<Entry>();
        readonly HashSet<SpriteRenderer> registered = new HashSet<SpriteRenderer>();
        sealed class Entry
        {
            public SpriteRenderer renderer;
            public bool originalForceOff, detail;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
            Install();
        }
        static void SceneLoaded(Scene scene, LoadSceneMode mode) { Install(); }
        static void Install()
        {
            if (!WorldHub.Instance || FindAnyObjectByType<WorldVisibility2D>()) return;
            WorldHub.Instance.gameObject.AddComponent<WorldVisibility2D>();
        }
        IEnumerator Start()
        {
            yield return null;
            if (!view) view = Camera.main;
            RebuildCache();
            EvaluateNow();
        }
        void LateUpdate()
        {
            if (Time.unscaledTime < nextEvaluation) return;
            nextEvaluation = Time.unscaledTime + updateInterval;
            EvaluateNow();
        }

        /// <summary>One scene scan at activation; builders may call again after creating new static art.</summary>
        public void RebuildCache()
        {
            RestoreAll(); entries.Clear(); registered.Clear();
            foreach (var renderer in FindObjectsByType<SpriteRenderer>()) Register(renderer);
        }
        public bool Register(SpriteRenderer renderer)
        {
            if (!renderer || registered.Contains(renderer) || !Eligible(renderer)) return false;
            var marker = renderer.GetComponentInParent<WorldStaticVisual2D>();
            if (marker && marker.alwaysVisible) return false;
            entries.Add(new Entry { renderer = renderer, originalForceOff = renderer.forceRenderingOff,
                detail = marker && marker.detail || renderer.name.Contains("raízes") || renderer.transform.root.name.StartsWith("Vegetação /") });
            registered.Add(renderer);
            return true;
        }
        public static bool Eligible(SpriteRenderer renderer)
        {
            if (!renderer || !renderer.gameObject.activeInHierarchy) return false;
            // A parent interaction, health or physics body makes a sprite a gameplay actor.
            for (Transform at = renderer.transform; at; at = at.parent)
            {
                if (at.GetComponent<Rigidbody2D>() || at.GetComponent<Health>() || at.GetComponent<PlayerMotor>()) return false;
                foreach (var behaviour in at.GetComponents<MonoBehaviour>()) if (behaviour is IInteractable) return false;
            }
            var marker = renderer.GetComponentInParent<WorldStaticVisual2D>();
            if (marker) return !marker.alwaysVisible;
            string root = renderer.transform.root.name;
            return root.StartsWith("Cidade /") || root.StartsWith("Distrito reconstruído /") || root.StartsWith("Vegetação /") || root.StartsWith("Mobiliário da rua /") ||
                root.StartsWith("Cenário") || root.StartsWith("Oclusão /") || root.StartsWith("Placa /") ||
                root.StartsWith("Calçada") || root.StartsWith("Rua de ligação") || root.StartsWith("Faixa da rua") ||
                root.StartsWith("Travessia /") || root.StartsWith("Terreno contínuo /") ||
                root.StartsWith("Paisagem /") || root.StartsWith("Fronteira /");
        }
        public void EvaluateNow()
        {
            if (!view) view = Camera.main;
            if (!view || !view.orthographic) return;
            long start = System.Diagnostics.Stopwatch.GetTimestamp();
            Vector3 center = view.transform.position;
            float halfHeight = view.orthographicSize, halfWidth = halfHeight * view.aspect;
            CulledCount = 0;
            foreach (var entry in entries)
            {
                if (!entry.renderer) continue;
                float margin = entry.detail ? detailMargin : sceneryMargin;
                var visible = new Bounds(new Vector3(center.x, center.y, 0),
                    new Vector3((halfWidth + margin) * 2, (halfHeight + margin) * 2, 10000));
                bool hide = !visible.Intersects(entry.renderer.bounds);
                entry.renderer.forceRenderingOff = entry.originalForceOff || hide;
                if (hide) CulledCount++;
            }
            LastEvaluationMilliseconds = (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 /
                System.Diagnostics.Stopwatch.Frequency;
        }
        void RestoreAll()
        {
            foreach (var entry in entries) if (entry.renderer) entry.renderer.forceRenderingOff = entry.originalForceOff;
            CulledCount = 0;
        }
        void OnDisable() { RestoreAll(); }
        void OnDestroy() { RestoreAll(); }
    }
}
