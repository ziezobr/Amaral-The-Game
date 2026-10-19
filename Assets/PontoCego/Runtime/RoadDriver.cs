using UnityEngine;
using Unity.Cinemachine;
namespace PontoCego
{
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class RoadDriver:MonoBehaviour
    {
        public Sprite[] ridingSprites;
        public float HazardSlow{get;set;}=1; public float cruiseSpeed=9f;public Vector2 worldCenter;
        Vector3 oldScale,oldPosition;Sprite oldSprite;float oldRadius;Vector2 oldOffset;bool restored;
        public float Progress{get;private set;}
        PlayerInputReader input;Rigidbody2D body;LevelProgress level;PlayerMotor motor;SpriteRenderer rider;
        bool stopped;float nextCrash;Transform ahead;
        void Start()
        {
            input=GetComponent<PlayerInputReader>();motor=GetComponent<PlayerMotor>();body=GetComponent<Rigidbody2D>();level=FindAnyObjectByType<LevelProgress>();
            motor.enabled=false;var animation=GetComponent<DirectionalAnimation>();animation.enabled=false;animation.animator.enabled=false;rider=animation.bodySprite;oldSprite=rider.sprite;oldScale=rider.transform.localScale;oldPosition=rider.transform.localPosition;
            rider.sprite=ridingSprites[1];rider.transform.localScale=Vector3.one*(3.6f/Mathf.Max(rider.sprite.bounds.size.x,rider.sprite.bounds.size.y));
            rider.transform.localPosition=new Vector3(0,-rider.sprite.bounds.size.y*rider.transform.localScale.y*.5f,0);
            var circle=GetComponent<CircleCollider2D>();oldRadius=circle.radius;oldOffset=circle.offset;circle.radius=.55f;circle.offset=Vector2.zero;
            ahead=new GameObject("Antecipação da câmera / estrada").transform;
            FindAnyObjectByType<CinemachineCamera>().Follow=ahead;
        }
        void FixedUpdate()
        {
            if(!input||input.Paused||input.GameplayLocked||!GetComponent<Health>().Alive){body.linearVelocity=Vector2.zero;return;}
            Vector2 move=input.Move;
            if(stopped){body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,move*5,45*Time.fixedDeltaTime);return;}
            float targetX=(cruiseSpeed+move.x*2+(input.Sprint?3:0))*HazardSlow;
            Vector2 velocity=new Vector2(targetX,move.y*7);
            if(transform.position.y-worldCenter.y>6.5f&&velocity.y>0)velocity.y=0;if(transform.position.y-worldCenter.y< -5.6f&&velocity.y<0)velocity.y=0;
            body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,velocity,22*Time.fixedDeltaTime);
            Progress=Mathf.Clamp01((transform.position.x-worldCenter.x+level.mapSize.x*.5f)/level.mapSize.x);
            var mission=FindAnyObjectByType<CombatMission>();
            if(level.CurrentTarget&&mission.rules[level.Step].kind==MissionRuleKind.Reach&&transform.position.x>=level.CurrentTarget.position.x)level.Advance();
            if(level.Step>=level.objectives.Length-1&&level.CurrentTarget&&transform.position.x>=level.CurrentTarget.position.x-2)stopped=true;
        }
        void LateUpdate(){if(ahead)ahead.position=transform.position+new Vector3(4,0,0);if(rider)rider.sortingOrder=WorldSort.At(transform.position.y);}
        void OnCollisionEnter2D(Collision2D collision)
        {
            if(Time.time<nextCrash||collision.gameObject.layer!=8)return;nextCrash=Time.time+1;GetComponent<Health>().Damage(9,CombatTeam.Enemy);
        }
        public void RestoreWalking()
        {
            if(restored||!motor)return;var chase=GetComponent<RoadChase>();if(chase)Destroy(chase);restored=true;enabled=false;body.linearVelocity=Vector2.zero;motor.enabled=true;
            var animation=GetComponent<DirectionalAnimation>();animation.enabled=true;animation.animator.enabled=true;
            if(rider){rider.sprite=oldSprite;rider.transform.localScale=oldScale;rider.transform.localPosition=oldPosition;}
            var circle=GetComponent<CircleCollider2D>();circle.radius=oldRadius;circle.offset=oldOffset;
            var camera=FindAnyObjectByType<CinemachineCamera>();if(camera)camera.Follow=transform;
            if(ahead)Destroy(ahead.gameObject);
        }
        void OnDestroy(){if(WorldHub.Instance)RestoreWalking();if(ahead)Destroy(ahead.gameObject);}
    }
}
