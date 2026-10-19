using UnityEngine;
namespace PontoCego
{
    /// <summary>Lane traffic uses physical bodies, braking distance and a closed authored road loop.</summary>
    public sealed class CityTraffic:MonoBehaviour
    {
        public Vector2[] route; public Sprite[] views;public float speed=4.2f;
        Rigidbody2D body;SpriteRenderer visual;BoxCollider2D box;PlayerInputReader input;int next;float blocked;
        readonly RaycastHit2D[] obstacleHits=new RaycastHit2D[32];
        readonly ContactFilter2D avoidanceFilter=new ContactFilter2D{useLayerMask=true,layerMask=Physics2D.DefaultRaycastLayers,useTriggers=false};
        void Start(){body=GetComponent<Rigidbody2D>();box=GetComponent<BoxCollider2D>();visual=GetComponentInChildren<SpriteRenderer>();input=FindAnyObjectByType<PlayerInputReader>();next=1;}
        void FixedUpdate()
        {
            if(!input)input=FindAnyObjectByType<PlayerInputReader>();if(!input||input.Paused){body.linearVelocity=Vector2.zero;return;}
            Vector2 delta=route[next]-body.position;if(delta.magnitude<.4f){next=(next+1)%route.Length;delta=route[next]-body.position;}
            Vector2 dir=delta.normalized;float target=speed;
            // Cast from the nose; never include this car in obstacle detection.
            float half=Mathf.Abs(dir.x)>.5f?box.size.x*.5f:box.size.y*.5f;
            int hitCount=Physics2D.CircleCast(body.position+dir*(half+.28f),.44f,dir,avoidanceFilter,obstacleHits,2.2f);
            // A saturated buffer is conservatively treated as a blocked lane.
            if(hitCount==obstacleHits.Length)target=0;
            for(int i=0;i<hitCount;i++){var hit=obstacleHits[i];if(!hit.collider||hit.collider.isTrigger||hit.rigidbody==body||hit.collider.transform.IsChildOf(transform))continue;if(hit.collider.gameObject.layer==8||hit.collider.GetComponentInParent<PlayerMotor>()||hit.collider.GetComponentInParent<StreetWalker>()||hit.collider.GetComponentInParent<CityTraffic>())target=Mathf.Min(target,Mathf.Max(0,(hit.distance-.4f)*2));}
            blocked=target<.2f?blocked+Time.fixedDeltaTime:0;
            body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,dir*target,7*Time.fixedDeltaTime);
            int frame=Mathf.Abs(dir.x)>.5f?(dir.x>0?3:1):(dir.y>0?2:0);
            if(views!=null&&views.Length>=4){visual.sprite=views[frame];float length=3.25f;float scale=length/Mathf.Max(visual.sprite.bounds.size.x,visual.sprite.bounds.size.y);visual.transform.localScale=Vector3.one*scale;box.size=Mathf.Abs(dir.x)>.5f?new Vector2(3.25f,1.4f):new Vector2(1.4f,3.25f);}
        }
        void LateUpdate(){if(visual)visual.sortingOrder=WorldSort.At(transform.position.y);}
    }
}


