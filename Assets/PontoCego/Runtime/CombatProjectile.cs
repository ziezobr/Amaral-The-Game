using UnityEngine;
namespace PontoCego
{
    public sealed class CombatProjectile:MonoBehaviour
    {
        public CombatTeam team;public float damage=10,speed=15,lifetime=2;public Vector2 direction;
        PlayerInputReader player;
        void Start(){player=FindAnyObjectByType<PlayerInputReader>();}
        void Update()
        {
            if(Time.timeScale<=0||(player&&player.GameplayLocked))return;Vector2 delta=direction*speed*Time.deltaTime;
            int mask=LayerMask.GetMask("World",team==CombatTeam.Enemy?"Player":"Enemy",team==CombatTeam.Enemy?"Ally":"Enemy");
            var hit=Physics2D.Raycast(transform.position,delta.normalized,delta.magnitude,mask);
            if(hit.collider){var hp=hit.collider.GetComponentInParent<Health>();if(hp)hp.Damage(damage,team);Destroy(gameObject);return;}
            transform.position+=(Vector3)delta;if((lifetime-=Time.deltaTime)<=0)Destroy(gameObject);
        }
        public static void Fire(Vector2 from,Vector2 direction,CombatTeam team,float damage,Sprite sprite)
        {
            FindAnyObjectByType<OfficeAudio>()?.Gunshot();
            var go=new GameObject("Projétil");go.layer=14;go.transform.position=from;go.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.color=team==CombatTeam.Enemy?new Color(1,.28f,.15f):new Color(1,.84f,.3f);sr.sortingOrder=4200;go.transform.localScale=new Vector3(.24f,.07f,1);
            var shot=go.AddComponent<CombatProjectile>();shot.direction=direction;shot.team=team;shot.damage=damage;
        }
    }
}
