using UnityEngine;
namespace PontoCego
{
    [RequireComponent(typeof(Health),typeof(PlayerInputReader))]
    public sealed class CombatPlayer:MonoBehaviour
    {
        public float meleeDamage=24,meleeRange=1.35f;
        public float AttackFlash{get;private set;}
        public float AttackStarted{get;private set;}=-100;public Vector2 AttackDirection{get;private set;}=Vector2.down;
        public int Medkits{get;private set;}=2;
        PlayerInputReader input;PlayerAim aim;Health health;float nextHit;
        int combo;float lastCombo,hitAt;bool pending,finisher;
        void Awake(){input=GetComponent<PlayerInputReader>();aim=GetComponent<PlayerAim>();health=GetComponent<Health>();health.Died+=OnDeath;}
        void Start()
        {
            if(GameSession.Exists){Medkits=GameSession.Current.medkits;health.IncreaseMaximum(GameSession.Current.armorLevel*20);}
        }
        void Update()
        {
            if(!health.Alive||input.Paused||input.GameplayLocked){if(!health.Alive||input.GameplayLocked)pending=false;return;}
            if(GetComponent<PlayerMotor>().Model.Dodging){pending=false;return;}
            if(pending&&Time.time>=hitAt){pending=false;ResolveHit();}
            if(input.ConsumeHeal())UseMedkit();
            if((input.ConsumeMelee()||input.MeleeHeld)&&Time.time>nextHit)
            {
                nextHit=Time.time+.42f;AttackFlash=Time.time+.15f;
                combo=Time.time-lastCombo>1.1f?1:combo%3+1;lastCombo=Time.time;finisher=combo==3;AttackStarted=Time.time;AttackDirection=aim.Direction;hitAt=Time.time+.12f;pending=true;
            }
        }
        void ResolveHit()
        {
                CombatArc.Show(transform.position,AttackDirection,finisher);
                foreach(var mateus in FindObjectsByType<Mateus>())
                {
                    Vector2 direction=mateus.transform.position-transform.position;
                    if(direction.magnitude<=meleeRange&&Vector2.Dot(AttackDirection,direction.normalized)>=.15f&&!Physics2D.Raycast(transform.position,direction.normalized,direction.magnitude,LayerMask.GetMask("World")))mateus.ReceiveAttack(CombatTeam.Player);
                }
                foreach(var enemy in FindObjectsByType<EnemyAgent>())
                {
                    Vector2 d=enemy.transform.position-transform.position;
                    if(d.magnitude>meleeRange||Vector2.Dot(AttackDirection,d.normalized)<.15f)continue;
                    if(Physics2D.Raycast(transform.position,d.normalized,d.magnitude,LayerMask.GetMask("World")))continue;
                    enemy.GetComponent<Health>().Damage(meleeDamage*(finisher?1.5f:1),CombatTeam.Player);enemy.Knockback(d.normalized*(finisher?6:4));enemy.Stun(finisher?.6f:.4f);
                }
        }
        public void UseMedkit()
        {
            if(Medkits<=0||health.Current>=health.maximum)return;Medkits--;health.Heal(55);
            if(GameSession.Exists)GameSession.Current.medkits=Medkits;
        }
        public void AddMedkit(){Medkits=Mathf.Min(Medkits+1,8);if(GameSession.Exists)GameSession.Current.medkits=Medkits;}
        void OnDeath(Health _){input.SetGameplayLocked(true);GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;}
    }
}
