using UnityEngine;
namespace PontoCego
{
    /// <summary>A sign painted onto scenery follows its sprite fade and culling, never floating after a roof opens.</summary>
    [DefaultExecutionOrder(300)]
    public sealed class GroundedSpriteLabel:MonoBehaviour
    {
        public SpriteRenderer source;public TextMesh caption;Color original;
        void Start(){if(caption)original=caption.color;}
        void LateUpdate()
        {
            if(!source||!caption)return;var color=original;color.a*=source.color.a;caption.color=color;
            caption.GetComponent<MeshRenderer>().enabled=source.enabled&&!source.forceRenderingOff&&color.a>.02f;
        }
    }
}
