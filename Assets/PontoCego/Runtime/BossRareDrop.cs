using UnityEngine;
namespace PontoCego
{
    /// <summary>Legacy serialized component retained for save/scene compatibility; bosses no longer drop food.</summary>
    public sealed class BossRareDrop:MonoBehaviour
    {
        public Sprite[] foodSprites;public int Hits=>0;public bool Dropped=>false;
    }
}
