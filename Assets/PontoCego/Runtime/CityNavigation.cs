using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Shared pedestrian/GPS guidance. Mission targets update as each objective completes.</summary>
    public sealed class CityNavigation:MonoBehaviour
    {
        public Vector2[] Route{get;private set;}=System.Array.Empty<Vector2>();public string Objective{get;private set;}="";public float Distance{get;private set;}
        public bool FollowMateus;
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
            var savedColor=GUI.color;var clip=area;GUI.BeginGroup(clip);area=new Rect(0,0,clip.width,clip.height);try{
            var world=WorldHub.Instance;if(!world)return;GUI.color=savedColor*new Color(.18f,.23f,.15f);GUI.DrawTexture(area,Texture2D.whiteTexture);GUI.color=savedColor;
            var view=world.bounds;if(local&&player){Vector2 size=new Vector2(54,54*area.height/area.width);Vector2 center=player.transform.position;center.x=Mathf.Clamp(center.x,world.bounds.xMin+size.x*.5f,world.bounds.xMax-size.x*.5f);center.y=Mathf.Clamp(center.y,world.bounds.yMin+size.y*.5f,world.bounds.yMax-size.y*.5f);view=new Rect(center-size*.5f,size);}
            Vector2 Map(Vector2 p){var uv=new Vector2((p.x-view.xMin)/view.width,(view.yMax-p.y)/view.height);return area.position+Vector2.Scale(uv,area.size);}
            for(int i=0;i<world.roadStarts.Length;i++)Line(Map(world.roadStarts[i]),Map(world.roadEnds[i]),2,new Color(.55f,.56f,.51f));
            if(world.streetBlocks!=null)foreach(var block in world.streetBlocks){var at=Map(new Vector2(block.xMin,block.yMax));GUI.color=savedColor*new Color(.61f,.45f,.28f);GUI.DrawTexture(new Rect(at,Vector2.Scale(block.size/view.size,area.size)),Texture2D.whiteTexture);}GUI.color=savedColor;
            for(int i=0;i<world.districtRects.Length;i++){if(world.rebuiltOutdoorLayout&&(i==0||world.districtMaps[i].name.Contains("mocambo")||world.districtMaps[i].name.Contains("scrap")||world.districtMaps[i].name.Contains("road")))continue;var r=world.districtRects[i];var at=Map(new Vector2(r.xMin,r.yMax));GUI.DrawTexture(new Rect(at,Vector2.Scale(r.size/view.size,area.size)),world.districtMaps[i]);}
            if(world.landmarkPatches!=null&&world.landmarkMaps!=null)for(int i=0;i<Mathf.Min(world.landmarkPatches.Length,world.landmarkMaps.Length);i++){var r=world.landmarkPatches[i];var sprite=world.landmarkMaps[i];var tex=sprite.texture;var uv=sprite.rect;GUI.DrawTextureWithTexCoords(new Rect(Map(new Vector2(r.xMin,r.yMax)),Vector2.Scale(r.size/view.size,area.size)),tex,new Rect(uv.x/tex.width,uv.y/tex.height,uv.width/tex.width,uv.height/tex.height));}
            for(int i=1;i<Route.Length;i++)Line(Map(Route[i-1]),Map(Route[i]),2,UiTheme.Route);
            if(player){var at=Map(player.transform.position);GUI.color=savedColor;GUI.DrawTexture(new Rect(at.x-3,at.y-3,6,6),Texture2D.whiteTexture);GUI.color=savedColor;}

            }finally{GUI.color=savedColor;GUI.EndGroup();}
        }
    }
}
