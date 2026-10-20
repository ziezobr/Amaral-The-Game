using UnityEngine;
namespace PontoCego
{
    /// <summary>Ground-only placement: never hide quest items inside closed scenery footprints.</summary>
    public static class QuestPlacement2D
    {
        public static bool Clear(Vector2 point)
        {
            var world=WorldHub.Instance;if(world&&!world.bounds.Contains(point))return false;
            if(Physics2D.OverlapCircle(point,.6f,LayerMask.GetMask("World")))return false;
            foreach(var footprint in Object.FindObjectsByType<SceneryFootprint2D>())if(footprint.groundBounds.Contains(point))return false;
            return true;
        }
        public static Vector2 FindClear(Vector2 wanted)
        {
            Physics2D.SyncTransforms();if(Clear(wanted))return wanted;
            for(int ring=1;ring<=24;ring++)for(int i=0;i<32;i++)
            {float angle=i*Mathf.PI/16;var candidate=wanted+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*ring*.5f;if(Clear(candidate))return candidate;}
            Debug.LogError("No safe quest position near "+wanted);return wanted;
        }
    }
}
