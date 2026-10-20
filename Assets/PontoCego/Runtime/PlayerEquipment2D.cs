using UnityEngine;
using UnityEngine.InputSystem;
namespace PontoCego
{
    public enum PlayerEquipment { BlackCamera, PinkCamera, Fists }
    [DefaultExecutionOrder(-60)]
    public sealed class PlayerEquipment2D:MonoBehaviour
    {
        public PlayerEquipment Selected{get;private set;}
        public Sprite pinkIcon;public Sprite[] pinkIdle,pinkWalk;
        PlayerInputReader input;PlayerAim aim;ReferenceActorAnimation actor;Sprite black;Vector3 propScale;bool obtained;
        public bool CameraSelected=>Selected!=PlayerEquipment.Fists;
        public bool PinkSelected=>Selected==PlayerEquipment.PinkCamera;
        public string Label=>PinkSelected?"CÂMERA ROSA":CameraSelected?"CÂMERA PRETA":"MÃOS";
        void Awake(){input=GetComponent<PlayerInputReader>();aim=GetComponent<PlayerAim>();actor=GetComponent<ReferenceActorAnimation>();if(aim&&aim.cameraProp){black=aim.cameraProp.GetComponent<SpriteRenderer>().sprite;propScale=aim.cameraProp.localScale;}}
        void Update()
        {
            bool has=GameSession.Current.collected.Contains("weapon:xuxa");if(has&&!obtained){obtained=true;Select(PlayerEquipment.PinkCamera);}
            if(input.Paused||input.GameplayLocked||DriveableCar.Current)return;
            if(Mouse.current!=null){float scroll=Mouse.current.scroll.ReadValue().y;if(Mathf.Abs(scroll)>.1f)Cycle(scroll>0?1:-1);}
            if(Selected==PlayerEquipment.Fists&&(input.ConsumeFlash()||input.FlashHeld))GetComponent<CombatPlayer>().QueueAttack();
        }
        public void Cycle(int direction)
        {
            int at=(int)Selected;for(int i=0;i<3;i++){at=(at+(direction>=0?1:2))%3;if(at!=1||GameSession.Current.collected.Contains("weapon:xuxa")){Select((PlayerEquipment)at);return;}}
        }
        public bool Select(PlayerEquipment equipment)
        {if(equipment==PlayerEquipment.PinkCamera&&!GameSession.Current.collected.Contains("weapon:xuxa"))return false;Selected=equipment;return true;}
        public void RefreshVisual()
        {
            if(!aim||!aim.cameraProp)return;var prop=aim.cameraProp.GetComponent<SpriteRenderer>();if(DriveableCar.Current||TaxiService.Instance&&TaxiService.Instance.TripActive){prop.enabled=false;return;}
            bool held=PinkSelected&&pinkIdle!=null&&pinkIdle.Length==8&&actor;
            prop.enabled=CameraSelected&&!held&&!DriveableCar.Current;prop.sprite=PinkSelected?pinkIcon:black;
            aim.cameraProp.localScale=PinkSelected&&pinkIcon?Vector3.one*(.38f/Mathf.Max(.01f,pinkIcon.bounds.size.y)):propScale;
            if(held&&!GetComponent<CombatPlayer>().Attacking)
            {
                int dir=ReferenceActorAnimation.DirectionIndex(aim.Direction);var body=GetComponent<Rigidbody2D>();int count=pinkWalk==null?0:pinkWalk.Length/8;
                var sprite=body.linearVelocity.magnitude>.15f&&count>0?pinkWalk[dir*count+ReferenceActorAnimation.CycleFrame(Time.time*8,count)]:pinkIdle[dir];
                if(sprite){actor.visual.sprite=sprite;actor.visual.flipX=false;actor.visual.transform.localScale=Vector3.one*(actor.idle[0].bounds.size.y/sprite.bounds.size.y);}
            }
        }
        void OnGUI(){if(!input||input.Paused||input.GameplayLocked)return;var old=GUI.matrix;GUI.matrix=UiTheme.SafeMatrix();GUI.Label(new Rect(850,192,360,23),Label+" • SCROLL",new GUIStyle(UiTheme.Label(10,UiTheme.Accent,true)){alignment=TextAnchor.MiddleRight});GUI.matrix=old;}
    }
}
