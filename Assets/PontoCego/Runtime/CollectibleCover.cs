using UnityEngine;
namespace PontoCego
{
    public sealed class CollectibleCover:MonoBehaviour
    {
        public SpriteRenderer cover;public SpriteRenderer item;Color original;void Start(){if(cover)original=cover.color;}
        void LateUpdate(){var player=FindAnyObjectByType<PlayerMotor>();bool near=player&&Vector2.Distance(player.transform.position,transform.position)<2.2f;if(cover){cover.sortingOrder=WorldSort.At(transform.position.y)+3;cover.color=new Color(original.r,original.g,original.b,near?.28f:1);}if(item)item.sortingOrder=WorldSort.At(transform.position.y)+1;}
        public static void Hint(Vector3 at,string text)
        {
            if(!Camera.main)return;Vector3 p=Camera.main.WorldToScreenPoint(at+Vector3.up*.9f);if(p.z<0||p.x<0||p.x>Screen.width||p.y<0||p.y>Screen.height)return;
            var style=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,wordWrap=true,fontSize=14,padding=new RectOffset(10,10,6,6),normal={textColor=Color.white}};float width=Mathf.Min(360,Screen.width-30),height=Mathf.Max(30,style.CalcHeight(new GUIContent(text),width));var r=new Rect(Mathf.Clamp(p.x-width/2,10,Screen.width-width-10),Mathf.Clamp(Screen.height-p.y-height,10,Screen.height-height-10),width,height);var old=GUI.color;GUI.color=new Color(.02f,.04f,.05f,.9f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;GUI.Label(r,text,style);GUI.color=old;
        }
    }
}
