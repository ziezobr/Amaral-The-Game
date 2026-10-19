using UnityEngine;
namespace PontoCego
{
    /// <summary>Functional hit regions over the user's Uber artwork; fare and status are live values.</summary>
    public static class UberPhoneView
    {
        public static bool Draw(Texture2D image,string status,int fare,string destination,out bool choose)
        {
            Color previous=GUI.color;
            try
            {
                GUI.depth=-30;
                // GamePhoneView.Frame owns the artwork and navigation; redrawing it hides Home and the clock.
                var rect=GamePhoneView.Place(new Rect(826,209,270,38));GUI.color=previous*Color.white;GUI.DrawTexture(rect,Texture2D.whiteTexture);
                var stateStyle=new GUIStyle(UiTheme.Label(12,Color.black,true)){alignment=TextAnchor.MiddleCenter,wordWrap=false};Fit(stateStyle,status,rect.width-8);GUI.Label(rect,status,stateStyle);
                var money=GamePhoneView.Place(new Rect(785,636,370,65));GUI.color=previous*Color.black;GUI.DrawTexture(money,Texture2D.whiteTexture);GUI.color=previous;
                GUI.Label(money,"R$ "+fare,new GUIStyle(UiTheme.Label(27,UiTheme.Money,true)){alignment=TextAnchor.MiddleCenter,wordWrap=false});
                choose=GUI.Button(GamePhoneView.Place(new Rect(765,563,395,55)),string.IsNullOrEmpty(destination)?"ESCOLHER DESTINO":destination,GamePhoneView.ButtonStyle);if(choose)UiSound.PlayClick();
                var requestRect=GamePhoneView.Place(new Rect(813,769,294,67));string action=status.Contains("CHEGOU")?"EMBARCAR":status.Contains("CHEGANDO")?"AGUARDANDO":"SOLICITAR";
                CleanGui.Round(requestRect,Color.white);GUI.Label(requestRect,action,new GUIStyle(UiTheme.Label(14,Color.black,true)){alignment=TextAnchor.MiddleCenter,wordWrap=false});
                bool request=GUI.Button(requestRect,GUIContent.none,GUIStyle.none);if(request)UiSound.PlayClick();return request;
            }
            finally{GUI.color=previous;}
        }
        static void Fit(GUIStyle style,string text,float width){while(style.fontSize>10&&style.CalcSize(new GUIContent(text)).x>width)style.fontSize--;}
    }
}
