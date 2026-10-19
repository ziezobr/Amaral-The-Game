using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    [RequireComponent(typeof(Rigidbody2D),typeof(CircleCollider2D))]
    public sealed class CivilianFollower:MonoBehaviour
    {
        Rigidbody2D body;PlayerInputReader player;GridNavigator nav;SpriteRenderer visual;List<Vector2> path;int point;float refresh;
        void Awake(){body=GetComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;GetComponent<CircleCollider2D>().radius=.2f;visual=GetComponentInChildren<SpriteRenderer>();}
        void Start(){player=FindAnyObjectByType<PlayerInputReader>();nav=FindAnyObjectByType<GridNavigator>();}
        void FixedUpdate()
        {
            if(!player||player.Paused||player.GameplayLocked){body.linearVelocity=Vector2.zero;return;}
            Vector2 destination=(Vector2)player.transform.position+new Vector2(.9f,-1.25f);Vector2 delta=destination-(Vector2)transform.position;
            if(delta.magnitude<.5f){body.linearVelocity=Vector2.zero;return;}
            Vector2 dir=delta.normalized;if(nav&&!nav.Clear(transform.position,destination))
            {
                if(Time.time>refresh){path=nav.Path(transform.position,destination);point=0;refresh=Time.time+.6f;}
                if(path!=null&&point<path.Count){if(Vector2.Distance(transform.position,path[point])<.3f)point++;if(point<path.Count)dir=(path[point]-(Vector2)transform.position).normalized;else dir=Vector2.zero;}
            }
            body.linearVelocity=Vector2.MoveTowards(body.linearVelocity,dir*(delta.magnitude>4?5.5f:4),35*Time.fixedDeltaTime);
        }
        void LateUpdate(){if(visual){visual.sortingOrder=WorldSort.At(transform.position.y);if(body.linearVelocity.x<-.3f)visual.flipX=true;else if(body.linearVelocity.x>.3f)visual.flipX=false;}}
    }
}
