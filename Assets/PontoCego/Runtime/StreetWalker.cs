using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
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
            visual.sortingOrder=WorldSort.At(transform.position.y);
            if(body.linearVelocity.sqrMagnitude>.1f){visual.flipX=body.linearVelocity.x<0;visual.sprite=frames[body.linearVelocity.y>.35f?3:1+Mathf.FloorToInt(Time.time*7)%2];}
            else visual.sprite=frames[0];
        }
    }
}
