using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    public enum HubDestination { MainMission, Mocambo, CourierStart, CourierFinish, StreetPhoto, LostCamera }
    /// <summary>Street mission entrances and one-time side jobs share persisted, idempotent rewards.</summary>
    public sealed class HubGate:MonoBehaviour,IInteractable
    {
        public HubDestination destination;public string label;
        public string Prompt=>label;
        public void Interact()
        {
            var s=GameSession.Current;var say=FindAnyObjectByType<Interaction>();
            if(destination==HubDestination.MainMission){GameSession.EnterMission(s.mainMission);return;}
            if(destination==HubDestination.Mocambo){GameSession.EnterMission("24_Mocambo");return;}
            if(destination==HubDestination.CourierStart){s.deliveryActive=true;GameSession.Save();say.Say("Entrega: leve a encomenda da oficina à padaria. Recompensa: R$ 120.");return;}
            if(destination==HubDestination.CourierFinish)
            {
                if(!s.deliveryActive){say.Say("A padaria aguarda uma encomenda. Fale com o funcionário da oficina.");return;}
                if(GameSession.RewardOnce("bairro:entrega",120)){s.deliveryActive=false;say.Say("Entrega concluída! R$ 120. O Henzo vende munição e melhorias na oficina.");}
                else say.Say("A encomenda já foi entregue.");
            }
            if(destination==HubDestination.StreetPhoto)
            {
                var aim=FindAnyObjectByType<PlayerAim>();
                if(aim&&aim.FlashIntensity>0){GameSession.RewardOnce("bairro:foto",100);say.Say("Ensaio urbano concluído: R$ 100 pela foto da praça.");}
                else {say.Say("Ensaio urbano: mire na praça e fotografe com o botão esquerdo. Depois pressione E para entregar a foto.");s.photoJobActive=true;}
            }
            if(destination==HubDestination.LostCamera){GameSession.RewardOnce("bairro:camera",150);say.Say("Você recuperou a câmera perdida de um morador. Recompensa: R$ 150.");}
            GameSession.Save();
        }
    }
    public sealed class HubPhotoJob:MonoBehaviour
    {
        void Update()
        {
            var s=GameSession.Current;if(!s.photoJobActive||s.collected.Contains("bairro:foto"))return;
            var p=FindAnyObjectByType<PlayerAim>();if(!p||p.FlashIntensity<=0||Vector2.Distance(p.transform.position,transform.position)>4.5f)return;
            Vector2 d=transform.position-p.transform.position;if(Vector2.Dot(p.Direction,d.normalized)<.8f)return;
            if(Physics2D.Raycast(p.transform.position,d.normalized,d.magnitude,LayerMask.GetMask("World")))return;
            GameSession.RewardOnce("bairro:foto",100);s.photoJobActive=false;GameSession.Save();FindAnyObjectByType<Interaction>().Say("Foto da praça entregue: R$ 100. Trabalho concluído!");
        }
    }
    public sealed class StreetWalker:MonoBehaviour
    {
        public Vector2[] route;public Sprite[] frames;public SpriteRenderer visual;public float speed=1.4f;int index;float wait;Rigidbody2D body;PlayerInputReader input;
        void Start(){body=GetComponent<Rigidbody2D>();input=FindAnyObjectByType<PlayerInputReader>();index=Random.Range(0,route.Length);}
        void FixedUpdate()
        {
            if(input&&(input.Paused||input.GameplayLocked)){body.linearVelocity=Vector2.zero;return;}
            if(wait>0){wait-=Time.fixedDeltaTime;body.linearVelocity=Vector2.zero;return;}
            Vector2 delta=route[index]-body.position;
            if(delta.magnitude<.3f){index=(index+1)%route.Length;wait=Random.Range(.4f,2.5f);return;}
            Vector2 dir=delta.normalized;
            var hit=Physics2D.CircleCast(body.position,.25f,dir,.8f,LayerMask.GetMask("World"));
            if(hit.collider){index=(index+1)%route.Length;body.linearVelocity=Vector2.zero;wait=.5f;return;}
            body.linearVelocity=dir*speed;
        }
        void LateUpdate()
        {
            visual.sortingOrder=1000-Mathf.RoundToInt(transform.position.y*100);
            if(body.linearVelocity.sqrMagnitude>.1f){visual.flipX=body.linearVelocity.x<0;visual.sprite=frames[Mathf.FloorToInt(Time.time*7)%4];}
            else visual.sprite=frames[0];
        }
    }
    public sealed class VehicleDoor:MonoBehaviour,IInteractable
    {
        public DriveableCar car;public string Prompt=>"Entrar no veículo • E";public void Interact(){car.Enter();}
    }
    public sealed class DriveableCar:MonoBehaviour
    {
        public static DriveableCar Current{get;private set;}
        public Sprite[] frames;public SpriteRenderer visual;public float topSpeed=10,acceleration=11;
        Rigidbody2D body;PlayerMotor player;PlayerInputReader input;SpriteRenderer[] hidden;bool[] visibility;
        public void Enter()
        {
            if(Current)return;player=FindAnyObjectByType<PlayerMotor>();input=player.GetComponent<PlayerInputReader>();
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
        void Awake(){body=GetComponent<Rigidbody2D>();}
        void FixedUpdate()
        {
            if(Current!=this)return;
            if(input.Paused||input.GameplayLocked){body.linearVelocity=Vector2.zero;return;}
            Vector2 move=Vector2.ClampMagnitude(input.Move,1);body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,move*(input.Sprint?topSpeed:topSpeed*.65f),acceleration*Time.fixedDeltaTime);
            player.Body.position=body.position;player.transform.position=body.position;
            if(body.linearVelocity.sqrMagnitude>.15f){Vector2 d=body.linearVelocity;int frame=Mathf.Abs(d.x)>Mathf.Abs(d.y)?(d.x>0?1:3):(d.y>0?2:0);visual.sprite=frames[frame];}
            visual.sortingOrder=1000-Mathf.RoundToInt(transform.position.y*100);
        }
        void OnDestroy(){if(Current==this)Current=null;}
    }
}
