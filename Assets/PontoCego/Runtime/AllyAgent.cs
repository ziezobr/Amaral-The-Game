using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    [RequireComponent(typeof(Rigidbody2D),typeof(Health))]
    public sealed class AllyAgent:MonoBehaviour
    {
        public string characterName="Ribeiro";public bool ranged=true;public Vector2 formationOffset;
        public SpriteRenderer visual;public Sprite projectileSprite;public Sprite[] poses;
        Rigidbody2D body;Health health;PlayerMotor player;GridNavigator nav;Vector3 visualOrigin;List<Vector2> path;int waypoint;
        float nextAttack,nextPath,downUntil,attackUntil;int rounds=6;
        void Awake()
        {
            body=GetComponent<Rigidbody2D>();health=GetComponent<Health>();body.gravityScale=0;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            visualOrigin=visual.transform.localPosition;health.Died+=_=>{downUntil=Time.time+9;body.linearVelocity=Vector2.zero;};
        }
        void Start(){player=FindAnyObjectByType<PlayerMotor>();nav=FindAnyObjectByType<GridNavigator>();}
        void FixedUpdate()
        {
            if(!player)return;var input=player.GetComponent<PlayerInputReader>();
            if(!health.Alive){body.linearVelocity=Vector2.zero;if(Time.time>downUntil)health.Restore(70);return;}
            if(input.Paused||input.GameplayLocked){body.linearVelocity=Vector2.zero;return;}
            EnemyAgent enemy=null;float best=ranged?9:6;
            foreach(var e in FindObjectsByType<EnemyAgent>())
            {
                if(!e.Alive)continue;Vector2 delta=e.transform.position-transform.position;float distance=delta.magnitude;
                if(distance<best&&!Physics2D.Raycast(transform.position,delta.normalized,distance,LayerMask.GetMask("World"))){enemy=e;best=distance;}
            }
            Vector2 destination=(Vector2)player.transform.position+formationOffset;
            if(enemy)
            {
                if(!ranged)destination=enemy.transform.position;
                if(best<(ranged?8:1.35f)&&Time.time>nextAttack)
                {
                    float bonus=GameSession.Current.allyLevel*5;
                    attackUntil=Time.time+.25f;visual.flipX=enemy.transform.position.x<transform.position.x;
                    if(ranged&&(GoldenWeaponQuest.Glock||GameSession.Current.allyAmmo>0))
                    {
                        Vector2 d=(enemy.transform.position-transform.position).normalized;CombatProjectile.Fire(transform.position,d,CombatTeam.Ally,23+bonus,projectileSprite);
                        if(!GoldenWeaponQuest.Glock)GameSession.Current.allyAmmo--;rounds--;nextAttack=Time.time+(rounds<=0?2.1f:GameSession.Current.pcAssembled?.38f:.7f);if(rounds<=0)rounds=6;
                    }
                    else if(!ranged){enemy.GetComponent<Health>().Damage((32+bonus+(GameSession.Current.pcAssembled?20:0))*(GoldenWeaponQuest.Razor?2:1),CombatTeam.Ally);nextAttack=Time.time+.7f;}
                }
                if(ranged&&Vector2.Distance(player.transform.position,transform.position)<5)destination=transform.position;
            }
            Vector2 deltaMove=destination-(Vector2)transform.position;
            if(deltaMove.magnitude<.6f){body.linearVelocity=Vector2.zero;return;}
            Vector2 direction=deltaMove.normalized;
            if(nav&&!nav.Clear(transform.position,destination))
            {
                if(Time.time>nextPath){path=nav.Path(transform.position,destination);waypoint=0;nextPath=Time.time+.65f;}
                if(path!=null&&waypoint<path.Count)
                {if(Vector2.Distance(transform.position,path[waypoint])<.3f)waypoint++;if(waypoint<path.Count)direction=(path[waypoint]-(Vector2)transform.position).normalized;else direction=Vector2.zero;}
            }
            float speed=Vector2.Distance(player.transform.position,transform.position)>5?5.4f:3.9f;
            body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,direction*speed,34*Time.fixedDeltaTime);
        }
        void LateUpdate()
        {
            visual.sortingOrder=WorldSort.At(transform.position.y);visual.transform.localRotation=Quaternion.Euler(0,0,health.Alive?0:70);
            if(body.linearVelocity.sqrMagnitude>.1f)visual.flipX=body.linearVelocity.x<-.2f;
            if(poses!=null&&poses.Length==4)visual.sprite=poses[Time.time<attackUntil?3:body.linearVelocity.magnitude>.2f?1+(Mathf.FloorToInt(Time.time*9)%2):0];
            visual.transform.localPosition=visualOrigin+Vector3.up*(body.linearVelocity.magnitude>.2f?Mathf.Abs(Mathf.Sin(Time.time*10))*.035f:0);
        }
    }
}
