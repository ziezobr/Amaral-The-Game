using UnityEngine;
namespace PontoCego
{
    public static class MapWaypoint
    {
        public static bool Has=>GameSession.Current.waypointValid;
        public static Vector2 Position=>new Vector2(GameSession.Current.waypointX,GameSession.Current.waypointY);
        public static Vector2 FromMap(Vector2 normalized,Rect bounds)=>new Vector2(Mathf.Lerp(bounds.xMin,bounds.xMax,Mathf.Clamp01(normalized.x)),Mathf.Lerp(bounds.yMax,bounds.yMin,Mathf.Clamp01(normalized.y)));
        public static void Set(Vector2 at){var world=WorldHub.Instance;if(!world)return;at=QuestPlacement2D.FindClear(at);GameSession.Current.waypointValid=true;GameSession.Current.waypointX=at.x;GameSession.Current.waypointY=at.y;GameSession.Save();}
        public static void Clear(){GameSession.Current.waypointValid=false;GameSession.Save();}
    }
}
