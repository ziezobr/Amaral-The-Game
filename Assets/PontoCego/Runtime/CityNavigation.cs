using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Shared pedestrian/GPS guidance. Mission targets update as each objective completes.</summary>
    public sealed class CityNavigation:MonoBehaviour
    {
        public Vector2[] Route{get;private set;}=System.Array.Empty<Vector2>();public string Objective{get;private set;}="";public float Distance{get;private set;}
        public bool FollowMateus;public bool FollowWaypoint;public bool FollowContract;
        float next;PlayerMotor player;Vector2 previousGoal,lastStart;bool haveRoute;
        void Update()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+1;var world=WorldHub.Instance;if(!world)return;if(!player)player=FindAnyObjectByType<PlayerMotor>();if(!player)return;
            var level=world.Active!=null?world.Active.level:world.exploration;Objective=level.Objective;Vector2 goal=level.CurrentTarget?level.CurrentTarget.position:player.transform.position;
            if(FollowMateus&&world.Active==null){if(GoldenWeaponQuest.Glock&&GoldenWeaponQuest.Razor)FollowMateus=false;else{goal=GoldenWeaponQuest.NavigationTarget;Objective=GoldenWeaponQuest.Status;}}
            if(world.Active==null&&GameSession.Current.mainMission=="20_Paulo"&&!GameSession.Current.pcAssembled&&!FollowMateus)
            {
                PcCollectible nearest=null;float best=float.MaxValue;foreach(var part in FindObjectsByType<PcCollectible>()){if(GameSession.Current.collected.Contains(part.id)||GameSession.Current.collected.Contains("pc:kind:"+part.kind))continue;float distance=Vector2.Distance(player.transform.position,part.transform.position);if(distance<best){best=distance;nearest=part;}}
                if(nearest){goal=nearest.transform.position;Objective="Preparação final: recolha "+PcCollectible.Names[nearest.kind]+" ("+PcCollectible.TypesCollected+"/6).";}
                else{var table=FindAnyObjectByType<PcWorkbench>();if(table)goal=table.transform.position;Objective="Reúna o grupo na bancada de Henzo e monte o PC.";}
            }
            if(FollowWaypoint&&MapWaypoint.Has&&world.Active==null){goal=MapWaypoint.Position;Objective="Vá ao WAYPOINT marcado no mapa.";}
            if(FollowContract&&!CityContracts.Started)FollowContract=false;
            if(FollowContract&&world.Active==null){var contracts=world.GetComponent<CityContracts>();if(contracts&&contracts.Target){goal=contracts.Target.position;Objective=CityContracts.Objective;}}
            if(haveRoute&&world.Active==null&&Vector2.Distance(goal,previousGoal)<.1f&&Vector2.Distance(player.transform.position,lastStart)<5)return;previousGoal=goal;lastStart=player.transform.position;haveRoute=true;
            var points=new List<Vector2>{player.transform.position};
            if(world.Active!=null){var nav=FindAnyObjectByType<GridNavigator>();var path=nav?nav.Path(player.transform.position,goal):null;if(path!=null)points.AddRange(path);}
            else{var streets=TaxiCollisionRoute.Plan(player.transform.position,goal,player.Body,.35f,.8f);if(streets!=null)points.AddRange(streets);}
            points.Add(goal);Route=points.ToArray();Distance=0;for(int i=1;i<Route.Length;i++)Distance+=Vector2.Distance(Route[i-1],Route[i]);
        }
        void OnGUI()
        {
            if(!GamePreferences.Objectives)return;var world=WorldHub.Instance;if(!world||world.Active==null||!player)return;var input=player.GetComponent<PlayerInputReader>();var target=world.Active.level.CurrentTarget;if(input.GameplayLocked||input.Paused||!target||Vector2.Distance(player.transform.position,target.position)>12)return;
            string hint=target.GetComponent<PhotoTarget>()?"FOTOGRAFAR • ":target.GetComponent<IInteractable>()!=null?"E • ":"OBJETIVO • ";CollectibleCover.Hint(target.position,hint+world.Active.level.Objective);
        }
        public static void Line(Vector2 from,Vector2 to,float width,Color color)
        {var old=GUI.matrix;var oldColor=GUI.color;GUI.color=oldColor*color;Vector2 delta=to-from;GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,from);GUI.DrawTexture(new Rect(from.x,from.y-width*.5f,delta.magnitude,width),Texture2D.whiteTexture);GUI.matrix=old;GUI.color=oldColor;}
        public void DrawMap(Rect area,bool local=false)
        {
            var world=WorldHub.Instance;if(!world)return;
            var radar=GetComponent<CityRadar2D>();if(!radar)radar=gameObject.AddComponent<CityRadar2D>();
            radar.Draw(area,local,world,player,Route);
        }
    }
}