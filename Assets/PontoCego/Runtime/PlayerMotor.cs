using UnityEngine;

namespace PontoCego
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        public MovementSettings settings;
        public MotorModel Model { get; private set; }
        public Rigidbody2D Body { get; private set; }
        PlayerInputReader input;
        void Awake()
        {
            Body = GetComponent<Rigidbody2D>(); input = GetComponent<PlayerInputReader>();
            Model = new MotorModel(settings);
            Body.gravityScale = 0f; Body.freezeRotation = true;
            Body.interpolation = RigidbodyInterpolation2D.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        void FixedUpdate()
        {
            if (input.Paused || input.GameplayLocked) { Model.Stop(); Body.linearVelocity = Vector2.zero; return; }
            Model.Step(input.Move, input.Sprint, input.ConsumeDodge(), Time.fixedDeltaTime);
            // The physics solver resolves walls and furniture, including during dodge. No teleportation.
            Body.linearVelocity = Model.Velocity;
        }
        void OnDisable() { if (Body) Body.linearVelocity = Vector2.zero; Model?.Stop(); }
    }
}
