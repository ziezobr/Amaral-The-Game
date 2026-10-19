using UnityEngine;
namespace PontoCego
{
    [DefaultExecutionOrder(40)]public sealed class ReferenceActorAnimation:MonoBehaviour
    {
        public SpriteRenderer visual;public Sprite[] idle,walk;Rigidbody2D body;PlayerMotor player;EnemyAgent enemy;CombatPlayer combat;Vector2 facing=Vector2.down;
        void Start(){body=GetComponent<Rigidbody2D>();player=GetComponent<PlayerMotor>();enemy=GetComponent<EnemyAgent>();combat=GetComponent<CombatPlayer>();}
        void LateUpdate()
        {
            if(!visual||idle==null||idle.Length<8)return;if(enemy&&!enemy.Alive)return;
            if(combat&&Time.time-combat.AttackStarted<.42f)return;
            if(enemy){var b=enemy.GetComponent<BossAbilities>();if(enemy.WindupProgress>0||b&&(b.Charging||b.Dashing))return;}
            Vector2 velocity=body?body.linearVelocity:Vector2.zero;if(velocity.sqrMagnitude>.08f)facing=velocity.normalized;else if(player)facing=player.Model.Facing;
            int dir=Mathf.RoundToInt(Mathf.Atan2(-facing.x,-facing.y)/(Mathf.PI/4));dir=(dir+8)%8;
            visual.flipX=false;visual.sprite=velocity.sqrMagnitude>.08f&&walk!=null&&walk.Length>=24?walk[dir*3+Mathf.FloorToInt(Time.time*9)%3]:idle[dir];
            visual.transform.localScale=Vector3.one;visual.sortingOrder=WorldSort.At(transform.position.y);
        }
    }
}
