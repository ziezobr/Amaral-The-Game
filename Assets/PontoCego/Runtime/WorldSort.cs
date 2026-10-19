using UnityEngine;
namespace PontoCego
{
    /// <summary>Global north/south coordinates must never push actors behind the map background.</summary>
    public static class WorldSort
    {
        public static int At(float y)=>(WorldHub.Instance?20000:1000)-Mathf.RoundToInt(y*100);
    }
}
