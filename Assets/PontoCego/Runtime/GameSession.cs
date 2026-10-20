using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    [Serializable]public sealed class CampaignSave
    {
        public int difficulty=1,difficultySchema=1;
        public bool waypointValid;public float waypointX,waypointY;
        public string checkpointScene="01_Riberal";
        public int checkpointStep,armorLevel,flashLevel,allyLevel,completedStages,staminaLevel;
        public int currency=50;
        public int medkits=2;public bool pcAssembled;public int motherboardBombs;
        public bool openNeighborhood,deliveryActive,photoJobActive,sideMission,hasSidearm;
        public string mainMission="01_Riberal";
        public int worldSchema=2;public string worldMission="",worldDestination="";public int worldStep;public float worldX,worldY;public bool worldPositionValid;
        public int flashCharges=12,magazine=8,ammoReserve=24,allyAmmo=48,weaponLevel;
        public float playedSeconds;
        public List<string> collected=new List<string>();
        public List<string> journal=new List<string>();
    }
    /// <summary>Versioned local save. Campaign and quick-demo slots are independent; controls are untouched.</summary>
    public static class GameSession
    {
        static CampaignSave data;static bool demo;
        static string Key=>demo?"PontoCego.Demo.v1":"PontoCego.Campaign.v1";
        public static bool Exists=>data!=null;
        public static bool CanPersist=>Array.IndexOf(Environment.GetCommandLineArgs(),"--ponto-world-benchmark")<0&&Array.IndexOf(Environment.GetCommandLineArgs(),"--ponto-world-capture")<0&&Array.IndexOf(Environment.GetCommandLineArgs(),"--ponto-mocambo-capture")<0&&!Application.isBatchMode&&Array.IndexOf(Environment.GetCommandLineArgs(),"--ponto-capture")<0&&Array.IndexOf(Environment.GetCommandLineArgs(),"--ponto-boss-capture")<0;
        public static CampaignSave Current{get{if(data==null)Load();return data;}}
        public static bool HasSave=>PlayerPrefs.HasKey("PontoCego.Campaign.v1");
        static void Load()
        {
            try{data=CanPersist&&PlayerPrefs.HasKey(Key)?JsonUtility.FromJson<CampaignSave>(PlayerPrefs.GetString(Key)):new CampaignSave();}
            catch{data=new CampaignSave();}if(data==null)data=new CampaignSave();if(data.collected==null)data.collected=new List<string>();if(data.journal==null)data.journal=new List<string>();if(data.difficultySchema==0){data.difficulty=1;data.difficultySchema=1;}if(data.worldMission==null)data.worldMission="";if(data.worldDestination==null)data.worldDestination="";if(string.IsNullOrEmpty(data.mainMission))data.mainMission="01_Riberal";
        }
        public static void NewCampaign(bool quickDemo=false,int difficulty=1)
        {
            if(WorldEnvironment.DayNightCycle.Instance)WorldEnvironment.DayNightCycle.Instance.SetHour(16);demo=quickDemo;data=new CampaignSave{openNeighborhood=!quickDemo,difficulty=Mathf.Clamp(difficulty,0,2)};if(quickDemo){data.checkpointScene="04_Emboscada";data.currency=180;data.medkits=3;}if(!quickDemo){data.checkpointScene=WorldHub.SceneName;Save();SceneManager.LoadScene(WorldHub.SceneName);}else{Save();Travel(data.checkpointScene);}
        }
        public static void StartSelectedMission(string id,int difficulty=1)
        {
            demo=true;data=new CampaignSave{difficulty=Mathf.Clamp(difficulty,0,2),checkpointScene=WorldHub.SceneName,openNeighborhood=true,mainMission=id,worldMission=id,worldStep=0,currency=180,hasSidearm=id!="01_Riberal"&&id!="02_Gala"};Time.timeScale=1;SceneManager.LoadScene(WorldHub.SceneName);
        }
        public static void Continue(){demo=false;data=null;Load();Time.timeScale=1;
            if(Current.checkpointScene!="00_Menu"){Current.openNeighborhood=true;if(Current.checkpointScene!="25_Bairro"&&Current.checkpointScene!=WorldHub.SceneName){Current.worldMission=Current.checkpointScene;Current.worldStep=Current.checkpointStep;}Current.checkpointScene=WorldHub.SceneName;}
            SceneManager.LoadScene(Current.checkpointScene);}
        public static void EncounterDemo(string scene,int step)
        {
            demo=true;data=new CampaignSave{hasSidearm=true,checkpointScene=scene,checkpointStep=step,currency=220,medkits=4,armorLevel=1,flashLevel=1,allyLevel=1};Save();Time.timeScale=1;
            void Prepare(Scene loaded,LoadSceneMode mode)
            {
                if(loaded.name!=scene)return;SceneManager.sceneLoaded-=Prepare;var level=UnityEngine.Object.FindAnyObjectByType<LevelProgress>();level.RestoreStep(step);if(step==0)return;
                var player=UnityEngine.Object.FindAnyObjectByType<PlayerMotor>();var target=level.CurrentTarget;if(!player||!target)return;Physics2D.SyncTransforms();
                var mission=level.GetComponent<CombatMission>();
                for(int i=0;i<24;i++)
                {
                    float angle=Mathf.PI+i*Mathf.PI/12;Vector2 at=(Vector2)target.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*3.8f;
                    if(Physics2D.OverlapCircle(at,.35f,LayerMask.GetMask("World")))continue;bool crowded=false;
                    if(mission&&step<mission.rules.Length&&mission.rules[step].enemies!=null)foreach(var enemy in mission.rules[step].enemies)if(enemy&&Vector2.Distance(at,enemy.transform.position)<2.1f)crowded=true;
                    if(crowded)continue;player.Body.position=at;break;
                }
            }
            SceneManager.sceneLoaded+=Prepare;SceneManager.LoadScene(scene);
        }
        public static void Save(){if(data==null||!CanPersist)return;PlayerPrefs.SetString(Key,JsonUtility.ToJson(data));PlayerPrefs.Save();}
        public static void Checkpoint(LevelProgress level)
        {
            if(WorldHub.Instance){WorldHub.Instance.SaveCheckpoint(level);return;}
            Current.checkpointScene=SceneManager.GetActiveScene().name;Current.checkpointStep=level.Step;Save();
        }
        public static bool RewardOnce(string id,int cash)
        {
            if(Current.collected.Contains(id))return false;Current.collected.Add(id);Current.currency+=cash;return true;
        }
        public static void AddEvidence(string entry){if(!Current.journal.Contains(entry)){Current.journal.Add(entry);Save();}}
        public static void ExploreNeighborhood(string destination="04_Emboscada")
        {
            demo=true;data=new CampaignSave{checkpointScene=WorldHub.SceneName,openNeighborhood=true,currency=150,mainMission=destination};Time.timeScale=1;Save();SceneManager.LoadScene(WorldHub.SceneName);
        }
        public static void EnterMission(string scene)
        {
            if(WorldHub.Instance){WorldHub.Instance.Navigate(scene);return;}
            Current.sideMission=scene=="24_Mocambo";Current.checkpointScene=scene;Current.checkpointStep=0;Save();Time.timeScale=1;SceneManager.LoadScene(scene);
        }
        public static void Travel(string scene)
        {
            if(WorldHub.Instance){WorldHub.Instance.Complete(scene);return;}
            if(Current.openNeighborhood&&scene!=WorldHub.SceneName&&scene!="00_Menu"&&scene!="25_Bairro")
            {
                if(!Current.sideMission)Current.mainMission=scene;
                else RewardOnce("bairro:mocambo",250);
                Current.sideMission=false;scene=WorldHub.SceneName;
            }
            Current.checkpointScene=scene;Current.checkpointStep=0;Save();Time.timeScale=1;SceneManager.LoadScene(scene);
        }
        public static void Retry(){Time.timeScale=1;SceneManager.LoadScene(Current.checkpointScene);}
        public static void ReturnToMenu(){if(WorldHub.Instance)WorldHub.Instance.SavePosition();Save();Time.timeScale=1;SceneManager.LoadScene("00_Menu");}
    }
}
