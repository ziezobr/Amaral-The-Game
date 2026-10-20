using System;
using UnityEngine;
namespace PontoCego
{
    public enum CombatTeam{Player,Ally,Enemy}
    /// <summary>One damage entry point, team filtering, invulnerability and exactly-once death notification.</summary>
    public sealed class Health:MonoBehaviour
    {
        public CombatTeam team;
        public float maximum=100;
        public bool boss;
        public float Current{get;private set;}
        public bool Alive=>Current>0;
        public float LastDamageTime{get;private set;}=-100;
        public event Action<Health> Died;public event Action<Health,float> Damaged;
        public float InvincibleRemaining{get;private set;}public float TemporaryHealth{get;private set;}float temporaryLeft;
        public void GrantInvincibility(float seconds){InvincibleRemaining=Mathf.Max(InvincibleRemaining,seconds);}
        public void GrantTemporaryHealth(float amount,float seconds){if(!Alive)return;float add=Mathf.Min(maximum-Current,amount);Current+=add;TemporaryHealth+=add;temporaryLeft=seconds;}
        void Update(){var input=GetComponent<PlayerInputReader>();if(input&&(input.Paused||input.GameplayLocked))return;InvincibleRemaining=Mathf.Max(0,InvincibleRemaining-Time.deltaTime);if(temporaryLeft>0){temporaryLeft=Mathf.Max(0,temporaryLeft-Time.deltaTime);if(temporaryLeft<=0){Current=Mathf.Max(1,Current-TemporaryHealth);TemporaryHealth=0;}}}
        float immuneUntil;
        SpriteRenderer visual;PlayerMotor motor;Color original;
        void Awake(){Current=maximum;visual=GetComponentInChildren<SpriteRenderer>();motor=GetComponent<PlayerMotor>();if(visual)original=visual.color;}
        public bool Damage(float amount,CombatTeam attacker,bool ignoreDefense=false)
        {
            var mateus=GetComponent<Mateus>();if(mateus&&amount>0)return mateus.ReceiveAttack(attacker);
            if(!Alive||amount<=0||Time.time<immuneUntil||InvincibleRemaining>0)return false;
            if(team==attacker||(team!=CombatTeam.Enemy&&attacker!=CombatTeam.Enemy))return false;
            if(team==CombatTeam.Player&&motor&&motor.Model!=null&&motor.Model.Dodging&&motor.Model.DodgeProgress<.72f)return false;
            var guard=GetComponent<EnemyAgent>();var ability=GetComponent<BossAbilities>();if(!ignoreDefense&&guard&&guard.role==EnemyRole.Cenoura&&!guard.Stunned&&!(ability&&ability.Recovering))amount*=.32f;
            if(!ignoreDefense&&guard&&guard.role!=EnemyRole.Cenoura)amount*=.88f;
            if(!ignoreDefense&&GetComponent<EnemyVariety>() is EnemyVariety variety)amount=variety.AdjustIncoming(amount);
            if(attacker==CombatTeam.Enemy&&team!=CombatTeam.Enemy)amount*=DifficultyRules.Incoming(DifficultyRules.Current);
            float before=Current;Current=Mathf.Max(0,Current-amount);float applied=before-Current;TemporaryHealth=Mathf.Max(0,TemporaryHealth-applied);LastDamageTime=Time.time;Damaged?.Invoke(this,applied);
            if(visual)FindAnyObjectByType<OfficeAudio>()?.Impact();
            immuneUntil=Time.time+(team==CombatTeam.Player ? .5f : team==CombatTeam.Ally ? .15f : 0f);
            if(!Alive)Died?.Invoke(this);
            return true;
        }
        public void Heal(float amount){if(Alive){Current=Mathf.Min(maximum,Current-TemporaryHealth+Mathf.Max(0,amount));TemporaryHealth=0;temporaryLeft=0;}}
        public void Restore(float value=-1){Current=value<0?maximum:Mathf.Clamp(value,1,maximum);immuneUntil=Time.time+1;}
        public void IncreaseMaximum(float amount){maximum+=amount;Current=Mathf.Min(maximum,Current+amount);}
        void LateUpdate()
        {
            if(visual&&Alive)visual.color=Time.time-LastDamageTime<.13f?new Color(1,.35f,.32f):original;
        }
    }
}
