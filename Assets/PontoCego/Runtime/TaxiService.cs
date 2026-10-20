using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Phone bookings use a sixty-second game-time clock and an actual collision-aware arriving car.</summary>
    public sealed partial class TaxiService:MonoBehaviour
    {
        public static TaxiService Instance{get;private set;}
        public Sprite[] sprites;public float arrivalSeconds=10;
        public bool Requested{get;private set;}public bool Arrived{get;private set;}
        public float Remaining=>Requested?Mathf.Max(0,arrivalSeconds-elapsed):0;
        public Vector2 Pickup{get;private set;}public string Notice{get;private set;}="";
        public UberDriver Driver{get;private set;}
        public bool Approaching=>cab&&Requested&&!Arrived;
        float elapsed,departAt;Vector2[] route;int waypoint;Rigidbody2D body;GameObject cab;SpriteRenderer visual;BoxCollider2D collision;PlayerInputReader player;
        void Awake(){Instance=this;}
        void OnDestroy(){if(Instance==this)Instance=null;}
        public bool RequestPickup()
        {
            if(Requested)return false;player=FindAnyObjectByType<PlayerInputReader>();
            Vector2 wanted=ClosestStreet(player.transform.position);route=PlanApproach(wanted,player.transform.position);
            if(route==null){Notice="Sem acesso de carro aqui. Vá até uma rua e tente novamente.";return false;}
            Pickup=route[route.Length-1];elapsed=0;waypoint=1;Requested=true;Arrived=false;
            float length=0;for(int i=1;i<route.Length;i++)length+=Vector2.Distance(route[i-1],route[i]);departAt=Mathf.Max(0,arrivalSeconds-length/5.5f);
            Notice="Uber solicitado. Chegada em 10 segundos.";return true;
        }
        public void Cancel(){if(TripActive){Notice="Aguarde o desembarque para encerrar a corrida.";return;}if(cab)Destroy(cab);cab=null;Driver=null;Requested=Arrived=false;elapsed=0;Notice="Corrida cancelada.";}
        void Update()
        {
            if(TripActive){RideUpdate();return;}
            if(!Requested||player&&player.Paused)return;elapsed+=Time.deltaTime;
            if(!cab&&elapsed>=departAt)Spawn();
            if(cab&&!Arrived&&elapsed>=arrivalSeconds&&Vector2.Distance(body.position,Pickup)<1.2f)
            {Arrived=true;body.linearVelocity=Vector2.zero;Notice="Seu Uber chegou. Aproxime-se do motorista e pressione E.";FindAnyObjectByType<OfficeAudio>()?.Warning();}
        }
        void FixedUpdate()
        {
            if(TripActive){RideFixedUpdate();return;}
            if(!cab||Arrived||!Requested||player&&player.Paused){if(body)body.linearVelocity=Vector2.zero;return;}
            while(waypoint<route.Length&&Vector2.Distance(body.position,route[waypoint])<.22f)waypoint++;
            if(waypoint>=route.Length){body.linearVelocity=Vector2.zero;return;}
            Vector2 d=route[waypoint]-body.position;Vector2 next=Vector2.MoveTowards(body.position,route[waypoint],5.5f*Time.fixedDeltaTime);if(!CanAdvance(next,d))return;body.MovePosition(next);
            int frame=Mathf.Abs(d.x)>Mathf.Abs(d.y)?(d.x>0?1:3):(d.y>0?2:0);visual.sprite=sprites[frame];visual.transform.localScale=Vector3.one*(3.4f/Mathf.Max(visual.sprite.bounds.size.x,visual.sprite.bounds.size.y));collision.size=frame%2==0?new Vector2(1.55f,3.4f):new Vector2(3.4f,1.55f);
            if(Driver)Driver.transform.localPosition=frame%2==0?Vector3.right*1.35f:Vector3.down*1.35f;
            visual.sortingOrder=WorldSort.At(body.position.y);
        }
        void Spawn()
        {
            cab=new GameObject("Uber solicitado / chegada física");cab.layer=8;cab.transform.position=route[0];
            visual=cab.AddComponent<SpriteRenderer>();visual.sprite=sprites[0];float scale=3.4f/Mathf.Max(visual.sprite.bounds.size.x,visual.sprite.bounds.size.y);
            var carVisual=new GameObject("Carro do Uber");carVisual.transform.SetParent(cab.transform,false);var display=carVisual.AddComponent<SpriteRenderer>();display.sprite=visual.sprite;carVisual.transform.localScale=Vector3.one*scale;Destroy(visual);visual=display;
            body=cab.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.mass=8;body.bodyType=RigidbodyType2D.Kinematic;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;body.interpolation=RigidbodyInterpolation2D.Interpolate;
            collision=cab.AddComponent<BoxCollider2D>();collision.size=new Vector2(1.55f,3.4f);
            var door=new GameObject("Motorista / embarque");door.transform.SetParent(cab.transform,false);door.transform.localPosition=Vector3.right*1.35f;door.layer=10;
            door.AddComponent<CircleCollider2D>().isTrigger=true;var pickup=door.AddComponent<TaxiPickup>();pickup.service=this;
            Driver=door.AddComponent<UberDriver>();Driver.enabled=false;
        }
        public void OpenDestinations(){if(!Arrived)return;var phone=FindAnyObjectByType<PlayerPhone>();phone.OpenApp(PhoneApp.Uber);}
        public Vector2 ClosestStreet(Vector2 point)
        {
            Vector2 best=point;float score=float.MaxValue;
            void Segment(Vector2 a,Vector2 b){Vector2 d=b-a;float t=Mathf.Clamp01(Vector2.Dot(point-a,d)/d.sqrMagnitude);Vector2 p=a+d*t;float s=(point-p).sqrMagnitude;if(s<score){score=s;best=p;}}
            var world=WorldHub.Instance;if(world&&world.roadStarts!=null)for(int i=0;i<world.roadStarts.Length;i++)Segment(world.roadStarts[i],world.roadEnds[i]);
            foreach(float x in new[]{-10.88f,10.88f})Segment(new Vector2(x,-20),new Vector2(x,20));
            foreach(float y in new[]{9.38f,-5.55f})Segment(new Vector2(-30,y),new Vector2(30,y));
            Segment(new Vector2(86,-55),new Vector2(178,-55));return best;
        }
        public static Vector2[] PlanApproach(Vector2 wanted,Vector2 player)
        {
            const int n=81;const float step=.8f;Vector2 origin=wanted-Vector2.one*(n/2*step);var clear=new bool[n*n];int goal=-1;float best=float.MaxValue;
            Vector2 Point(int id)=>origin+new Vector2(id%n,id/n)*step;
            for(int id=0;id<clear.Length;id++)
            {
                clear[id]=!Physics2D.OverlapCircle(Point(id),1.9f,LayerMask.GetMask("World"));if(!clear[id])continue;
                float distance=(Point(id)-wanted).sqrMagnitude;if(distance<best){best=distance;goal=id;}
            }
            if(goal<0||best>36)return null;
            var parent=new int[n*n];for(int i=0;i<parent.Length;i++)parent[i]=-2;parent[goal]=-1;var queue=new Queue<int>();queue.Enqueue(goal);int start=-1;
            while(queue.Count>0)
            {
                int at=queue.Dequeue();if(Vector2.Distance(Point(at),Point(goal))>20&&Vector2.Distance(Point(at),player)>19){start=at;break;}
                int x=at%n,y=at/n;foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})
                {int nx=x+d.x,ny=y+d.y;if(nx<0||ny<0||nx>=n||ny>=n)continue;int next=ny*n+nx;if(!clear[next]||parent[next]!=-2)continue;parent[next]=at;queue.Enqueue(next);}
            }
            if(start<0)return null;var path=new List<Vector2>();for(int at=start;at>=0;at=parent[at])path.Add(Point(at));return path.ToArray();
        }
    }
}
