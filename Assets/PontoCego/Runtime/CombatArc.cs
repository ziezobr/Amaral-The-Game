using UnityEngine;
namespace PontoCego
{
    public sealed class CombatArc:MonoBehaviour
    {
        LineRenderer line;float remaining=.16f;Material material;
        public static void Show(Vector3 position,Vector2 direction,bool finisher)
        {
            var root=new GameObject("Rastro do golpe / profundidade 2D");root.transform.position=position;root.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            var effect=root.AddComponent<CombatArc>();effect.line=root.AddComponent<LineRenderer>();effect.material=new Material(Shader.Find("Sprites/Default"));effect.line.sharedMaterial=effect.material;
            effect.line.useWorldSpace=false;effect.line.positionCount=20;effect.line.startWidth=finisher?.12f:.075f;effect.line.endWidth=.01f;effect.line.sortingOrder=4500;
            for(int i=0;i<20;i++){float a=Mathf.Lerp(-.95f,.95f,i/19f);effect.line.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a))*(finisher?1.35f:1.1f));}
        }
        void Update(){remaining-=Time.deltaTime;line.startColor=line.endColor=new Color(1,.82f,.4f,Mathf.Clamp01(remaining/.16f));if(remaining<=0)Destroy(gameObject);}
        void OnDestroy(){if(material)Destroy(material);}
    }
}
