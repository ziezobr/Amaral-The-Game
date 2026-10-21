using UnityEngine;
namespace PontoCego
{
    /// <summary>Third-phase reinforcement: jab, cross and sweeping finisher with a real recovery window.</summary>
    public sealed class HigorMartialArts:MonoBehaviour
    {
        public Sprite[] frames;public SpriteRenderer visual;Rigidbody2D body;float clock;int strike;EnemyAgent target;PlayerInputReader input;
        public int ComboStep=>strike; public int FinishedCombos{get;private set;}
        public bool AttackVisualActive=>target&&body&&body.linearVelocity.sqrMagnitude<.1f&&clock<.3f;
        void Start(){body=GetComponent<Rigidbody2D>();input=FindAnyObjectByType<PlayerInputReader>();}
        void FixedUpdate()
        {
            if(!input||input.Paused||input.GameplayLocked){body.linearVelocity=Vector2.zero;return;}
            if(!target||!target.Alive){target=null;foreach(var e in FindObjectsByType<EnemyAgent>())if(e.Alive&&e.role==EnemyRole.Cenoura)target=e;}
            if(!target){body.linearVelocity=Vector2.zero;return;}
            Vector2 d=target.transform.position-transform.position;
            if(d.magnitude>1.3f){var nav=FindAnyObjectByType<GridNavigator>();Vector2 dir=d.normalized;if(nav&&!nav.Clear(transform.position,target.transform.position)){var path=nav.Path(transform.position,target.transform.position);if(path.Count>0)dir=(path[0]-(Vector2)transform.position).normalized;}body.linearVelocity=dir*4.8f;return;}
            body.linearVelocity=Vector2.zero;clock-=Time.fixedDeltaTime;if(clock<=0){strike=(strike+1)%3;target.GetComponent<Health>().Damage(strike==2?65:24,CombatTeam.Ally);if(strike==2){target.Stun(.5f);target.Knockback(d.normalized*5);FinishedCombos++;}clock=strike==2?2.4f:.22f;}
        }
        void LateUpdate(){if(!visual)return;visual.sortingOrder=WorldSort.At(transform.position.y);if(frames!=null&&frames.Length>0){int f=body&&body.linearVelocity.sqrMagnitude>.1f?1+Mathf.FloorToInt(Time.time*9)%3:clock<.3f?4+strike:0;visual.sprite=frames[Mathf.Min(f,frames.Length-1)];visual.transform.localRotation=Quaternion.Euler(0,0,clock<.2f?(strike==2?24:-10):0);}}
    }
}
