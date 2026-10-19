using UnityEngine;
namespace PontoCego
{
    [DefaultExecutionOrder(30)]public sealed class BossCombatSprites:MonoBehaviour
    {
        public Sprite[] poses;EnemyAgent enemy;BossAbilities abilities;PlayerMotor player;
        void Start(){enemy=GetComponent<EnemyAgent>();abilities=GetComponent<BossAbilities>();player=FindAnyObjectByType<PlayerMotor>();}
        void LateUpdate()
        {
            if(!enemy||!enemy.Alive||poses==null||poses.Length!=4||!abilities)return;int frame=0;
            if(enemy.Stunned||abilities.Recovering)frame=3;
            else if(abilities.Charging)frame=enemy.role==EnemyRole.Cenoura?(abilities.ShieldCharge?1:2):1;
            else if(abilities.Dashing)frame=1;
            else if(enemy.WindupProgress>0||Time.time-enemy.LastAttackAt<.14f)frame=enemy.role==EnemyRole.Cenoura?3:2;
            enemy.visual.sprite=poses[frame];enemy.visual.sortingOrder=WorldSort.At(transform.position.y);
            if(abilities.Charging||abilities.Dashing)enemy.visual.flipX=abilities.PowerDirection.x<0;
            else if(enemy.WindupProgress>0||Time.time-enemy.LastAttackAt<.14f)enemy.visual.flipX=enemy.AttackDirection.x<0;
            else if(player)enemy.visual.flipX=player.transform.position.x<transform.position.x;
        }
    }
}
