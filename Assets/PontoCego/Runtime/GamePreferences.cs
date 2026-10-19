using UnityEngine;
namespace PontoCego
{
    /// <summary>User preferences are independent of campaign/checkpoint saves.</summary>
    public static class GamePreferences
    {
        public static float Master=PlayerPrefs.GetFloat("pc.audio.master",.85f),Music=PlayerPrefs.GetFloat("pc.audio.music",.8f),Effects=PlayerPrefs.GetFloat("pc.audio.effects",1),AimSize=PlayerPrefs.GetFloat("pc.ui.aim",1);
        public static bool Minimap=PlayerPrefs.GetInt("pc.ui.map",1)==1,Objectives=PlayerPrefs.GetInt("pc.ui.objectives",1)==1,EnemyBars=PlayerPrefs.GetInt("pc.ui.enemies",1)==1,Clock=PlayerPrefs.GetInt("pc.ui.clock",1)==1;
        public static bool ReduceMotion=PlayerPrefs.GetInt("pc.ui.reduceMotion",0)==1,SmartHud=PlayerPrefs.GetInt("pc.ui.smartHud",1)==1;
        public static float TextScale=Mathf.Clamp(PlayerPrefs.GetFloat("pc.ui.textScale",1),.9f,1.3f);
        // 0: original, 1: protanopia, 2: deuteranopia, 3: tritanopia. Labels remain in every mode.
        public static int ColorMode=Mathf.Clamp(PlayerPrefs.GetInt("pc.ui.colorMode",0),0,3);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Apply(){AudioListener.volume=Master;QualitySettings.vSyncCount=PlayerPrefs.GetInt("pc.video.vsync",1);Application.targetFrameRate=PlayerPrefs.GetInt("pc.video.fps",60);}
        public static void Save()
        {
            AudioListener.volume=Master;TextScale=Mathf.Clamp(TextScale,.9f,1.3f);ColorMode=Mathf.Clamp(ColorMode,0,3);if(!GameSession.CanPersist)return;
            PlayerPrefs.SetFloat("pc.audio.master",Master);PlayerPrefs.SetFloat("pc.audio.music",Music);PlayerPrefs.SetFloat("pc.audio.effects",Effects);PlayerPrefs.SetFloat("pc.ui.aim",AimSize);PlayerPrefs.SetInt("pc.ui.map",Minimap?1:0);PlayerPrefs.SetInt("pc.ui.objectives",Objectives?1:0);PlayerPrefs.SetInt("pc.ui.enemies",EnemyBars?1:0);PlayerPrefs.SetInt("pc.ui.clock",Clock?1:0);PlayerPrefs.SetInt("pc.video.vsync",QualitySettings.vSyncCount);PlayerPrefs.SetInt("pc.video.fps",Application.targetFrameRate);
            PlayerPrefs.SetFloat("pc.ui.textScale",TextScale);PlayerPrefs.SetInt("pc.ui.colorMode",ColorMode);PlayerPrefs.SetInt("pc.ui.reduceMotion",ReduceMotion?1:0);PlayerPrefs.SetInt("pc.ui.smartHud",SmartHud?1:0);PlayerPrefs.Save();
        }
    }
}
