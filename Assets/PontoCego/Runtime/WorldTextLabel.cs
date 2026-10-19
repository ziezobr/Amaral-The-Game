using UnityEngine;
namespace PontoCego
{
    /// <summary>Fits the actual generated font mesh to world units, independent of font atlas resolution.</summary>
    [RequireComponent(typeof(TextMesh))]
    public sealed class WorldTextLabel:MonoBehaviour
    {
        public float width=4,height=.45f,visibleDistance;
        Renderer mesh;Transform player;
        void Start(){mesh=GetComponent<Renderer>();player=FindAnyObjectByType<PlayerMotor>()?.transform;Fit();}
        public void Fit()
        {
            var renderer=GetComponent<Renderer>();var bounds=renderer.localBounds.size;
            if(bounds.x>.001f&&bounds.y>.001f)transform.localScale=Vector3.one*Mathf.Min(width/bounds.x,height/bounds.y);
        }
        void LateUpdate(){if(mesh&&visibleDistance>0&&player)mesh.enabled=Vector2.Distance(player.position,transform.position)<visibleDistance;}
    }
}
