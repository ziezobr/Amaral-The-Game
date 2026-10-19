using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego.WorldEnvironment
{
    /// <summary>One world clock for every gameplay scene; changing missions never resets the hour.</summary>
    [DefaultExecutionOrder(-500)]public sealed class DayNightCycle:MonoBehaviour
    {
        public static DayNightCycle Instance{get;private set;}
        [Range(0,24)]public float hour=16;[Min(120)]public float dayLengthSeconds=480;public bool frozen;
        public float Daylight=>DaylightAtHour(hour);
        public static float DaylightAtHour(float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.18f,.24f,Mathf.Sin((value-6)/24*Mathf.PI*2)));
        public float Night=>1-Daylight;
        /// <summary>Direction toward the active sun/moon in the flat world. Used by painted 2D shadows.</summary>
        public Vector2 LightDirection2D=>DirectionAtHour(hour);
        public static Vector2 DirectionAtHour(float value)
        {
            float angle=(value-6)/24*Mathf.PI*2;
            // Blend the sun/moon contribution through twilight instead of flipping at 50% daylight.
            float side=Mathf.SmoothStep(-1,1,DaylightAtHour(value));
            return new Vector2(-Mathf.Cos(angle)*side,.25f+Mathf.Abs(Mathf.Sin(angle))*.8f).normalized;
        }
        public float ShadowLength2D=>1-Mathf.Abs(Mathf.Sin((hour-6)/24*Mathf.PI*2));
        public string Clock=>Mathf.FloorToInt(hour).ToString("00")+":"+Mathf.FloorToInt(hour%1*60).ToString("00");
        Light sun,moon;Material sky;bool is2D,menu;float nextBind;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Boot(){if(!Instance)new GameObject("Relógio global / dia e noite").AddComponent<DayNightCycle>();}
        void Awake(){if(Instance&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);if(CanSave)hour=PlayerPrefs.GetFloat("PontoCego.WorldHour.v1",16);SceneManager.sceneLoaded+=Loaded;}
        bool CanSave=>!Application.isBatchMode&&Array.IndexOf(System.Environment.GetCommandLineArgs(),"--capture-3d")<0&&Array.IndexOf(System.Environment.GetCommandLineArgs(),"--capture-city")<0&&Array.IndexOf(System.Environment.GetCommandLineArgs(),"--ponto-world-capture")<0&&Array.IndexOf(System.Environment.GetCommandLineArgs(),"--ponto-world-benchmark")<0;
        void Loaded(Scene scene,LoadSceneMode mode)
        {
            menu=scene.name=="00_Menu";is2D=FindObjectsByType<SpriteRenderer>().Length>20;sun=RenderSettings.sun;if(sky)Destroy(sky);sky=RenderSettings.skybox?new Material(RenderSettings.skybox):null;if(sky)RenderSettings.skybox=sky;
            if(!is2D&&!menu){if(!sun){var go=new GameObject("Sol do cenário");sun=go.AddComponent<Light>();sun.type=LightType.Directional;sun.shadows=LightShadows.Soft;RenderSettings.sun=sun;}var m=new GameObject("Luar");moon=m.AddComponent<Light>();moon.type=LightType.Directional;moon.color=new Color(.43f,.57f,.87f);moon.shadows=LightShadows.None;}
            Apply();
        }
        public void SetHour(float value){hour=Mathf.Repeat(value,24);Apply();}
        void Update(){if(menu)return;if(!frozen&&Time.timeScale>0)hour=Mathf.Repeat(hour+Time.deltaTime*24/Mathf.Max(120,dayLengthSeconds),24);Apply();}
        void Apply()
        {
            if(menu||is2D)return;float day=Daylight;float elevation=Mathf.Sin((hour-6)/24*Mathf.PI*2);
            if(sun){sun.transform.rotation=Quaternion.Euler(hour*15-90,-35,0);sun.intensity=Mathf.Lerp(.02f,1.2f,day);sun.color=Color.Lerp(new Color(1,.43f,.22f),new Color(1,.94f,.83f),Mathf.Clamp01(elevation*2));sun.enabled=day>.01f;}
            if(moon){moon.transform.rotation=Quaternion.Euler(55,-120,0);moon.intensity=.3f*(1-day);}
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=Color.Lerp(new Color(.19f,.25f,.39f),new Color(.57f,.66f,.76f),day);RenderSettings.ambientEquatorColor=Color.Lerp(new Color(.15f,.20f,.30f),new Color(.52f,.48f,.40f),day);RenderSettings.ambientGroundColor=Color.Lerp(new Color(.06f,.08f,.13f),new Color(.25f,.28f,.23f),day);
            RenderSettings.fogColor=Color.Lerp(new Color(.055f,.08f,.14f),new Color(.66f,.72f,.75f),day);if(sky&&sky.HasProperty("_SkyTint")){sky.SetColor("_SkyTint",Color.Lerp(new Color(.025f,.04f,.10f),new Color(.46f,.65f,.86f),day));sky.SetFloat("_Exposure",Mathf.Lerp(.25f,1.1f,day));}
        }
        void OnGUI(){if(menu||!is2D||Night<.01f)return;int oldDepth=GUI.depth;GUI.depth=1000;GUI.color=new Color(.035f,.07f,.19f,Night*.48f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;GUI.depth=oldDepth;}
        void OnApplicationPause(bool paused){if(paused&&CanSave)PlayerPrefs.SetFloat("PontoCego.WorldHour.v1",hour);}
        void OnApplicationQuit(){if(CanSave){PlayerPrefs.SetFloat("PontoCego.WorldHour.v1",hour);PlayerPrefs.Save();}}
        void OnDestroy(){if(Instance==this){Instance=null;SceneManager.sceneLoaded-=Loaded;}if(sky)Destroy(sky);}
    }
}
