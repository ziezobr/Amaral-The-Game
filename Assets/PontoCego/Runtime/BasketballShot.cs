using UnityEngine;
namespace PontoCego
{
    /// <summary>Readable bouncing ball: continuous collision checks prevent tunnelling through walls.</summary>
    public sealed class BasketballShot:MonoBehaviour
    {
        public Vector2 direction;public float damage=16,speed=7,lifetime=3;
        float age;int rebounds;SpriteRenderer visual;PlayerInputReader input;
        public static void Launch(Vector2 from,Vector2 direction,Sprite sprite,float damage)
        {
            var go=new GameObject("Pivô / bola de basquete");go.transform.position=from;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=4100;
            go.transform.localScale=Vector3.one*.65f;
            var ball=go.AddComponent<BasketballShot>();ball.direction=direction.normalized;ball.damage=damage;
        }
        void Start(){visual=GetComponent<SpriteRenderer>();input=FindAnyObjectByType<PlayerInputReader>();}
        void Update()
        {
            if(input&&(input.Paused||input.GameplayLocked))return;
            float distance=speed*Time.deltaTime;
            var hit=Physics2D.CircleCast(transform.position,.18f,direction,distance,LayerMask.GetMask("World","Player","Ally"));
            if(hit.collider)
            {
                var hp=hit.collider.GetComponentInParent<Health>();
                if(hp){hp.Damage(damage,CombatTeam.Enemy);Destroy(gameObject);return;}
                if(++rebounds>1){Destroy(gameObject);return;}
                direction=Vector2.Reflect(direction,hit.normal);transform.position=(Vector3)(hit.centroid+hit.normal*.05f);
            }
            else transform.position+=(Vector3)(direction*distance);
            age+=Time.deltaTime;visual.transform.rotation=Quaternion.Euler(0,0,-age*350);
            if(age>=lifetime)Destroy(gameObject);
        }
    }
}
