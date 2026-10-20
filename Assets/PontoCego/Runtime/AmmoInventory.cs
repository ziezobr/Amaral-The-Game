using UnityEngine;
namespace PontoCego
{
    /// <summary>Finite resources are persistent; ordinary evidence photos never require stun charges.</summary>
    public sealed class AmmoInventory:MonoBehaviour
    {
        public Sprite bullet;float nextShot,reloading;
        public int MagazineCapacity=>8+GameSession.Current.weaponLevel*2;
        public bool Reloading=>reloading>0;
        public float ReloadProgress=>Reloading?1-Mathf.Clamp01(reloading/1.2f):0;
        public float LastShotAt{get;private set;}=-100;
        public Vector2 LastShotDirection{get;private set;}
        float heat;
        public static float SpreadDegrees(float heat,float movement)=>Mathf.Lerp(.3f,4f,Mathf.Clamp01(heat))+Mathf.Clamp01(movement/5)*2;
        PlayerInputReader input;PlayerAim aim;
        void Awake(){input=GetComponent<PlayerInputReader>();aim=GetComponent<PlayerAim>();}
        public bool SpendFlash()
        {
            var s=GameSession.Current;if(s.flashCharges<=0)return false;s.flashCharges--;return true;
        }
        public void Reload()
        {
            var s=GameSession.Current;if(reloading>0||s.magazine>=MagazineCapacity||s.ammoReserve<=0)return;reloading=1.2f;
        }
        public bool Shoot()
        {
            if(input.Paused||input.GameplayLocked||DriveableCar.Current||!GetComponent<Health>().Alive)return false;
            var s=GameSession.Current;if(!s.hasSidearm||Reloading||Time.time<nextShot)return false;
            if(GetComponent<PlayerMotor>().Model.Dodging||GetComponent<CombatPlayer>().Attacking)return false;
            if(s.magazine<=0){Reload();return false;}
            float spread=SpreadDegrees(heat,GetComponent<Rigidbody2D>().linearVelocity.magnitude);
            Vector2 direction=Quaternion.Euler(0,0,Random.Range(-spread,spread))*aim.Direction;
            s.magazine--;nextShot=Time.time+.32f;LastShotAt=Time.time;LastShotDirection=direction;heat=Mathf.Min(1,heat+.22f);
            // Fire from the physical body, so the muzzle cannot skip a wall at close range.
            CombatProjectile.Fire(transform.position,direction,CombatTeam.Player,24+s.weaponLevel*4,bullet);return true;
        }
        void Update()
        {
            if(input.Paused||input.GameplayLocked||!GetComponent<Health>().Alive||DriveableCar.Current)return;
            heat=Mathf.MoveTowards(heat,0,Time.deltaTime*.8f);
            if(reloading>0){reloading-=Time.deltaTime;if(reloading<=0){var s=GameSession.Current;int n=Mathf.Min(MagazineCapacity-s.magazine,s.ammoReserve);s.magazine+=n;s.ammoReserve-=n;GameSession.Save();}}
            var shoot=input.Actions.FindAction("Shoot");if(shoot!=null&&shoot.IsPressed())Shoot();
            var reload=input.Actions.FindAction("Reload");if(reload!=null&&reload.WasPressedThisFrame())Reload();
        }
    }
}
