using UnityEngine;
namespace PontoCego
{
    /// <summary>Short sprite-free combat accents; no time-scale changes, colliders or permanent materials.</summary>
    public sealed class CombatFeedback2D:MonoBehaviour
    {
        static Material shared;LineRenderer line;Color tint;float left,duration;
        static CombatFeedback2D Create(Vector2 point,Color color,float seconds,int points,float width)
        {
            if(!shared)shared=new Material(Shader.Find("Sprites/Default"));
            var go=new GameObject("Impacto 2D");go.transform.position=point;
            var fx=go.AddComponent<CombatFeedback2D>();fx.line=go.AddComponent<LineRenderer>();fx.line.sharedMaterial=shared;
            fx.line.useWorldSpace=false;fx.line.positionCount=points;fx.line.startWidth=width;fx.line.endWidth=.005f;fx.line.sortingOrder=4501;fx.tint=color;fx.left=fx.duration=seconds;return fx;
        }
        public static void Muzzle(Vector2 point,Vector2 direction)
        {
            var fx=Create(point+direction*.35f,new Color(1,.85f,.4f),.065f,4,.06f);var side=Vector2.Perpendicular(direction);
            fx.line.SetPosition(0,Vector3.zero);fx.line.SetPosition(1,direction*.28f+side*.09f);fx.line.SetPosition(2,direction*.13f);fx.line.SetPosition(3,direction*.28f-side*.09f);
        }
        public static void Impact(Vector2 point,Color color)
        {
            var fx=Create(point,color,.13f,9,.035f);
            for(int i=0;i<9;i++){float a=i*Mathf.PI/4;fx.line.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a))*(i%2==0?.16f:.04f));}
        }
        void Update(){left-=Time.deltaTime;var c=tint;c.a=Mathf.Clamp01(left/duration);line.startColor=line.endColor=c;if(left<=0)Destroy(gameObject);}
    }
}
