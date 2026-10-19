using UnityEngine;
namespace PontoCego
{
    public sealed class NpcIdle:MonoBehaviour
    {
        public SpriteRenderer visual;Vector3 origin;
        void Awake(){origin=visual.transform.localPosition;}
        void LateUpdate()
        {
            visual.transform.localPosition=origin+Vector3.up*(Mathf.Sin(Time.time*1.7f+transform.position.x)*.013f);
            visual.sortingOrder=WorldSort.At(transform.position.y);
        }
    }
}
