using UnityEngine;
namespace PontoCego
{
    public sealed class GameUiArt:ScriptableObject{public Sprite[] icons;public Sprite caramelo;public Texture2D mainMenu;public Texture2D phone;public Texture2D uber;}
    public static class CleanGui
    {
        static Texture2D circle;
        static Texture2D Circle
        {
            get{if(circle)return circle;circle=new Texture2D(128,128,TextureFormat.RGBA32,false);for(int y=0;y<128;y++)for(int x=0;x<128;x++)circle.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(64-Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(64,64)))));circle.Apply();return circle;}
        }
        public static void Round(Rect r,Color color)
        {
            if(r.width<=0||r.height<=0)return;float radius=CornerRadius(r);Color old=GUI.color;
            try
            {
                GUI.color=old*color;
                // Draw four unscaled corner quadrants and flat edges. Large panels stay rectangular.
                GUI.DrawTextureWithTexCoords(new Rect(r.x,r.y,radius,radius),Circle,new Rect(0,.5f,.5f,.5f));
                GUI.DrawTextureWithTexCoords(new Rect(r.xMax-radius,r.y,radius,radius),Circle,new Rect(.5f,.5f,.5f,.5f));
                GUI.DrawTextureWithTexCoords(new Rect(r.x,r.yMax-radius,radius,radius),Circle,new Rect(0,0,.5f,.5f));
                GUI.DrawTextureWithTexCoords(new Rect(r.xMax-radius,r.yMax-radius,radius,radius),Circle,new Rect(.5f,0,.5f,.5f));
                if(r.width>radius*2)GUI.DrawTexture(new Rect(r.x+radius,r.y,r.width-radius*2,r.height),Texture2D.whiteTexture);
                if(r.height>radius*2){GUI.DrawTexture(new Rect(r.x,r.y+radius,radius,r.height-radius*2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(r.xMax-radius,r.y+radius,radius,r.height-radius*2),Texture2D.whiteTexture);}
            }
            finally{GUI.color=old;}
        }
        public static float CornerRadius(Rect r){float half=Mathf.Max(0,Mathf.Min(r.width,r.height)*.5f);return Mathf.Min(r.width,r.height)<=54?half:Mathf.Min(14,half);}
        public static void Icon(Rect r,Sprite sprite)
        {
            if(!sprite)return;var t=sprite.textureRect;float ratio=t.width/t.height;float w=Mathf.Min(r.width,r.height*ratio),h=w/ratio;
            GUI.DrawTextureWithTexCoords(new Rect(r.center.x-w*.5f,r.center.y-h*.5f,w,h),sprite.texture,new Rect(t.x/sprite.texture.width,t.y/sprite.texture.height,t.width/sprite.texture.width,t.height/sprite.texture.height));
        }
        public static void ShadowLabel(Rect r,string text,GUIStyle style)
        {
            var shadow=r;shadow.position+=new Vector2(1,1);GUI.Label(shadow,text,new GUIStyle(style){normal={textColor=new Color(0,0,0,.95f)}});GUI.Label(r,text,style);
        }
        public static void OutlinedLabel(Rect r,string text,GUIStyle style)
        {
            var outline=new GUIStyle(style){normal={textColor=Color.black}};var side=r;side.x-=1;GUI.Label(side,text,outline);side.x+=2;GUI.Label(side,text,outline);side=r;side.y-=1;GUI.Label(side,text,outline);side.y+=2;GUI.Label(side,text,outline);GUI.Label(r,text,style);
        }
    }
}
