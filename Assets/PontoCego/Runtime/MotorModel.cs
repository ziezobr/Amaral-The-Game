using UnityEngine;

namespace PontoCego
{
    /// <summary>Deterministic motor state: no input devices, scene lookups or wall-clock time.</summary>
    public sealed class MotorModel
    {
        public Vector2 Velocity { get; private set; }
        public Vector2 Facing { get; private set; } = Vector2.down;
        public float Stamina { get; private set; }
        public float Cooldown { get; private set; }
        public bool Dodging => dodgeLeft > 0f;
        public bool Sprinting { get; private set; }
        public float InfiniteStaminaRemaining{get;private set;}
        public void GrantInfiniteStamina(float seconds){InfiniteStaminaRemaining=Mathf.Max(InfiniteStaminaRemaining,seconds);Stamina=settings.sprintCapacity;exhaustedLeft=0;}
        float dodgeLeft, recoveryLeft, exhaustedLeft, dodgeBuffer;
        public float DodgeProgress => 1f-dodgeLeft/settings.dodgeDuration;
        public bool ShowStamina => Sprinting || Stamina < settings.sprintCapacity-.005f;
        Vector2 dodgeDirection;
        readonly MovementSettings settings;

        public MotorModel(MovementSettings settings)
        {
            this.settings = settings;
            Stamina = settings.sprintCapacity;
        }

        /// <summary>Digital diagonals are normalized; analog magnitudes below one are retained.</summary>
        public static Vector2 NormalizeInput(Vector2 value) => Vector2.ClampMagnitude(value, 1f);

        public void Stop() { Velocity = Vector2.zero; Sprinting = false; dodgeLeft = 0f; dodgeBuffer=0f; }

        public void Step(Vector2 move, bool sprint, bool dodge, float dt)
        {
            if (dt <= 0f) return;
            bool infinite=InfiniteStaminaRemaining>0;InfiniteStaminaRemaining=Mathf.Max(0,InfiniteStaminaRemaining-dt);if(infinite){Stamina=settings.sprintCapacity;exhaustedLeft=0;}
            move = NormalizeInput(move);
            dodgeBuffer=dodge?settings.dodgeInputBuffer:Mathf.Max(0,dodgeBuffer-dt);
            Cooldown = Mathf.Max(0f, Cooldown - dt);
            exhaustedLeft = Mathf.Max(0f, exhaustedLeft - dt);
            recoveryLeft = Mathf.Max(0f, recoveryLeft - dt);
            if (move.sqrMagnitude > .001f) Facing = move.normalized;
            if (dodgeBuffer>0f && !Dodging && Cooldown <= 0f)
            {
                dodgeDirection = move.sqrMagnitude > .001f ? move.normalized : Facing;
                dodgeLeft = settings.dodgeDuration;
                Cooldown = settings.dodgeDuration + settings.dodgeCooldown;
                dodgeBuffer=0;
            }
            Sprinting = sprint && !Dodging && move.sqrMagnitude > .001f && Stamina > 0f && exhaustedLeft <= 0f;
            if (Sprinting)
            {
                if(!infinite)Stamina = Mathf.Max(0f, Stamina - dt);
                recoveryLeft = settings.sprintRecoveryDelay;
                if (Stamina <= 0f) { exhaustedLeft = settings.exhaustedCooldown; Sprinting = false; }
            }
            else if (recoveryLeft <= 0f)
                Stamina = Mathf.Min(settings.sprintCapacity, Stamina + settings.sprintRecovery * dt);

            if (Dodging)
            {
                // Average the final partial tick so distance does not depend on fixedDeltaTime.
                float active = Mathf.Min(dt, dodgeLeft);
                float duration=settings.dodgeDuration;
                float t0=duration-dodgeLeft,t1=t0+active;
                float easedDistance=settings.dodgeSpeed*(active+settings.dodgeEase*duration/Mathf.PI*
                    (Mathf.Sin(Mathf.PI*t1/duration)-Mathf.Sin(Mathf.PI*t0/duration)));
                Velocity = dodgeDirection * (easedDistance / dt);
                dodgeLeft = Mathf.Max(0f, dodgeLeft - dt);
                return;
            }
            Vector2 target = move * settings.walkSpeed * (Sprinting ? settings.sprintMultiplier : 1f);
            bool turning=Velocity.sqrMagnitude>.1f&&move.sqrMagnitude>.001f&&Vector2.Dot(Velocity.normalized,move.normalized)<.5f;
            float response=move.sqrMagnitude>.001f?(turning?settings.turnAcceleration:settings.acceleration):settings.deceleration;
            Velocity=Vector2.MoveTowards(Velocity,target,response*dt);
        }
    }
}
