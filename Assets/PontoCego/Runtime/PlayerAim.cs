using UnityEngine;

namespace PontoCego
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerAim : MonoBehaviour
    {
        public Transform reticle;
        public Transform cameraProp;
        public Vector2 Direction { get; private set; } = Vector2.down;
        public float FlashRemaining => Mathf.Max(0f, nextFlash - Time.time);
        public float FlashIntensity=>Mathf.Clamp01((flashUntil-Time.time)/.1f);
        PlayerInputReader input;
        Camera view;
        float nextFlash, flashUntil;
        SpriteRenderer indicator;
        bool pointerMode;
        void Awake() { input = GetComponent<PlayerInputReader>(); view = Camera.main; indicator = reticle.GetComponent<SpriteRenderer>(); }
        public bool CanPhotograph(PhotoTarget target)
        {
            if(!target.Available)return false;Vector2 delta=target.transform.position-transform.position;
            if(delta.magnitude>4.5f||Vector2.Dot(Direction,delta.normalized)<.85f)return false;
            var hit=Physics2D.Raycast(transform.position,delta.normalized,delta.magnitude,LayerMask.GetMask("World"));return !hit.collider||hit.collider.transform.IsChildOf(target.transform);
        }
        void Update()
        {
            if (input.Paused || input.GameplayLocked || !view) return;
            if (input.PointerMoved) pointerMode = true;
            Vector2 stick = input.AimStick;
            if (stick.sqrMagnitude > .08f) { Direction = stick.normalized; pointerMode = false; }
            else if (pointerMode)
            {
                var point = view.ScreenToWorldPoint(new Vector3(input.Pointer.x, input.Pointer.y, -view.transform.position.z));
                var delta = (Vector2)(point - transform.position);
                if (delta.sqrMagnitude > .02f) Direction = delta.normalized;
            }
            reticle.localPosition = (Vector3)(Direction * 1.25f) + new Vector3(0, .3f, 0);
            cameraProp.localPosition = (Vector3)(Direction * .35f) + new Vector3(0, .65f, 0);
            cameraProp.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg);
            var equipment=GetComponent<PlayerEquipment2D>();
            if ((!equipment||equipment.CameraSelected)&&(input.ConsumeFlash()||input.FlashHeld) && Time.time >= nextFlash)
            {
                nextFlash = Time.time + Mathf.Max(.35f,.8f-GameSession.Current.flashLevel*.1f); flashUntil = Time.time + .1f;
                var audio=FindAnyObjectByType<OfficeAudio>();if(audio)audio.Shutter();
                bool powered=GetComponent<AmmoInventory>()?.SpendFlash()??true;
                foreach(var enemy in FindObjectsByType<EnemyAgent>())
                {
                    if(!powered||!enemy.Alive)continue;Vector2 delta=enemy.transform.position-transform.position;
                    if(delta.magnitude>4.5f||Vector2.Dot(Direction,delta.normalized)<.62f)continue;
                    if(Physics2D.Raycast(transform.position,delta.normalized,delta.magnitude,LayerMask.GetMask("World")))continue;
                    if(equipment&&equipment.PinkSelected){var hp=enemy.GetComponent<Health>();hp.Damage(hp.maximum*.1f,CombatTeam.Player,true);}
                    enemy.Stun(1.7f+GameSession.Current.flashLevel*.45f);
                }
                foreach (var target in FindObjectsByType<PhotoTarget>())
                {
                    if(CanPhotograph(target)){target.Photograph();break;}
                }
                foreach(var task in FindObjectsByType<GoldenQuestPickup>())task.TryPhotograph(this);
                foreach(var task in FindObjectsByType<CityContractPoint>())task.Photograph(this);
            }
            indicator.color = Time.time < flashUntil ? Color.white : new Color(.35f, 1f, .85f, .8f);
            indicator.transform.localScale = Vector3.one * (Time.time < flashUntil ? .65f : .3f)*GamePreferences.AimSize;
        }
    }
}
