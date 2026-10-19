using UnityEngine;using Unity.Cinemachine;
namespace PontoCego
{
    public sealed partial class TaxiService
    {
        public string Contacts{get{var hits=new Collider2D[32];int n=body?body.GetContacts(hits):0;string result="";for(int i=0;i<n;i++)result+=" ["+hits[i].name+" rb="+(hits[i].attachedRigidbody?hits[i].attachedRigidbody.bodyType.ToString():"static")+"]";return result;}}public string Diagnostics=>"cab="+(body?body.position.ToString():"null")+" node="+tripNode+"/"+(trip==null?0:trip.Length)+" target="+(trip!=null&&tripNode<trip.Length?trip[tripNode].ToString():"end");public bool TripActive=>rideState>0;public int RideState=>rideState;public string SelectedDestination{get;private set;}
        public int Fare{get;private set;}=5;public float TripDistance{get;private set;}
        int rideState,tripNode;float rideClock;Vector2[] trip;PlayerMotor rider;Collider2D riderCollider;SpriteRenderer[] riderRenderers;CinemachineCamera follow;Vector2 door,seat,exit;Transform oldFollow;LineRenderer doorLine;
        public bool SelectDestination(string id)
        {
            var mission=WorldHub.Instance?WorldHub.Instance.Find(id):null;if(mission==null||TripActive)return false;
            Vector2 start=Requested?Pickup:(Vector2)FindAnyObjectByType<PlayerMotor>().transform.position;var path=TaxiRoadGraph.Route(start,mission.entry);if(path==null){Notice="Não há rua conectada a esse destino.";return false;}
            SelectedDestination=id;float length=0;for(int i=1;i<path.Length;i++)length+=Vector2.Distance(path[i-1],path[i]);Fare=Mathf.Max(5,Mathf.CeilToInt(3+length*.12f));return true;
        }
        public bool BeginRide()
        {
            if(!Arrived||TripActive||!body)return false;rider=FindAnyObjectByType<PlayerMotor>();if(Vector2.Distance(rider.transform.position,body.position)>5){Notice="Aproxime-se da porta do carro e pressione E para embarcar.";return false;}
            if(string.IsNullOrEmpty(SelectedDestination)&&!SelectDestination(GameSession.Current.mainMission))return false;
            var target=WorldHub.Instance.Find(SelectedDestination);trip=TaxiCollisionRoute.Plan(body.position,target.entry,body);if(trip==null){Notice="Rota indisponível.";return false;}
            if(GameSession.Current.currency<Fare){Notice="Dinheiro insuficiente para esta corrida.";return false;}
            FindAnyObjectByType<PlayerPhone>()?.Close();rider.GetComponent<PlayerInputReader>().SetGameplayLocked(true);rider.enabled=false;riderCollider=rider.GetComponent<Collider2D>();riderRenderers=rider.GetComponentsInChildren<SpriteRenderer>();
            bool vertical=collision.size.y>collision.size.x;Vector2 side=vertical?Vector2.right:Vector2.down;if(Vector2.Dot((Vector2)rider.transform.position-body.position,side)<0)side=-side;door=body.position+side*1.55f;seat=body.position+side*.2f;
            follow=FindAnyObjectByType<CinemachineCamera>();oldFollow=follow.Follow;var doorObject=new GameObject("Porta do Peugeot / embarque");doorObject.transform.SetParent(cab.transform,false);doorLine=doorObject.AddComponent<LineRenderer>();doorLine.sharedMaterial=new Material(Shader.Find("Sprites/Default"));doorLine.startColor=doorLine.endColor=new Color(.65f,.68f,.7f);doorLine.startWidth=doorLine.endWidth=.055f;doorLine.positionCount=2;doorLine.sortingOrder=WorldSort.At(body.position.y)+3;
            rideState=1;rideClock=0;TripDistance=0;tripNode=0;Notice="Amaral está embarcando.";return true;
        }
        void RideUpdate()
        {
            if(player&&player.Paused)return;rideClock+=Time.deltaTime;
            if(doorLine){doorLine.enabled=rideState==1||rideState==2||rideState==4;if(doorLine.enabled){Vector2 side=(door-body.position).normalized;var hinge=(Vector3)(body.position+side*.72f);doorLine.SetPosition(0,hinge+Vector3.up*.45f);doorLine.SetPosition(1,hinge+(Vector3)side*.8f+Vector3.down*.35f);doorLine.sortingOrder=WorldSort.At(body.position.y)+3;}}

            if(rideState==1&&Vector2.Distance(rider.Body.position,door)<.18f){rideState=2;rideClock=0;riderCollider.enabled=false;}
            else if(rideState==2){rider.Body.position=Vector2.Lerp(door,seat,Mathf.Clamp01(rideClock/.7f));if(rideClock>=.7f){foreach(var sr in riderRenderers)sr.enabled=false;follow.Follow=cab.transform;GameSession.Current.currency-=Fare;GameSession.Save();rideState=3;rideClock=0;Notice="Em viagem com Higor. Acompanhe o trajeto.";}}
            else if(rideState==4){foreach(var sr in riderRenderers)sr.enabled=true;rider.Body.position=Vector2.Lerp(seat,exit,Mathf.Clamp01(rideClock/.75f));if(rideClock>=.75f){riderCollider.enabled=true;rider.enabled=true;rider.GetComponent<PlayerInputReader>().SetGameplayLocked(false);follow.Follow=oldFollow;rideState=0;WorldHub.Instance.Navigate(SelectedDestination);Notice="Você chegou. Caminhe até a entrada para iniciar a missão.";Arrived=true;}}
        }
        void RideFixedUpdate()
        {
            if(player&&player.Paused)return;
            if(rideState==1){Vector2 toDoor=door-rider.Body.position;rider.Model.Step(toDoor.normalized,false,false,Time.fixedDeltaTime);rider.Body.MovePosition(Vector2.MoveTowards(rider.Body.position,door,2.7f*Time.fixedDeltaTime));return;}
            if(rideState!=3)return;
            while(tripNode<trip.Length&&Vector2.Distance(body.position,trip[tripNode])<.04f)tripNode++;
            if(tripNode>=trip.Length){body.linearVelocity=Vector2.zero;bool vertical=collision.size.y>collision.size.x;Vector2 side=vertical?Vector2.left:Vector2.down;exit=body.position+side*1.7f;if(Physics2D.OverlapCircle(exit,.28f,LayerMask.GetMask("World")))exit=body.position-side*1.7f;seat=body.position;rideState=4;rideClock=0;return;}
            Vector2 d=trip[tripNode]-body.position;int frame=Mathf.Abs(d.x)>Mathf.Abs(d.y)?(d.x>0?1:3):(d.y>0?2:0);visual.sprite=sprites[frame];visual.transform.localScale=Vector3.one*(3.4f/Mathf.Max(visual.sprite.bounds.size.x,visual.sprite.bounds.size.y));collision.size=frame%2==0?new Vector2(1.55f,3.4f):new Vector2(3.4f,1.55f);
            Vector2 next=Vector2.MoveTowards(body.position,trip[tripNode],6.5f*Time.fixedDeltaTime);foreach(var hit in Physics2D.BoxCastAll(body.position,collision.size,0,d.normalized,Vector2.Distance(body.position,next)+.08f,LayerMask.GetMask("World")))if(hit.collider&&hit.rigidbody!=body&&!hit.collider.transform.IsChildOf(cab.transform)){Notice="Aguardando passagem: "+hit.collider.name;return;}
            TripDistance+=Vector2.Distance(body.position,next);body.MovePosition(next);rider.Body.position=next;visual.sortingOrder=WorldSort.At(next.y);
        }
    }
}
