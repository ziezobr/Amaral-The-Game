using UnityEngine;
namespace PontoCego
{
    public enum PhoneApp{Home,Uber,Map,Messages,Camera,Music,News,Browser,Videos}
    /// <summary>The supplied phone art remains intact. Hit areas use the original image coordinates.</summary>
    public static class GamePhoneView
    {
        public static readonly Rect Source=new Rect(704,22,512,1036);
        public static readonly Rect Device=new Rect(875,15,330,668);
        public static Rect Place(Rect source)=>new Rect(Device.x+(source.x-Source.x)/Source.width*Device.width,Device.y+(source.y-Source.y)/Source.height*Device.height,source.width/Source.width*Device.width,source.height/Source.height*Device.height);
        public static Rect? LocalContent;
        public static Rect Content=>LocalContent??Place(new Rect(746,160,423,697));
        static readonly PhoneApp[] apps={PhoneApp.News,PhoneApp.Videos,PhoneApp.Map,PhoneApp.Uber,PhoneApp.Browser,PhoneApp.Camera,PhoneApp.Messages,PhoneApp.Music};
        static readonly Rect[] spots={new Rect(762,774,83,85),new Rect(861,774,83,85),new Rect(961,774,91,85),new Rect(1068,774,86,85),new Rect(762,915,83,84),new Rect(861,915,83,84),new Rect(961,915,91,84),new Rect(1073,915,81,84)};
        public static bool Frame(Texture2D image,ref PhoneApp app,string clock)
        {
            GUI.depth=-30;
            if(image)GUI.DrawTextureWithTexCoords(Device,image,new Rect(Source.x/image.width,1-(Source.y+Source.height)/image.height,Source.width/image.width,Source.height/image.height));
            else CleanGui.Round(Device,UiTheme.Surface);
            GUI.Label(Place(new Rect(856,60,130,38)),clock,new GUIStyle(GUI.skin.label){fontSize=UiTheme.Font(13),fontStyle=FontStyle.Bold,normal={textColor=Color.white}});
            if(app==PhoneApp.Home){for(int i=0;i<spots.Length;i++)if(GUI.Button(Place(spots[i]),new GUIContent("",apps[i].ToString()),GUIStyle.none)){app=apps[i];UiSound.PlayClick();}}
            else if(app==PhoneApp.Uber){if(GUI.Button(Place(new Rect(750,130,130,42)),"‹ INÍCIO",UiTheme.Label(11,Color.white,true))){app=PhoneApp.Home;UiSound.PlayClick();}}
            else
            {
                var screen=Place(new Rect(730,135,459,751));CleanGui.Round(screen,UiTheme.Surface);
                GUI.Label(Place(new Rect(752,145,412,42)),Title(app),new GUIStyle(GUI.skin.label){fontSize=UiTheme.Font(16),fontStyle=FontStyle.Bold,normal={textColor=Color.white}});
                if(GUI.Button(Place(new Rect(777,938,363,51)),"INÍCIO",ButtonStyle)){app=PhoneApp.Home;UiSound.PlayClick();}
            }
            bool close=GUI.Button(new Rect(Device.x,Device.yMax+4,Device.width,28),"FECHAR CELULAR / T",ButtonStyle);if(close)UiSound.PlayClick();return close;
        }
        static string Title(PhoneApp app)=>app==PhoneApp.Uber?"Uber / Higor":app==PhoneApp.Map?"Mapa / Patos de Minas":app==PhoneApp.Messages?"Mensagens":app==PhoneApp.Camera?"Câmera":app==PhoneApp.Music?"Música":app==PhoneApp.News?"Comunidade de Patos":app==PhoneApp.Browser?"Informações locais":"Galeria de vídeos";
        public static GUIStyle Text=>UiTheme.Label(13);
        public static GUIStyle ButtonStyle=>new GUIStyle(GUI.skin.button){fontSize=UiTheme.Font(12),wordWrap=true,padding=new RectOffset(5,5,3,3)};
        public static bool Button(ref float y,string label){var c=Content;bool click=GUI.Button(new Rect(c.x,y,c.width,38),label,ButtonStyle);y+=46;if(click)UiSound.PlayClick();return click;}
        public static void Message(ref float y,string text,float height=90){var c=Content;var style=Text;float actual=Mathf.Max(height,style.CalcHeight(new GUIContent(text),c.width));GUI.Label(new Rect(c.x,y,c.width,actual),text,style);y+=actual+8;}
    }
}
