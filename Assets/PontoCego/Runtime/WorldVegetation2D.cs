using UnityEngine;
namespace PontoCego
{
    /// <summary>Visual bending only: never moves a bush collider or a collectible's interaction position.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WorldVegetation2D : MonoBehaviour
    {
        [Min(.1f)] public float interactionRadius = 1.5f;
        [Range(0,6)] public float maxSwayDegrees = 4;
        Vector3 originalScale; Quaternion originalRotation; float phase;
        void Awake()
        {
            originalScale = transform.localScale; originalRotation = transform.localRotation;
            phase = transform.position.x * .73f + transform.position.y * .41f;
            if (GetComponent<Collider2D>()) enabled=false; // Attach to a visual child, never the solid footprint.
        }
        void LateUpdate()
        {
            var weather = WorldWeather2D.Instance;
            var cameraView = weather ? weather.ViewCamera : Camera.main;
            var player = weather ? weather.PlayerTransform : null;
            if (!cameraView) return;
            var screen = cameraView.WorldToViewportPoint(transform.position);
            if (screen.x < -.1f || screen.x > 1.1f || screen.y < -.1f || screen.y > 1.1f) return;
            float wind = weather ? weather.Wind : .18f;
            float distance = player ? Vector2.Distance(player.transform.position, transform.position) : 100;
            float contact = Mathf.Clamp01(1 - distance / Mathf.Max(.1f,interactionRadius));
            float swing = GamePreferences.ReduceMotion ? 0 : Mathf.Sin(Time.time * (1.5f + wind) + phase) * Mathf.Min(maxSwayDegrees,1 + wind * 3);
            float side = player && player.transform.position.x < transform.position.x ? 1 : -1;
            var desired = originalScale; desired.y *= 1 - contact * .14f; desired.x *= 1 + contact * .07f;
            float blend = 1 - Mathf.Exp(-10 * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, desired, blend);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation * Quaternion.Euler(0, 0, Mathf.Clamp(swing + side * contact * (GamePreferences.ReduceMotion ? 0 : 5),-6,6)), blend);
        }
        void OnDisable() { transform.localScale = originalScale; transform.localRotation = originalRotation; }
    }
}
