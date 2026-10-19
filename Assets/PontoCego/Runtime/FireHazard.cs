using UnityEngine;
namespace PontoCego
{
    public sealed class FireHazard:MonoBehaviour
    {
        public Vector2 size=new Vector2(2,2);public float damage=8;float next;
        void Update()
        {
            var player=FindAnyObjectByType<PlayerInputReader>();if(!player||player.Paused||player.GameplayLocked||Time.time<next)return;
            Vector2 d=player.transform.position-transform.position;
            if(Mathf.Abs(d.x)<size.x*.5f&&Mathf.Abs(d.y)<size.y*.5f){player.GetComponent<Health>().Damage(damage,CombatTeam.Enemy);next=Time.time+.7f;}
            transform.localScale=Vector3.one*(1+Mathf.Sin(Time.time*8)*.08f);
        }
    }
}
