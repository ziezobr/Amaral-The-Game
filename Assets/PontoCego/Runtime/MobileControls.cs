using UnityEngine;
using UnityEngine.InputSystem;

namespace PontoCego
{
    /// <summary>Two floating touch sticks plus actions. Multi-touch routes through the same input intent.</summary>
    public sealed class MobileControls : MonoBehaviour
    {
        public PlayerInputReader input;
        public bool previewInEditor;
        int leftId=-1,rightId=-1;
        Vector2 leftOrigin,rightOrigin,left,right;
        bool sprint;
        bool Active => Application.isMobilePlatform || previewInEditor;
        Rect Safe => new Rect(Screen.safeArea.x,Screen.height-Screen.safeArea.yMax,Screen.safeArea.width,Screen.safeArea.height);
        Rect Button(int index)
        {
            var s=Safe;float size=Mathf.Clamp(s.height*.105f,46,82);int col=index%3,row=index/3;
            return new Rect(s.xMax-size*(col+1)-14*(col+1),s.yMax-size*(row+1)-18*(row+1),size,size);
        }
        void Update()
        {
            if(!Active || Touchscreen.current==null) return;
            bool dodge=false,interact=false,flash=false,melee=false,heal=false;
            if(input.Paused || input.GameplayLocked) { leftId=rightId=-1; left=right=Vector2.zero; input.Mobile(left,right,false,false,false,false); return; }
            foreach(var touch in Touchscreen.current.touches)
            {
                int id=touch.touchId.ReadValue();
                Vector2 raw=touch.position.ReadValue(); Vector2 p=new Vector2(raw.x,Screen.height-raw.y);
                if(touch.press.wasPressedThisFrame)
                {
                    if(Button(0).Contains(p)) { dodge=true; continue; }
                    if(Button(1).Contains(p)) { interact=true; continue; }
                    if(Button(2).Contains(p)) { flash=true; continue; }
                    if(Button(3).Contains(p)) { sprint=!sprint; continue; }
                    if(Button(4).Contains(p)) { melee=true; continue; }
                    if(Button(5).Contains(p)) { heal=true; continue; }
                    if(Button(6).Contains(p)) { input.GetComponent<AmmoInventory>()?.Shoot(); continue; }
                    if(Button(7).Contains(p)) { input.GetComponent<AmmoInventory>()?.Reload(); continue; }
                    if(Button(8).Contains(p)) { input.GetComponent<PlayerPhone>()?.Toggle(); continue; }
                    if(p.y < Safe.y+80) continue;
                    if(p.x<Safe.center.x && leftId<0) { leftId=id; leftOrigin=p; }
                    else if(p.x>=Safe.center.x && rightId<0) { rightId=id; rightOrigin=p; }
                }
                if(!touch.press.isPressed)
                {
                    if(id==leftId) {leftId=-1;left=Vector2.zero;}
                    if(id==rightId) {rightId=-1;right=Vector2.zero;}
                    continue;
                }
                float radius=Mathf.Clamp(Safe.height*.12f,48,90);
                if(id==leftId) left=Vector2.ClampMagnitude(new Vector2(p.x-leftOrigin.x,leftOrigin.y-p.y)/radius,1);
                if(id==rightId) right=Vector2.ClampMagnitude(new Vector2(p.x-rightOrigin.x,rightOrigin.y-p.y)/radius,1);
            }
            input.Mobile(left,right,sprint,dodge,interact,flash,melee,heal);
        }
        void OnGUI()
        {
            if(!Active || input.Paused || input.GameplayLocked) return;
            GUI.color=new Color(.3f,.9f,.8f,.65f);
            string[] names={"ESQUIVA","USAR","FLASH",sprint?"CORRER ✓":"CORRER","GOLPE","CURAR","ATIRAR","RECARGA","CELULAR"};
            for(int i=0;i<9;i++) GUI.Box(Button(i),names[i]);
            if(leftId>=0) GUI.Box(new Rect(leftOrigin.x-42+left.x*30,leftOrigin.y-42-left.y*30,84,84),"MOVER");
            if(rightId>=0) GUI.Box(new Rect(rightOrigin.x-42+right.x*30,rightOrigin.y-42-right.y*30,84,84),"MIRAR");
            GUI.color=Color.white;
        }
        void OnDisable() {leftId=rightId=-1;left=right=Vector2.zero;}
    }
}
