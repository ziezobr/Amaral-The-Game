using UnityEngine;
namespace PontoCego
{
    /// <summary>Additive visual-only weight shift. Physics and authored attack poses remain untouched.</summary>
    [DefaultExecutionOrder(75)]
    public sealed class ActorSecondaryMotion2D:MonoBehaviour
    {
        public SpriteRenderer visual;Rigidbody2D body;Transform rig;Vector3 origin,authoredScale;float phase,weight;Vector2 lastVelocity;ReferenceActorAnimation animation;PlayerMotor motor;Health health;CombatPlayer combat;PlayerInputReader input;StreetWalker walker;EnemyAgent gaitEnemy;AmmoInventory gun;
        void Start()
        {
            body=GetComponent<Rigidbody2D>();if(!visual)return;
            // Keep the authored hierarchy: Animator bindings target this exact renderer path.
            rig=visual.transform;origin=rig.localPosition;authoredScale=rig.localScale;phase=Mathf.Abs(transform.position.x*.7f+transform.position.y)%6;
            gun=GetComponent<AmmoInventory>();walker=GetComponent<StreetWalker>();gaitEnemy=GetComponent<EnemyAgent>();animation=GetComponent<ReferenceActorAnimation>();motor=GetComponent<PlayerMotor>();health=GetComponent<Health>();combat=GetComponent<CombatPlayer>();input=GetComponent<PlayerInputReader>();
        }
        void LateUpdate()
        {
            if(!rig||health&&!health.Alive||input&&(input.Paused||input.GameplayLocked))return;
            float dt=Time.deltaTime;Vector2 velocity=body?body.linearVelocity:Vector2.zero;float speed=velocity.magnitude;
            weight=Mathf.Lerp(weight,Mathf.Clamp01(speed/2),1-Mathf.Exp(-15*dt));
            phase+=dt*(speed>.1f?Mathf.Clamp(speed*2.1f,3,12):1.6f);
            float gait=animation?animation.GaitCycle*Mathf.PI*2:walker?walker.VisualGaitCycle*Mathf.PI*2:gaitEnemy?gaitEnemy.VisualGaitCycle*Mathf.PI*2:phase;
            bool sprint=motor?motor.Model.Sprinting:speed>4.2f;
            float breath=Mathf.Sin(Time.time*1.8f+phase)*.008f*(1-weight);
            float lift=Mathf.Abs(Mathf.Sin(gait*2))*(sprint?.035f:.018f)*weight;
            bool reduced=GamePreferences.ReduceMotion;
            float acceleration=dt>.0001f?Mathf.Clamp((velocity.x-lastVelocity.x)/dt,-25,25):0;
            float recoil=health?Mathf.Clamp01(1-(Time.time-health.LastDamageTime)/.2f):0;
            Vector3 offset=new Vector3(Mathf.Sin(gait)*.012f*weight+Mathf.Sin(recoil*15)*recoil*.035f,breath+lift,0);
            float lean=Mathf.Clamp(-velocity.x*(sprint?.85f:.5f)-acceleration*.06f,-5,5);
            if(combat&&Time.time-combat.AttackStarted<.3f)lean+=Mathf.Sin((Time.time-combat.AttackStarted)/.3f*Mathf.PI)*combat.AttackDirection.x*-3;
            if(gun){float shot=Mathf.Clamp01(1-(Time.time-gun.LastShotAt)/.16f);offset-=(Vector3)gun.LastShotDirection*shot*.07f;lean+=gun.LastShotDirection.x*shot*4;}
            float stretch=1+Mathf.Sin(gait*2)*weight*.008f;
            if(motor&&motor.Model.Dodging){lean=-motor.Model.Facing.x*10;stretch=1-Mathf.Sin(motor.Model.DodgeProgress*Mathf.PI)*.07f;}
            if(reduced){offset=Vector3.zero;lean=0;stretch=1;}
            rig.localPosition=origin+offset;
            // Never accumulate scale on NPCs that have no directional animation controller.
            Vector3 baseline=animation?rig.localScale:authoredScale;
            rig.localScale=new Vector3(baseline.x/Mathf.Sqrt(stretch),baseline.y*stretch,baseline.z);
            rig.localRotation=Quaternion.Slerp(rig.localRotation,Quaternion.Euler(0,0,lean),1-Mathf.Exp(-18*dt));
            lastVelocity=velocity;
        }
    }
}
