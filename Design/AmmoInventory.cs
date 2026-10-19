using UnityEngine;
namespace PontoCego
{
    /// <summary>Finite resources are persistent; ordinary evidence photos never require stun charges.</summary>
    public sealed class AmmoInventory:MonoBehaviour
    {
        public Sprite bullet;float nextShot,reloading;
        public int MagazineCapacity=>8+GameSession.Current.weaponLevel*2;
        public bool Reloading=>reloading>0;
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
            var s=GameSession.Current;if(!s.hasSidearm||Reloading||Time.time<nextShot)return false;
            if(s.magazine<=0){Reload();return false;}
            s.magazine--;nextShot=Time.time+.32f;CombatProjectile.Fire((Vector2)transform.position+aim.Direction*.4f,aim.Direction,CombatTeam.Player,24+s.weaponLevel*4,bullet);return true;
        }
        void Update()
        {
            if(input.Paused||input.GameplayLocked||!GetComponent<Health>().Alive||DriveableCar.Current)return;
            if(reloading>0){reloading-=Time.deltaTime;if(reloading<=0){var s=GameSession.Current;int n=Mathf.Min(MagazineCapacity-s.magazine,s.ammoReserve);s.magazine+=n;s.ammoReserve-=n;GameSession.Save();}}
            var shoot=input.Actions.FindAction("Shoot");if(shoot!=null&&shoot.IsPressed())Shoot();
            var reload=input.Actions.FindAction("Reload");if(reload!=null&&reload.WasPressedThisFrame())Reload();
        }
    }
}
