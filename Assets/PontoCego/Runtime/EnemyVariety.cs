using UnityEngine;
namespace PontoCego
{
    public enum EnemyArchetype { StreetFighter,Sprinter,Flanker,Armored,Marksman,Enforcer }
    /// <summary>Common-enemy roles complement the existing physics, telegraphs and boss systems.</summary>
    [DefaultExecutionOrder(20)]
    public sealed class EnemyVariety:MonoBehaviour
    {
        public EnemyArchetype archetype;EnemyAgent enemy;Rigidbody2D body;Health health;PlayerMotor player;PlayerInputReader input;
        float nextCharge,prepareUntil,dashUntil;Vector2 chargeDirection;bool preparing;
        static readonly string[] Names={"LUTADOR","VELOCISTA","FLANQUEADOR","BLINDADO","ATIRADOR","BRUTAMONTES"};
        public string Label=>preparing?"PREPARANDO INVESTIDA":dashUntil>Time.time?"INVESTIDA!":Names[(int)archetype];
        public static float ShieldMultiplier(EnemyArchetype kind,bool stunned)=>kind==EnemyArchetype.Armored&&!stunned?.4f:1;
        public float AdjustIncoming(float amount)=>amount*ShieldMultiplier(archetype,enemy&&enemy.Stunned);
        void Start()
        {
            enemy=GetComponent<EnemyAgent>();body=GetComponent<Rigidbody2D>();health=GetComponent<Health>();player=FindAnyObjectByType<PlayerMotor>();input=player?player.GetComponent<PlayerInputReader>():null;
            if(!enemy||health.boss){enabled=false;return;}
            nextCharge=Time.time+3;
            switch(archetype)
            {
                case EnemyArchetype.StreetFighter:enemy.speed=2.5f;enemy.attackInterval=.95f;break;
                case EnemyArchetype.Sprinter:enemy.speed=3.5f;enemy.damage=11;enemy.attackInterval=.85f;break;
                case EnemyArchetype.Flanker:enemy.speed=3;enemy.damage=15;enemy.attackInterval=1.15f;break;
                case EnemyArchetype.Armored:enemy.speed=1.7f;enemy.damage=20;enemy.attackInterval=1.6f;health.IncreaseMaximum(65);break;
                case EnemyArchetype.Marksman:enemy.speed=1.65f;enemy.damage=18;enemy.attackInterval=2;enemy.detectionRange=17;break;
                case EnemyArchetype.Enforcer:enemy.speed=1.9f;enemy.damage=26;enemy.attackInterval=1.8f;health.IncreaseMaximum(85);break;
            }
        }
        void FixedUpdate()
        {
            if(!enemy||!enemy.Alive||!player||input&&(input.Paused||input.GameplayLocked))return;
            if(enemy.Stunned){preparing=false;dashUntil=0;nextCharge=Mathf.Max(nextCharge,Time.time+2);return;}
            Vector2 delta=player.transform.position-transform.position;float distance=delta.magnitude;
            if(archetype==EnemyArchetype.Sprinter)
            {
                if(preparing){body.linearVelocity=Vector2.zero;enemy.SuspendNormalAttack();if(Time.time>=prepareUntil){preparing=false;dashUntil=Time.time+.4f;}return;}
                if(Time.time<dashUntil){body.linearVelocity=chargeDirection*7;return;}
                if(dashUntil>0){dashUntil=0;enemy.Stun(.65f);nextCharge=Time.time+5;return;}
                if(Time.time>nextCharge&&distance>2.5f&&distance<8&&!Physics2D.Raycast(transform.position,delta.normalized,distance,LayerMask.GetMask("World"))){chargeDirection=delta.normalized;preparing=true;prepareUntil=Time.time+.8f;body.linearVelocity=Vector2.zero;enemy.SuspendNormalAttack();}
            }
            if(enemy.WindupProgress>0)return;
            if(archetype==EnemyArchetype.Flanker&&distance>2.2f&&distance<9)
            {
                Vector2 tangent=new Vector2(-delta.y,delta.x).normalized*(transform.position.x<player.transform.position.x?1:-1);Vector2 dir=(delta.normalized*.35f+tangent*.8f).normalized;
                if(!Physics2D.CircleCast(body.position,.3f,dir,.9f,LayerMask.GetMask("World")))body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,dir*enemy.speed,20*Time.fixedDeltaTime);
            }
            if(archetype==EnemyArchetype.Marksman&&distance<5&&distance>1&&!Physics2D.CircleCast(body.position,.3f,-delta.normalized,1,LayerMask.GetMask("World")))body.linearVelocity=-delta.normalized*enemy.speed;
        }
    }
}
