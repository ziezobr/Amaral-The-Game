using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    public enum EnemyRole{Baton,Gunner,Elite,Heavy,Cenoura,Paulo,Pivo}
    [RequireComponent(typeof(Rigidbody2D),typeof(Health))]
    public sealed class EnemyAgent:MonoBehaviour
    {
        public float VisualGaitCycle=>Mathf.Repeat(walkPhase/4f,1);

        public EnemyRole role;public float speed=2.3f,damage=12,attackInterval=1.2f,detectionRange=11;
        public SpriteRenderer visual;public Sprite projectileSprite;public Sprite[] poses;public string rewardId;
        public int cashReward=12;public bool startAlerted=true;
        public bool Stunned=>Time.time<stunUntil;
        public bool Alive=>health&&health.Alive;
        public string StateLabel=>!Alive?"CAÍDO":Stunned?"ATORDOADO":windingUp?"ATACANDO":GetComponent<EnemyVariety>() is EnemyVariety variety?variety.Label:"";
        public float WindupProgress=>windingUp?Mathf.Clamp01((Time.time-attackStarted)/.45f):0;
        public float LastAttackAt{get;private set;}=-100;
        public Vector2 AttackDirection=>attackDirection;
        Rigidbody2D body;Health health;PlayerMotor player;GridNavigator nav;Vector3 origin;
        float stunUntil,nextAttack,nextPath,attackStarted,deadAt=-1,walkPhase;bool windingUp,alerted;Vector2 attackDirection;List<Vector2> path;int waypoint;
        LineRenderer telegraph;static Material lineMaterial;Vector2 knockback;float knockbackUntil;
        float nextShortStun,nextBossFlash;
        public float FlashResistanceRemaining=>Mathf.Max(0,nextBossFlash-Time.time);
        public bool FlashResistant=>health&&health.boss&&FlashResistanceRemaining>0;
        public void Knockback(Vector2 force,float seconds=.13f){knockback=force;knockbackUntil=Time.time+seconds;}
        public void SuspendNormalAttack(){windingUp=false;nextAttack=Mathf.Max(nextAttack,Time.time+.6f);if(telegraph)telegraph.enabled=false;}
        void OnDisable(){if(telegraph)telegraph.enabled=false;}
        void Awake()
        {
            body=GetComponent<Rigidbody2D>();health=GetComponent<Health>();health.IncreaseMaximum(health.maximum*((health.boss?1.2f:1.35f)*DifficultyRules.EnemyLife(DifficultyRules.Current)-1));speed*=DifficultyRules.EnemyPace(DifficultyRules.Current);attackInterval/=DifficultyRules.EnemyPace(DifficultyRules.Current);body.gravityScale=0;body.freezeRotation=true;
            body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            origin=visual.transform.localPosition;health.Died+=OnDeath;
            var line=new GameObject("Sinal de ataque");line.transform.SetParent(transform,false);telegraph=line.AddComponent<LineRenderer>();
            if(!lineMaterial)lineMaterial=new Material(Shader.Find("Sprites/Default"));telegraph.sharedMaterial=lineMaterial;telegraph.positionCount=2;telegraph.useWorldSpace=true;
            telegraph.startWidth=.035f;telegraph.endWidth=.035f;telegraph.sortingOrder=3400;telegraph.enabled=false;
        }
        void Start(){player=FindAnyObjectByType<PlayerMotor>();nav=FindAnyObjectByType<GridNavigator>();alerted=startAlerted;}
        public void Stun(float seconds)
        {
            if(health.boss)
            {
                if(seconds>=.8f){if(Time.time<nextBossFlash)return;nextBossFlash=Time.time+2.4f;}
                else {var special=GetComponent<BossAbilities>();if(special&&special.Charging)return;}
            }
            if(seconds<.8f&&(health.boss||role==EnemyRole.Heavy||role==EnemyRole.Elite))
            {
                if(Time.time<nextShortStun)return;nextShortStun=Time.time+(health.boss?1.15f:.9f);
                seconds*=role==EnemyRole.Heavy?.45f:role==EnemyRole.Elite?.65f:1;
            }
            if(health.boss)seconds=Mathf.Min(.6f,seconds*.32f);stunUntil=Mathf.Max(stunUntil,Time.time+seconds);windingUp=false;nextAttack=Mathf.Max(nextAttack,stunUntil+.2f);
        }
        void OnDeath(Health _)
        {
            deadAt=Time.time;body.linearVelocity=Vector2.zero;GetComponent<Collider2D>().enabled=false;
            GameSession.RewardOnce(rewardId,cashReward);
            var mission=FindAnyObjectByType<CombatMission>();if(mission)mission.NotifyKill(this);
        }
        void FixedUpdate()
        {
            if(!player)return;
            if(WorldHub.Instance&&Vector2.Distance(player.transform.position,transform.position)>18){body.linearVelocity=Vector2.zero;return;}
            var input=player.GetComponent<PlayerInputReader>();if(input.Paused||input.GameplayLocked||!player.GetComponent<Health>().Alive){body.linearVelocity=Vector2.zero;attackStarted+=Time.fixedDeltaTime;nextAttack+=Time.fixedDeltaTime;stunUntil+=Time.fixedDeltaTime;return;}
            if(!Alive){body.linearVelocity=Vector2.zero;return;}
            if(Time.time<knockbackUntil){body.linearVelocity=knockback;return;}
            if(Stunned){body.linearVelocity=Vector2.zero;return;}
            Health target=player.GetComponent<Health>();float distance=Vector2.Distance(transform.position,target.transform.position);
            foreach(var ally in FindObjectsByType<AllyAgent>())
            {var hp=ally.GetComponent<Health>();float d=Vector2.Distance(transform.position,ally.transform.position);if(hp.Alive&&d<distance*.8f){target=hp;distance=d;}}
            Vector2 delta=target.transform.position-transform.position;
            bool clear=!Physics2D.Raycast(transform.position,delta.normalized,distance,LayerMask.GetMask("World"));
            if(!alerted){if(distance<detectionRange&&clear)alerted=true;else{body.linearVelocity=Vector2.zero;return;}}
            bool ranged=role==EnemyRole.Gunner||role==EnemyRole.Elite||role==EnemyRole.Paulo;
            float range=ranged?7.5f:1.15f;
            if(windingUp)
            {
                body.linearVelocity=Vector2.zero;
                if(Time.time-attackStarted>=.45f)
                {
                    windingUp=false;nextAttack=Time.time+attackInterval;LastAttackAt=Time.time;
                    if(ranged)
                    {
                        CombatProjectile.Fire(transform.position,attackDirection,CombatTeam.Enemy,damage,projectileSprite);
                        if(role==EnemyRole.Elite)for(int side=-1;side<=1;side+=2)CombatProjectile.Fire(transform.position,Quaternion.Euler(0,0,side*12)*attackDirection,CombatTeam.Enemy,damage*.65f,projectileSprite);
                    }
                    else if(distance<1.6f&&clear&&Vector2.Dot(delta.normalized,attackDirection)>.1f)target.Damage(damage,CombatTeam.Enemy);
                }
                return;
            }
            if(distance<range&&clear&&Time.time>=nextAttack)
            {windingUp=true;attackStarted=Time.time;attackDirection=delta.normalized;body.linearVelocity=Vector2.zero;return;}
            if(ranged&&distance<5.7f&&clear)
            {
                Vector2 retreat=distance<2.5f?-delta.normalized:Vector2.Perpendicular(delta.normalized)*Mathf.Sign(Mathf.Sin(Time.time*.8f+transform.position.x));
                body.linearVelocity=nav&&nav.Clear(transform.position,(Vector2)transform.position+retreat)?retreat*speed*.65f:Vector2.zero;return;
            }
            Vector2 direction=delta.normalized;
            if(nav&&!nav.Clear(transform.position,target.transform.position))
            {
                if(Time.time>nextPath){path=nav.Path(transform.position,target.transform.position);waypoint=0;nextPath=Time.time+.7f+Random.value*.2f;}
                if(path!=null&&waypoint<path.Count)
                {if(Vector2.Distance(transform.position,path[waypoint])<.25f)waypoint++;if(waypoint<path.Count)direction=(path[waypoint]-(Vector2)transform.position).normalized;else direction=Vector2.zero;}
            }
            body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,direction*speed,30*Time.fixedDeltaTime);
        }
        void LateUpdate()
        {
            if(!visual)return;visual.sortingOrder=WorldSort.At(transform.position.y);
            if(telegraph)
            {
                telegraph.enabled=windingUp&&Alive&&!Stunned;
                if(telegraph.enabled){bool ranged=role==EnemyRole.Gunner||role==EnemyRole.Elite||role==EnemyRole.Paulo;telegraph.SetPosition(0,transform.position);telegraph.SetPosition(1,transform.position+(Vector3)attackDirection*(ranged?8:1.7f));telegraph.startColor=telegraph.endColor=new Color(1,.35f,.13f,.2f+WindupProgress*.55f);}
            }
            if(deadAt>=0)
            {
                visual.transform.localRotation=Quaternion.Euler(0,0,80);visual.color=new Color(.6f,.6f,.6f,Mathf.Clamp01(1-(Time.time-deadAt)/2.8f));
                if(Time.time-deadAt>3)Destroy(gameObject);return;
            }
            if(body.linearVelocity.sqrMagnitude>.1f)visual.flipX=body.linearVelocity.x<-.15f;
            if(windingUp)visual.flipX=attackDirection.x<0;
            walkPhase+=Time.deltaTime*ReferenceActorAnimation.FramesPerSecond(body.linearVelocity.magnitude,body.linearVelocity.magnitude>4.2f,3);
            int stride=Mathf.FloorToInt(walkPhase)%4;
            if(poses!=null&&poses.Length==4)visual.sprite=poses[windingUp||Time.time-LastAttackAt<.14f?3:body.linearVelocity.magnitude>.2f?(stride==0?1:stride==2?2:0):0];
            visual.transform.localPosition=origin+Vector3.up*(body.linearVelocity.magnitude>.2f?Mathf.Abs(Mathf.Sin(Time.time*11+transform.position.x))*.045f:0);
        }
    }
}
