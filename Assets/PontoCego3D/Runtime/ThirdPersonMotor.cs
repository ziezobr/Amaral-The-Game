using UnityEngine;
namespace PontoCego3D
{
    [RequireComponent(typeof(CharacterController),typeof(ThirdPersonInput))]
    public sealed class ThirdPersonMotor:MonoBehaviour
    {
        public Transform visual;public Animator animator;public OrbitCamera orbit;
        public float walkSpeed=3.6f,runSpeed=6.2f,acceleration=20,braking=28,staminaCapacity=4.5f;
        public float Stamina{get;private set;}public bool Running{get;private set;}public bool Dodging=>dodgeLeft>0;
        public float DodgeCooldown=>cooldown;public bool Locked;public Vector3 Velocity=>velocity;
        ThirdPersonInput input;CharacterController controller;CombatActor actor;
        Vector3 velocity,dodgeDirection;float vertical,turnVelocity,dodgeLeft,cooldown,recovery;
        void Awake(){controller=GetComponent<CharacterController>();input=GetComponent<ThirdPersonInput>();actor=GetComponent<CombatActor>();Stamina=staminaCapacity;}
        void Update()
        {
            if(input.Paused)return;float dt=Time.deltaTime;cooldown=Mathf.Max(0,cooldown-dt);recovery=Mathf.Max(0,recovery-dt);
            bool blocked=Locked||actor&&(!actor.Alive||actor.Stunned);Vector2 axis=blocked?Vector2.zero:input.Move;
            Vector3 forward=Quaternion.Euler(0,orbit.yaw,0)*Vector3.forward,right=Quaternion.Euler(0,orbit.yaw,0)*Vector3.right;
            Vector3 desired=forward*axis.y+right*axis.x;
            if(!blocked&&input.Down("Dodge")&&cooldown<=0&&Stamina>.8f)
            {
                dodgeDirection=desired.sqrMagnitude>.01f?desired.normalized:visual.forward;dodgeLeft=.46f;cooldown=1.1f;Stamina-=.8f;recovery=1;
                visual.rotation=Quaternion.LookRotation(dodgeDirection);animator.CrossFadeInFixedTime("Roll",.06f);actor?.Protect(.32f);
            }
            Running=!blocked&&input.Sprint&&!input.Aim&&axis.sqrMagnitude>.1f&&Stamina>0&&!Dodging;
            if(Running){Stamina=Mathf.Max(0,Stamina-dt);recovery=1;}else if(recovery<=0)Stamina=Mathf.Min(staminaCapacity,Stamina+dt*1.5f);
            float speed=Running?runSpeed:input.Aim?2:walkSpeed;if(actor&&actor.Attacking)speed*=.25f;
            velocity=Vector3.MoveTowards(velocity,desired*speed,(axis.sqrMagnitude>.01f?acceleration:braking)*dt);
            if(Dodging){dodgeLeft=Mathf.Max(0,dodgeLeft-dt);velocity=dodgeDirection*(7.5f*(.75f+.25f*Mathf.Sin(dodgeLeft/.46f*Mathf.PI)));}
            if(controller.isGrounded&&vertical<0)vertical=-2;
            if(!blocked&&!Dodging&&input.Down("Jump")&&controller.isGrounded){vertical=6;animator.CrossFadeInFixedTime("Jump",.08f);}
            vertical-=20*dt;controller.Move((velocity+Vector3.up*vertical)*dt);
            if(!Dodging&&!blocked)
            {
                Vector3 facing=input.Aim?forward:desired;
                if(orbit.isometric&&UnityEngine.InputSystem.Mouse.current!=null){var ray=Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());var plane=new Plane(Vector3.up,transform.position);if(plane.Raycast(ray,out var enter)&&input.Aim)facing=(ray.GetPoint(enter)-transform.position).WithoutY();}
                if(facing.sqrMagnitude>.01f){float yaw=Mathf.Atan2(facing.x,facing.z)*Mathf.Rad2Deg;visual.rotation=Quaternion.Euler(0,Mathf.SmoothDampAngle(visual.eulerAngles.y,yaw,ref turnVelocity,.075f),0);}
            }
            animator.SetFloat("Speed",new Vector3(controller.velocity.x,0,controller.velocity.z).magnitude,.13f,dt);
            if(transform.position.y< -8)FindAnyObjectByType<OpenDistrict>().Respawn();
        }
        public void Warp(Vector3 point){controller.enabled=false;transform.position=point;controller.enabled=true;velocity=Vector3.zero;vertical=0;}
    }
}
