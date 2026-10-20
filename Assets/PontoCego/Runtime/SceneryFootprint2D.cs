using UnityEngine;
namespace PontoCego
{
    /// <summary>Authored ground footprint, independent from the painted roof and its height.</summary>
    public sealed class SceneryFootprint2D:MonoBehaviour
    {
        public Rect groundBounds;public Vector2[] entrances;public float[] entranceWidths;
    }
}
