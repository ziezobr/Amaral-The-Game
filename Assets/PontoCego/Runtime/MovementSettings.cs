using UnityEngine;

namespace PontoCego
{
    /// <summary>Shared tuning asset. Distances in Unity units and times in seconds.</summary>
    [CreateAssetMenu(menuName = "Ponto Cego/Movement Settings")]
    public sealed class MovementSettings : ScriptableObject
    {
        [Min(0.1f)] public float walkSpeed = 3.6f;
        [Min(1f)] public float sprintMultiplier = 1.65f;
        [Min(1f)] public float acceleration = 38f;
        [Min(1f)] public float deceleration = 48f;
        [Min(0.01f)] public float dodgeDuration = 0.19f;
        [Min(0.1f)] public float dodgeSpeed = 11f;
        [Min(0f)] public float dodgeCooldown = 0.8f;
        [Min(0.1f)] public float sprintCapacity = 3f;
        [Min(0f)] public float sprintRecoveryDelay = 0.65f;
        [Min(0.1f)] public float sprintRecovery = 1.1f;
        [Min(0f)] public float exhaustedCooldown = 1.4f;
        [Range(.02f,.25f)] public float dodgeInputBuffer = .12f;
        [Range(0f,.4f)] public float dodgeEase = .22f;
        [Min(1f)] public float turnAcceleration = 64f;
    }
}
