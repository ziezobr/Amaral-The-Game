using UnityEngine;

namespace PontoCego
{
    /// <summary>Body follows locomotion; the separate camera prop/reticle follows aim.</summary>
    public sealed class DirectionalAnimation : MonoBehaviour
    {
        public Animator animator;
        public SpriteRenderer bodySprite;
        PlayerMotor motor;
        static readonly int X = Animator.StringToHash("FaceX"), Y = Animator.StringToHash("FaceY"),
            Speed = Animator.StringToHash("Speed"), Sprint = Animator.StringToHash("Sprint"), Dodge = Animator.StringToHash("Dodge");
        void Awake() { motor = GetComponent<PlayerMotor>(); }
        void Update()
        {
            var model = motor.Model;
            animator.SetFloat(X, model.Facing.x); animator.SetFloat(Y, model.Facing.y);
            animator.SetFloat(Speed, motor.Body.linearVelocity.magnitude);
            animator.SetBool(Sprint, model.Sprinting); animator.SetBool(Dodge, model.Dodging);
            float rate=motor.Body.linearVelocity.magnitude>.1f?Mathf.Clamp(motor.Body.linearVelocity.magnitude/motor.settings.walkSpeed,.8f,1.25f):1;
            animator.speed=Mathf.Lerp(animator.speed,rate,1-Mathf.Exp(-14*Time.deltaTime));
            float lean=model.Dodging?-model.Facing.x*11:0;
            bodySprite.transform.localRotation=Quaternion.Lerp(bodySprite.transform.localRotation,Quaternion.Euler(0,0,lean),1-Mathf.Exp(-22*Time.deltaTime));
            bodySprite.sortingOrder = WorldSort.At(transform.position.y);
        }
    }
}
