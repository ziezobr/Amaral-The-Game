using UnityEngine;
namespace PontoCego
{
    /// <summary>Clock-driven painted shadow for a free-standing 2D building or tree. No collision geometry.</summary>
    public sealed class WorldPropShadow2D : MonoBehaviour
    {
        public SpriteRenderer source;
        public Vector2 footLocalOffset;
        [Min(.1f)] public float height=3;
        public int groundSortingOrder=-4998;
        SpriteRenderer projected;
        void Start()
        {
            if(!source)source=GetComponent<SpriteRenderer>();
            if(!source)return;
            var go=new GameObject("Sombra do cenário / dia e noite"); go.transform.SetParent(transform,false);
            projected=go.AddComponent<SpriteRenderer>(); projected.sortingOrder=groundSortingOrder;
        }
        void LateUpdate()
        {
            if(!source||!projected)return;
            if(source.forceRenderingOff){projected.enabled=false;return;}
            var clock=WorldEnvironment.DayNightCycle.Instance;var weather=WorldWeather2D.Instance;
            float daylight=clock?clock.Daylight:1,sun=weather?weather.SunVisibility:1;
            Vector2 direction=clock?-clock.LightDirection2D:new Vector2(.6f,-.8f);
            float length=clock?clock.ShadowLength2D:.3f;
            projected.sprite=source.sprite;projected.flipX=source.flipX;projected.flipY=source.flipY;
            projected.transform.position=transform.TransformPoint(footLocalOffset)+(Vector3)(direction*height*(.08f+length*.13f));
            projected.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
            var sourceScale=source.transform.lossyScale;
            var parentScale=transform.lossyScale;
            projected.transform.localScale=new Vector3(Mathf.Abs(sourceScale.x)/Mathf.Max(.001f,Mathf.Abs(parentScale.x)),Mathf.Abs(sourceScale.y)*(.14f+length*.18f)/Mathf.Max(.001f,Mathf.Abs(parentScale.y)),1);
            projected.color=new Color(.03f,.05f,.085f,Mathf.Lerp(.08f,.21f*sun,daylight)*source.color.a);
            projected.enabled=source.enabled;
        }
    }
}
