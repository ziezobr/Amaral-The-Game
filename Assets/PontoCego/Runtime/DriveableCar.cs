using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    public sealed class DriveableCar:MonoBehaviour
    {
        public static DriveableCar Current{get;private set;}
        public Sprite[] frames;public SpriteRenderer visual;public float topSpeed=10,acceleration=11;
        Rigidbody2D body;PlayerMotor player;PlayerInputReader input;SpriteRenderer[] hidden;bool[] visibility;
        public void Enter()
        {
            if(Current||TaxiService.Instance&&TaxiService.Instance.TripActive)return;player=FindAnyObjectByType<PlayerMotor>();input=player.GetComponent<PlayerInputReader>();
            var traffic=GetComponent<CityTraffic>();if(traffic)traffic.enabled=false;body.bodyType=RigidbodyType2D.Dynamic;body.gravityScale=0;body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            Current=this;player.enabled=false;player.GetComponent<PlayerAim>().enabled=false;player.GetComponent<CombatPlayer>().enabled=false;
            hidden=player.GetComponentsInChildren<SpriteRenderer>();visibility=new bool[hidden.Length];for(int i=0;i<hidden.Length;i++){visibility[i]=hidden[i].enabled;hidden[i].enabled=false;}
            player.Body.linearVelocity=Vector2.zero;player.Body.simulated=false;player.GetComponent<Interaction>().Say("WASD / analógico: dirigir • Shift: acelerar • E: sair");
        }
        public void Exit()
        {
            if(!player)return;Physics2D.SyncTransforms();Vector2 point=body.position;bool found=false;
            for(float r=1.5f;r<=3.5f&&!found;r+=.5f)for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var candidate=body.position+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;if(Physics2D.OverlapCircle(candidate,.32f,LayerMask.GetMask("World")))continue;point=candidate;found=true;break;}
            if(!found){player.GetComponent<Interaction>().Say("A saída está bloqueada. Afaste o veículo.");return;}
            player.Body.position=point;player.transform.position=point;player.Body.simulated=true;player.enabled=true;player.GetComponent<PlayerAim>().enabled=true;player.GetComponent<CombatPlayer>().enabled=true;
            for(int i=0;i<hidden.Length;i++)if(hidden[i])hidden[i].enabled=visibility[i];Current=null;body.linearVelocity=Vector2.zero;player=null;
        }
        void Awake(){body=GetComponent<Rigidbody2D>();if(visual&&visual.sprite)visual.transform.localScale=Vector3.one*(3.2f/Mathf.Max(visual.sprite.bounds.size.x,visual.sprite.bounds.size.y));}
        void FixedUpdate()
        {
            if(Current!=this)return;
            if(input.Paused||input.GameplayLocked){body.linearVelocity=Vector2.zero;return;}
            Vector2 move=Vector2.ClampMagnitude(input.Move,1);body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,move*(input.Sprint?topSpeed:topSpeed*.65f),acceleration*Time.fixedDeltaTime);
            player.Body.position=body.position;player.transform.position=body.position;
            if(body.linearVelocity.sqrMagnitude>.15f){Vector2 d=body.linearVelocity;int frame=Mathf.Abs(d.x)>Mathf.Abs(d.y)?(d.x>0?1:3):(d.y>0?2:0);visual.sprite=frames[frame];visual.transform.localScale=Vector3.one*(3.2f/Mathf.Max(visual.sprite.bounds.size.x,visual.sprite.bounds.size.y));var door=GetComponentInChildren<VehicleDoor>();if(door)door.transform.localPosition=frame%2==0?Vector3.right*1.3f:Vector3.down*1.3f;GetComponent<BoxCollider2D>().size=frame%2==0?new Vector2(1.35f,2.9f):new Vector2(3.2f,1.35f);}
            visual.sortingOrder=WorldSort.At(transform.position.y);
        }
        void LateUpdate(){if(visual)visual.sortingOrder=WorldSort.At(transform.position.y);}
        void OnDestroy(){if(Current==this)Current=null;}
    }
}
