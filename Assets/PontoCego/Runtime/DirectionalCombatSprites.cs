using UnityEngine;
namespace PontoCego
{
    /// <summary>Attack poses follow the locked strike direction; locomotion Animator remains independent.</summary>
    [DefaultExecutionOrder(30)]public sealed class DirectionalCombatSprites:MonoBehaviour
    {
        public Sprite[] poses,locomotion;public SpriteRenderer body;CombatPlayer combat;PlayerMotor motor;PlayerInputReader input;
        void Awake(){combat=GetComponent<CombatPlayer>();motor=GetComponent<PlayerMotor>();input=GetComponent<PlayerInputReader>();}
        void LateUpdate()
        {
            if(poses==null||poses.Length!=32||!body||!combat||input.GameplayLocked||!motor.enabled||motor.Model.Dodging)return;
            float time=Time.time-combat.AttackStarted;if(time<0||time>=.30f)return;
            var direction=combat.AttackDirection;int row=(Mathf.RoundToInt(Mathf.Atan2(-direction.x,-direction.y)*Mathf.Rad2Deg/45)+8)%8;
            int frame=time>.22f?3:time>.12f?2:time>.04f?1:0;body.sprite=poses[row*4+frame];body.flipX=false;
            if(locomotion!=null&&locomotion.Length==32){float ratio=locomotion[row*4+1].bounds.size.y/poses[row*4].bounds.size.y;body.transform.localScale=new Vector3(ratio,ratio,1);}
        }
    }
}
