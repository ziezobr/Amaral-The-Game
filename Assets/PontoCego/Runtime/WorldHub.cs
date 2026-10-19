using System;
using UnityEngine;
using Unity.Cinemachine;
namespace PontoCego
{
    /// <summary>One loaded world, player and camera. Mission activation swaps quest actors, never scenes.</summary>
    public sealed class WorldHub:MonoBehaviour
    {
        public const string SceneName="26_PatosWorld";
        public static WorldHub Instance{get;private set;}
        public CityMission[] missions;public LevelProgress exploration;public GameHud hud;
        public Rect bounds=new Rect(-110,-102,330,210);
        public Texture2D[] districtMaps;public Rect[] districtRects;
        public Rect[] streetBlocks;
        public Rect[] landmarkPatches;public Sprite[] landmarkMaps;
        public bool rebuiltOutdoorLayout;
        public Vector2[] roadStarts,roadEnds;
        public CityMission Active{get;private set;}
        public string Destination{get;private set;}
        public bool Transitioning{get;private set;}
        PlayerMotor player;PlayerInputReader input;GridNavigator navigation;float saveAt;
        bool requireExit;Vector2 completedGate;
        string pendingMission;
        public void RequestBegin(string id){pendingMission=id;}
        void Awake(){Instance=this;for(int layer=0;layer<32;layer++)Physics2D.IgnoreLayerCollision(11,layer,true);var confiner=FindAnyObjectByType<CinemachineConfiner2D>();if(confiner&&confiner.BoundingShape2D)confiner.BoundingShape2D.isTrigger=true;}
        void Start()
        {
            player=FindAnyObjectByType<PlayerMotor>();input=player.GetComponent<PlayerInputReader>();navigation=FindAnyObjectByType<GridNavigator>();
            foreach(var m in missions){m.actors.SetActive(false);m.template=m.actors;m.templateLevel=m.level;}
            Destination=string.IsNullOrEmpty(GameSession.Current.worldDestination)?GameSession.Current.mainMission:GameSession.Current.worldDestination;
            BindExploration();
            var save=GameSession.Current;
            if(save.worldPositionValid&&IsWalkable(new Vector2(save.worldX,save.worldY)))SetPosition(new Vector2(save.worldX,save.worldY));
            if(!string.IsNullOrEmpty(save.worldMission))
            {
                var mission=Find(save.worldMission);if(mission!=null)Activate(mission,save.worldStep,true);
            }
            if(save.collected.Contains("story:adryan-rescued")&&!FindAnyObjectByType<CivilianFollower>())
            {var rescue=Find("21_Adryan")?.template.GetComponentInChildren<GameplaySequence>(true);if(rescue&&rescue.actor){var actor=Instantiate(rescue.actor.gameObject,player.transform.position+Vector3.right,Quaternion.identity);actor.name="Adryan resgatado / acompanhante";actor.SetActive(true);actor.layer=13;if(!actor.GetComponent<CivilianFollower>())actor.AddComponent<CivilianFollower>();}}
            RefreshTriggers();
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
        public CityMission Find(string id)=>Array.Find(missions,m=>m.id==id);
        public bool IsWalkable(Vector2 point)=>bounds.Contains(point)&&!Physics2D.OverlapCircle(point,.32f,LayerMask.GetMask("World"));
        public bool TryBegin(string id)
        {
            if(TaxiService.Instance&&TaxiService.Instance.TripActive)return false;
            if(Transitioning||Active!=null||!player||input.Paused||input.GameplayLocked)return false;
            if(requireExit){if(Vector2.Distance(player.transform.position,completedGate)<2)return false;requireExit=false;}
            var mission=Find(id);if(mission==null||!mission.trigger.gameObject.activeSelf||Vector2.Distance(player.transform.position,mission.entry)>mission.trigger.radius+.35f)return false;
            if(id=="20_Paulo"&&!GameSession.Current.pcAssembled){FindAnyObjectByType<Interaction>()?.Say("Antes de Paulo, reúna as seis peças e monte o PC com Henzo no bairro.");return false;}
            if(DriveableCar.Current){DriveableCar.Current.Exit();return false;}
            Activate(mission,0,false);return true;
        }
        void Activate(CityMission mission,int step,bool resume)
        {
            Transitioning=true;Active=mission;exploration.gameObject.SetActive(false);
            mission.actors=Instantiate(mission.template);mission.actors.name="Atores da missão ativa / "+mission.id;
            mission.level=mission.actors.GetComponentInChildren<LevelProgress>(true);
            mission.level.RestoreStep(step);mission.actors.SetActive(true);hud.SetLevel(mission.level);
            navigation.SetArea(mission.center,mission.size+Vector2.one*8);
            if(resume&&!GameSession.Current.worldPositionValid)SetPosition(mission.entry);
            if(mission.road){var road=player.gameObject.AddComponent<RoadDriver>();road.ridingSprites=GetComponent<WorldRoadAssets>().riding;road.worldCenter=mission.center;road.cruiseSpeed=3;player.gameObject.AddComponent<RoadChase>();}
            RefreshTriggers();Transitioning=false;SaveCheckpoint(mission.level);
        }
        public void Complete(string next)
        {
            if(Active==null||Transitioning)return;Transitioning=true;
            if(player.TryGetComponent<RoadDriver>(out var road)){road.RestoreWalking();Destroy(road);}
            if(Active.id=="21_Adryan"){foreach(var follower in Active.actors.GetComponentsInChildren<CivilianFollower>(true))follower.transform.SetParent(null,true);if(!GameSession.Current.collected.Contains("story:adryan-rescued"))GameSession.Current.collected.Add("story:adryan-rescued");}
            bool side=Active.id=="24_Mocambo";completedGate=Active.entry;requireExit=true;
            Active.actors.SetActive(false);Destroy(Active.actors);Active.actors=Active.template;Active.level=Active.templateLevel;Active=null;
            foreach(var projectile in FindObjectsByType<CombatProjectile>()){projectile.gameObject.SetActive(false);Destroy(projectile.gameObject);}
            foreach(var ball in FindObjectsByType<BasketballShot>()){ball.gameObject.SetActive(false);Destroy(ball.gameObject);}
            foreach(var warning in FindObjectsByType<GroundWarning>()){warning.gameObject.SetActive(false);Destroy(warning.gameObject);}
            var save=GameSession.Current;if(side)GameSession.RewardOnce("bairro:mocambo",250);else if(!string.IsNullOrEmpty(next))save.mainMission=next;
            save.worldMission="";save.worldStep=0;save.sideMission=false;Destination=save.mainMission;save.worldDestination=Destination;
            BindExploration();RefreshTriggers();Transitioning=false;SavePosition();
            player.GetComponent<Interaction>().Say("Missão concluída. Vá pelo mapa até o próximo ponto marcado.");
        }
        void BindExploration(){exploration.gameObject.SetActive(true);hud.SetLevel(exploration);navigation.SetArea(Vector2.zero,new Vector2(64,128f/3));UpdateObjective();}
        public bool Navigate(string id)
        {
            if(id=="25_Bairro"||id==SceneName){Destination="";GameSession.Current.worldDestination="";return true;}
            var mission=Find(id);if(mission==null)return false;
            if(Active!=null){player.GetComponent<Interaction>().Say("Conclua a missão atual antes de marcar outro destino.");return false;}
            Destination=id;GameSession.Current.worldDestination=id;RefreshTriggers();UpdateObjective();GameSession.Save();
            player.GetComponent<Interaction>().Say("Rota marcada. Vá a pé ou de carro até "+mission.level.title+".");return true;
        }
        void RefreshTriggers()
        {
            foreach(var m in missions)m.trigger.gameObject.SetActive(Active==null&&(m.id==(string.IsNullOrEmpty(Destination)?GameSession.Current.mainMission:Destination)||m.id=="24_Mocambo"));
        }
        void UpdateObjective()
        {
            if(Active!=null)return;var target=Find(Destination);if(target==null)target=Find(GameSession.Current.mainMission);
            exploration.objectives=new[]{target==null?"Explore Patos de Minas. Procure o Henzo ou escolha um destino no Uber.":"Vá até "+target.level.title+". A missão começa ao alcançar a marca no chão."};
            exploration.objectiveTargets=target==null?Array.Empty<Transform>():new[]{target.trigger.transform};
        }
        void Update()
        {
            if(!player||input.Paused||input.GameplayLocked)return;
            // Physics callbacks only queue intent; actor activation and navigation rebuild happen outside simulation.
            if(pendingMission!=null){string id=pendingMission;pendingMission=null;TryBegin(id);}
            if(requireExit&&Vector2.Distance(player.transform.position,completedGate)>=2)requireExit=false;
            if(Time.unscaledTime>saveAt){saveAt=Time.unscaledTime+5;SavePosition();}
        }
        public void SaveCheckpoint(LevelProgress level)
        {
            var save=GameSession.Current;save.checkpointScene=SceneName;save.checkpointStep=0;
            save.worldMission=Active==null?"":Active.id;save.worldStep=Active==null?0:level.Step;SavePosition();
        }
        public void SavePosition()
        {
            if(!player)return;var save=GameSession.Current;save.checkpointScene=SceneName;
            Vector2 position=player.transform.position;save.worldX=position.x;save.worldY=position.y;save.worldPositionValid=true;GameSession.Save();
        }
        void OnApplicationPause(bool paused){if(paused)SavePosition();}
        void SetPosition(Vector2 at){player.Body.position=at;player.transform.position=at;player.Body.linearVelocity=Vector2.zero;}
        public Vector2 MapUV(Vector2 point)=>new Vector2((point.x-bounds.xMin)/bounds.width,1-(point.y-bounds.yMin)/bounds.height);
    }
}
