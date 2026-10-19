using UnityEngine;using UnityEngine.AI;
namespace PontoCego3D
{
    [RequireComponent(typeof(NavMeshAgent),typeof(CombatActor))]
    public sealed class StreetPedestrian:MonoBehaviour
    {
        public Vector3[] stops;public bool hostile;public Animator animator;public Transform visual;
        NavMeshAgent agent;CombatActor actor;ThirdPersonMotor player;float wait,nextDecision,frightUntil;Vector3 home;
        public bool Fleeing=>Time.time<frightUntil;public bool Alerted{get;private set;}
        void Awake(){agent=GetComponent<NavMeshAgent>();actor=GetComponent<CombatActor>();home=transform.position;}
        void Start()
        {
            player=FindAnyObjectByType<ThirdPersonMotor>();if(NavMesh.SamplePosition(transform.position,out var hit,4,NavMesh.AllAreas)){transform.position=hit.position;agent.enabled=true;agent.Warp(hit.position);}
            wait=Time.time+Random.Range(.1f,2);
        }
        public void Flee(Vector3 threat)
        {
            if(hostile||!actor.Alive||!agent.enabled||!agent.isOnNavMesh)return;frightUntil=Time.time+8;agent.speed=4.4f;
            var away=(transform.position-threat).normalized;away.y=0;Vector3 point=transform.position+away*Random.Range(12,20);
            if(NavMesh.SamplePosition(point,out var hit,10,NavMesh.AllAreas))agent.SetDestination(hit.position);
        }
        public void Alert(Vector3 position){if(hostile)Alerted=true;else Flee(position);}
        void Update()
        {
            if(!player||!actor.Alive||!agent.enabled||!agent.isOnNavMesh)return;
            if(hostile&&player.Locked){agent.isStopped=true;animator.SetFloat("Speed",0);return;}
            if(actor.Stunned||actor.Attacking||!player.GetComponent<CombatActor>().Alive){agent.isStopped=true;animator.SetFloat("Speed",0);return;}
            agent.isStopped=false;
            if(hostile)
            {
                Vector3 delta=player.transform.position-transform.position;float distance=delta.magnitude;
                bool clear=!Physics.Linecast(transform.position+Vector3.up,player.transform.position+Vector3.up,1<<8);
                if(distance<10&&clear)Alerted=true;
                if(Alerted&&distance<28)
                {
                    agent.speed=3.7f;agent.stoppingDistance=1.3f;
                    if(Time.time>nextDecision){agent.SetDestination(player.transform.position);nextDecision=Time.time+.2f;}
                    if(distance<1.8f&&clear){visual.rotation=Quaternion.RotateTowards(visual.rotation,Quaternion.LookRotation(delta.WithoutY()),600*Time.deltaTime);actor.StartMelee(Random.value>.5f?"Jab":"Cross",12);}
                }
                else{Alerted=false;agent.stoppingDistance=.3f;if(Time.time>nextDecision){agent.SetDestination(home);nextDecision=Time.time+1;}}
            }
            else if(!agent.pathPending&&agent.remainingDistance<.7f&&Time.time>wait)
            {
                if(stops!=null&&stops.Length>0){var point=stops[Random.Range(0,stops.Length)];if(NavMesh.SamplePosition(point,out var hit,3,NavMesh.AllAreas))agent.SetDestination(hit.position);}
                if(!Fleeing)agent.speed=Random.Range(1.1f,1.8f);wait=Time.time+Random.Range(2,5);
            }
            var velocity=agent.velocity;animator.SetFloat("Speed",velocity.magnitude,.18f,Time.deltaTime);
            if(velocity.sqrMagnitude>.05f)visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(velocity.WithoutY()),1-Mathf.Exp(-Time.deltaTime*10));
        }
    }
    public static class VectorUtility{public static Vector3 WithoutY(this Vector3 v)=>new Vector3(v.x,0,v.z);}
}
