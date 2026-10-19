using UnityEngine;
namespace PontoCego
{
    /// <summary>Accessible 2D interface palette. All information keeps a text label as well as a colour.</summary>
    public static class UiTheme
    {
        public static float Night=>WorldEnvironment.DayNightCycle.Instance?WorldEnvironment.DayNightCycle.Instance.Night:0;
        public static Color Surface=>Color.Lerp(new Color(.09f,.12f,.15f,.98f),new Color(.025f,.04f,.065f,.98f),Night);
        public static Color Panel=>Color.Lerp(new Color(.15f,.19f,.22f),new Color(.065f,.09f,.13f),Night);
        public static Color Text=>Color.Lerp(new Color(.97f,.97f,.94f),new Color(.82f,.88f,.94f),Night);
        public static Color Muted=>Color.Lerp(new Color(.75f,.79f,.80f),new Color(.62f,.7f,.79f),Night);
        public static Color Accent=>GamePreferences.ColorMode==3?new Color(1,.52f,.51f):GamePreferences.ColorMode>0?new Color(1,.77f,.30f):new Color(1,.77f,.31f);
        public static Color Life=>LifeForMode(GamePreferences.ColorMode);
        public static Color Route=>GamePreferences.ColorMode==3?new Color(1,.52f,.51f):new Color(1,.82f,.30f);
        public static Color Destination=>GamePreferences.ColorMode==3?new Color(.22f,.9f,.78f):new Color(.35f,.82f,1f);
        public static Color Stamina=>StaminaForMode(GamePreferences.ColorMode);
        public static Color Money=>GamePreferences.ColorMode>0?new Color(.48f,.83f,.98f):new Color(.38f,.76f,.28f);
        public static Color LifeForMode(int mode)=>mode==3?new Color(.96f,.34f,.5f):mode>0?new Color(.3f,.7f,1f):new Color(.86f,.13f,.2f);
        public static Color StaminaForMode(int mode)=>mode==3?new Color(.22f,.9f,.78f):new Color(1,.76f,.12f);
        public static int Font(int basis)=>Mathf.RoundToInt(basis*Mathf.Clamp(GamePreferences.TextScale,.9f,1.3f));
        public static GUIStyle Label(int size=17,Color? color=null,bool bold=false)=>new GUIStyle(GUI.skin.label){fontSize=Font(size),wordWrap=true,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,normal={textColor=color??Text}};
        public static float MenuEase(float value)=>1-Mathf.Pow(1-Mathf.Clamp01(value),3);
        public static float StepVisibility(float current,bool open,float delta,bool reduceMotion)=>reduceMotion?(open?1:0):Mathf.MoveTowards(current,open?1:0,Mathf.Max(0,delta)/.2f);
        public static float HudTarget(bool smart,bool danger,bool recentlyUsed,bool lowHealth)=>!smart||danger||recentlyUsed||lowHealth?1:.48f;
        public static Matrix4x4 SafeMatrix()
        {
            var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1280f,safe.height/720f);
            return Matrix4x4.TRS(new Vector3(safe.x+(safe.width-1280*scale)*.5f,Screen.height-safe.yMax+(safe.height-720*scale)*.5f),Quaternion.identity,Vector3.one*scale);
        }
        // Native antialiased shapes stay crisp at different screen sizes; no font-dependent emoji.
        public static void DrawTimeIcon(Rect r,bool night)
        {
            if(night){CleanGui.Round(new Rect(r.x+2,r.y+2,r.width-4,r.height-4),new Color(.75f,.85f,1));CleanGui.Round(new Rect(r.x+r.width*.42f,r.y-1,r.width*.68f,r.height*.68f),Surface);return;}
            CleanGui.Round(new Rect(r.x+r.width*.28f,r.y+r.height*.28f,r.width*.44f,r.height*.44f),Accent);
            for(int i=0;i<8;i++){float angle=i*Mathf.PI/4;Vector2 a=r.center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r.width*.35f,b=r.center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r.width*.48f;Line(a,b,1.3f,Accent);}
        }
        public static string WeatherLabel(WeatherKind kind)=>kind==WeatherKind.Storm?"TEMPESTADE":kind==WeatherKind.Rain?"CHUVA":kind==WeatherKind.Cloudy?"NUBLADO":"CÉU LIMPO";
        public static void DrawWeatherIcon(Rect r,WeatherKind kind)
        {
            if(kind==WeatherKind.Clear){DrawTimeIcon(r,false);return;}
            Color cloud=kind==WeatherKind.Storm?new Color(.63f,.71f,.84f):new Color(.78f,.84f,.9f);
            CleanGui.Round(new Rect(r.x,r.y+r.height*.34f,r.width,r.height*.37f),cloud);CleanGui.Round(new Rect(r.x+r.width*.2f,r.y+r.height*.08f,r.width*.56f,r.height*.62f),cloud);
            if(kind==WeatherKind.Rain||kind==WeatherKind.Storm)for(int i=0;i<3;i++)Line(new Vector2(r.x+r.width*(.25f+i*.25f),r.y+r.height*.76f),new Vector2(r.x+r.width*(.17f+i*.25f),r.yMax),1.3f,kind==WeatherKind.Storm?Accent:StaminaForMode(3));
        }
        static void Line(Vector2 a,Vector2 b,float width,Color color)
        {
            Matrix4x4 matrix=GUI.matrix;Color previous=GUI.color;try{GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);GUI.color=previous*color;GUI.DrawTexture(new Rect(a.x,a.y-width*.5f,Vector2.Distance(a,b),width),Texture2D.whiteTexture);}finally{GUI.matrix=matrix;GUI.color=previous;}
        }
    }
}
