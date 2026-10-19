using System.Collections;using UnityEngine;using UnityEngine.AI;
namespace PontoCego3D
{
    public enum Role3D{Player,Civilian,Enemy}
    public sealed class CombatActor:MonoBehaviour
    {
        public Role3D role;public Animator animator;public Transform visual;public float maximum=100;
        public float Health{get;private set;}public bool Alive=>Health>0;public bool Attacking=>Time.time<attackUntil;public bool Stunned=>Time.time<stunUntil;
        public string displayName="Capanga";public float Windup{get;private set;}
        float protection,attackUntil,stunUntil,lastDamage=-10,nextAttack;int combo;public float HurtAt{get;private set;}=-10;
        public event System.Action<CombatActor> Died;
        void Awake()=>Health=maximum;
        void Update()
        {
            if(role!=Role3D.Player||!Alive)return;var input=GetComponent<ThirdPersonInput>();var motor=GetComponent<ThirdPersonMotor>();
            if(input.Paused||motor.Locked||Stunned)return;
            if(input.Down("Attack")&&!Attacking&&!motor.Dodging)
            {
                if(input.Aim)Flash();else StartMelee(combo++%2==0?"Jab":"Cross",combo%2==0?26:20);
            }
            if(input.Down("Flash"))Flash();
            if(input.Down("Heal"))FindAnyObjectByType<OpenDistrict>().UseKit();
        }
        public void Protect(float seconds)=>protection=Mathf.Max(protection,Time.time+seconds);
        public bool Damage(float amount,CombatActor from,Vector3 direction)
        {
            if(!Alive||Time.time<protection||Time.time-lastDamage<.18f||from&&from.role==role)return false;
            if(role==Role3D.Civilian){GetComponent<StreetPedestrian>()?.Flee(transform.position-direction*2);return false;}
            Health=Mathf.Max(0,Health-amount);lastDamage=HurtAt=Time.time;
            if(Health<=0)
            {
                animator.CrossFadeInFixedTime("Death",.08f);var agent=GetComponent<NavMeshAgent>();if(agent)agent.enabled=false;
                var col=GetComponent<CapsuleCollider>();if(col)col.enabled=false;Died?.Invoke(this);
                if(role==Role3D.Enemy)FindAnyObjectByType<OpenDistrict>().EnemyDown();return true;
            }
            animator.CrossFadeInFixedTime("Hit",.05f);stunUntil=Time.time+.32f;
            var brain=GetComponent<StreetPedestrian>();if(brain)brain.Alert(from?from.transform.position:transform.position-direction);
            if(role==Role3D.Player)Protect(.55f);return true;
        }
        public void Restore(){Health=maximum;protection=Time.time+1;stunUntil=attackUntil=0;var collider=GetComponent<CapsuleCollider>();if(collider)collider.enabled=true;animator.Play("Locomotion");}
        public void Heal(float amount)=>Health=Mathf.Min(maximum,Health+amount);
        public void Stun(float seconds){if(!Alive)return;stunUntil=Mathf.Max(stunUntil,Time.time+seconds);animator.CrossFadeInFixedTime("Hit",.06f);}
        public void StartMelee(string clip="Jab",float damage=12)
        {
            if(!Alive||Attacking||Stunned)return;attackUntil=Time.time+.65f;animator.CrossFadeInFixedTime(clip,.055f);StartCoroutine(MeleeHit(damage));
            FindAnyObjectByType<OpenDistrict>().RaiseAlarm(transform.position);
        }
        IEnumerator MeleeHit(float damage)
        {
            float start=Time.time;while(Time.time-start<.23f){Windup=Mathf.InverseLerp(start,start+.23f,Time.time);yield return null;}Windup=0;
            if(!Alive||Stunned)yield break;Vector3 point=transform.position+Vector3.up*.95f+visual.forward*.8f;
            foreach(var col in Physics.OverlapSphere(point,1.0f,(1<<9)|(1<<12)|(1<<13),QueryTriggerInteraction.Ignore))
            {
                var other=col.GetComponentInParent<CombatActor>();if(!other||other==this||other.role==role)continue;
                Vector3 delta=other.transform.position-transform.position;
                if(delta.magnitude>1.9f||Vector3.Dot(visual.forward,delta.normalized)<.05f)continue;
                if(Physics.Linecast(transform.position+Vector3.up,other.transform.position+Vector3.up,1<<8))continue;
                if(other.Damage(damage,this,delta.normalized))FindAnyObjectByType<OpenDistrict>().Feedback("IMPACTO",.5f);
            }
        }
        public void Flash()
        {
            if(Time.time<nextAttack||!Alive)return;nextAttack=Time.time+1.1f;var district=FindAnyObjectByType<OpenDistrict>();if(district.flashCharges<=0){district.Feedback("Sem cargas de flash. Reabasteça antes de enfrentar os capangas.");return;}district.flashCharges--;district.FlashAt=Time.time;
            Vector3 origin=transform.position+Vector3.up*1.4f;var camera=Camera.main;Vector3 direction=GetComponent<ThirdPersonMotor>()&&GetComponent<ThirdPersonMotor>().orbit.isometric?visual.forward:camera.transform.forward;
            foreach(var enemy in FindObjectsByType<CombatActor>())
            {
                if(enemy.role!=Role3D.Enemy||!enemy.Alive)continue;Vector3 delta=enemy.transform.position+Vector3.up-origin;
                if(delta.magnitude<9&&Vector3.Dot(direction,delta.normalized)>.72f&&!Physics.Raycast(origin,delta.normalized,delta.magnitude,1<<8))enemy.Stun(1.8f);
            }
            district.Photograph();district.RaiseAlarm(transform.position);
        }
    }
}
