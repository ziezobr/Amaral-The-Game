using UnityEngine;using UnityEngine.SceneManagement;
namespace PontoCego
{
    /// <summary>Boss attacks use fixed ground warnings, respect dialogue/pause, and leave recovery windows.</summary>
    public sealed class BossAbilities:MonoBehaviour
    {
        EnemyAgent enemy;Health health;PlayerMotor player;Rigidbody2D body;float cooldown=3,clock,recovery;int attack;Vector2 direction;bool casting,dashing;
        public bool Charging=>casting;public bool Recovering=>recovery>0;public bool Dashing=>dashing;
        public int Phase{get{var hp=health?health:GetComponent<Health>();return !hp?1:Mathf.Clamp(1+Mathf.FloorToInt((1-hp.Current/hp.maximum)*PhaseCount),1,PhaseCount);}}
        public int PhaseCount{get{var e=enemy?enemy:GetComponent<EnemyAgent>();return e&&e.role==EnemyRole.Paulo?5:e&&e.role==EnemyRole.Cenoura?3:2;}}
        int lastPhase;float baseSpeed,baseDamage,baseInterval;public GameObject higor;
        public bool ShieldCharge=>enemy&&enemy.role==EnemyRole.Cenoura&&attack%2==1;
        public Vector2 PowerDirection=>direction;
        public string AbilityName=>enemy&&enemy.role==EnemyRole.Pivo?(attack%2==1?"ARREMESSO TRIPLO":"ENTERRADA SÍSMICA"):enemy&&enemy.role==EnemyRole.Cenoura?(attack%2==1?"INVESTIDA DO ESCUDO":"IMPACTO NO CHÃO"):"HOLOFOTES / SOBRECARGA";
        public bool Enraged=>Phase>1;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Register(){SceneManager.sceneLoaded+=Setup;Setup(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
        static void Setup(Scene scene,LoadSceneMode mode)
        {
            foreach(var e in Resources.FindObjectsOfTypeAll<EnemyAgent>())
            {
                if(!e.gameObject.scene.IsValid()||e.GetComponent<BossAbilities>())continue;
                if(e.role==EnemyRole.Cenoura||e.role==EnemyRole.Paulo||e.role==EnemyRole.Pivo)e.gameObject.AddComponent<BossAbilities>();
            }
        }
        void Start(){enemy=GetComponent<EnemyAgent>();health=GetComponent<Health>();body=GetComponent<Rigidbody2D>();player=FindAnyObjectByType<PlayerMotor>();baseSpeed=enemy.speed;baseDamage=enemy.damage;baseInterval=enemy.attackInterval;}
        void Update()
        {
            if(!player||!enemy.Alive){body.linearVelocity=Vector2.zero;casting=dashing=false;recovery=0;enemy.enabled=true;return;}
            if(lastPhase!=Phase){lastPhase=Phase;enemy.speed=baseSpeed*(1+.13f*(Phase-1));enemy.damage=baseDamage*(1+.18f*(Phase-1));enemy.attackInterval=baseInterval/(1+.16f*(Phase-1));if(Phase==3&&enemy.role==EnemyRole.Cenoura&&higor){higor.SetActive(true);FindAnyObjectByType<Interaction>()?.Say("Higor: Recebi o aviso! Deixa os golpes comigo; protejam o Amaral!");}}
            var input=player.GetComponent<PlayerInputReader>();if(input.Paused||input.GameplayLocked||!player.GetComponent<Health>().Alive){if(casting||dashing)body.linearVelocity=Vector2.zero;return;}
            if(recovery>0){body.linearVelocity=Vector2.zero;recovery=Mathf.Max(0,recovery-Time.deltaTime);if(recovery<=0)enemy.enabled=true;return;}
            if(dashing)
            {
                clock-=Time.deltaTime;body.linearVelocity=direction*12;
                foreach(var h in FindObjectsByType<Health>())if(h.team!=CombatTeam.Enemy&&Vector2.Distance(h.transform.position,transform.position)<.95f)h.Damage(Enraged?30:23,CombatTeam.Enemy);
                if(clock<=0){dashing=false;body.linearVelocity=Vector2.zero;recovery=.75f;cooldown=Enraged?2.7f:4;}return;
            }
            if(casting)
            {
                body.linearVelocity=Vector2.zero;clock-=Time.deltaTime;
                if(enemy.Stunned){casting=false;enemy.enabled=true;cooldown=2;return;}
                if(clock>0)return;casting=false;enemy.enabled=true;
                if(enemy.role==EnemyRole.Cenoura&&attack%2==1){dashing=true;clock=.48f;enemy.enabled=false;}
                else if(enemy.role==EnemyRole.Pivo)
                {
                    if(attack%2==1)for(int i=-1;i<=1;i++){var d=(Vector2)(Quaternion.Euler(0,0,i*(Enraged?22:15))*direction);BasketballShot.Launch(body.position,d,enemy.projectileSprite,Enraged?22:16);}
                }
                else if(enemy.role==EnemyRole.Paulo)
                {
                    int n=6+Phase*2;for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;CombatProjectile.Fire(transform.position,new Vector2(Mathf.Cos(a),Mathf.Sin(a)),CombatTeam.Enemy,9,enemy.projectileSprite);}
                }
                if(!dashing){enemy.enabled=false;recovery=.65f;}
                cooldown=Mathf.Max(1.8f,5-.65f*(Phase-1));return;
            }
            if(enemy.Stunned||Vector2.Distance(player.transform.position,transform.position)>12)return;
            if((cooldown-=Time.deltaTime)>0)return;
            attack++;casting=true;clock=enemy.role==EnemyRole.Paulo?1.25f:.95f;enemy.SuspendNormalAttack();enemy.enabled=false;direction=((Vector2)player.transform.position-body.position).normalized;
            FindAnyObjectByType<OfficeAudio>()?.Warning();
            FindAnyObjectByType<Interaction>().Say(AbilityName+(enemy.FlashResistant?" — resistência ao flash: esquive da marca!":" — saia da marca! O flash pode interromper a preparação."));
            if(enemy.role==EnemyRole.Cenoura&&attack%2==1)
            {
                var wall=Physics2D.CircleCast(body.position,.28f,direction,5.5f,LayerMask.GetMask("World"));float length=wall.collider?wall.distance:5.5f;
                for(int i=1;i<=5;i++)if(i*1.1f<=length)GroundWarning.Spawn(body.position+direction*i*1.1f,.7f,clock,0,this);
            }
            else
            {
                Vector2 center=enemy.role==EnemyRole.Cenoura?body.position:(Vector2)player.transform.position;
                if(enemy.role!=EnemyRole.Pivo||attack%2==0)GroundWarning.Spawn(center,enemy.role==EnemyRole.Cenoura?2.8f:enemy.role==EnemyRole.Pivo?2.4f:1.55f,clock,Enraged?28:21,this);
                else for(int i=1;i<=5;i++)GroundWarning.Spawn(body.position+direction*i, .45f,clock,0,this);
                if(enemy.role==EnemyRole.Paulo&&Phase>=3){GroundWarning.Spawn(center+Vector2.left*3,1.45f,clock+.25f,21,this);GroundWarning.Spawn(center+Vector2.right*3,1.45f,clock+.25f,21,this);if(Phase>=4){GroundWarning.Spawn(center+Vector2.up*3,1.45f,clock+.4f,24,this);GroundWarning.Spawn(center+Vector2.down*3,1.45f,clock+.4f,24,this);}if(Phase==5)GroundWarning.Spawn(body.position,3,clock+.6f,32,this);}
            }
        }
        void OnDisable(){if(body)body.linearVelocity=Vector2.zero;}
    }
    public sealed class GroundWarning:MonoBehaviour
    {
        public float radius,delay,damage;float elapsed;LineRenderer line;BossAbilities owner;Material material;
        public static void Spawn(Vector2 at,float radius,float delay,float damage,BossAbilities owner=null)
        {
            var go=new GameObject("Área de ataque / aviso");go.transform.position=at;var warning=go.AddComponent<GroundWarning>();warning.radius=radius;warning.delay=delay;warning.damage=damage;warning.owner=owner;
        }
        void Start(){line=gameObject.AddComponent<LineRenderer>();material=new Material(Shader.Find("Sprites/Default"));line.sharedMaterial=material;line.loop=true;line.useWorldSpace=false;line.positionCount=48;line.startWidth=line.endWidth=.055f;line.sortingOrder=3500;for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;line.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a))*radius);}}
        void Update()
        {
            var input=FindAnyObjectByType<PlayerInputReader>();if(!input||input.Paused||input.GameplayLocked)return;
            if(owner&&(!owner.GetComponent<Health>().Alive||owner.GetComponent<EnemyAgent>().Stunned)){Destroy(gameObject);return;}
            elapsed+=Time.deltaTime;line.startColor=line.endColor=Color.Lerp(new Color(1,.76f,.16f,.6f),new Color(1,.16f,.1f,1),elapsed/delay);
            if(elapsed<delay)return;
            if(damage>0)foreach(var h in FindObjectsByType<Health>())
            {
                Vector2 d=h.transform.position-transform.position;if(h.team==CombatTeam.Enemy||d.magnitude>radius)continue;
                if(!Physics2D.Raycast(transform.position,d.normalized,d.magnitude,LayerMask.GetMask("World")))h.Damage(damage,CombatTeam.Enemy);
            }
            Destroy(gameObject);
        }
        void OnDestroy(){if(material)Destroy(material);}
    }
}
