using UnityEngine;
namespace PontoCego
{
    /// <summary>Four focus corners replace the circular aim marker; green means the same checks as the shutter pass.</summary>
    public sealed class CameraViewfinder:MonoBehaviour
    {
        PlayerAim aim;PlayerInputReader input;LineRenderer[] corners;Material material;Transform frame;float visibleUntil;
        void Start()
        {
            aim=GetComponent<PlayerAim>();input=GetComponent<PlayerInputReader>();aim.reticle.GetComponent<SpriteRenderer>().enabled=false;
            frame=new GameObject("Visor fotográfico / foco").transform;frame.SetParent(transform,false);corners=new LineRenderer[4];material=new Material(Shader.Find("Sprites/Default"));
            for(int i=0;i<4;i++){var go=new GameObject("Canto do enquadramento");go.transform.SetParent(frame,false);var l=go.AddComponent<LineRenderer>();l.sharedMaterial=material;l.useWorldSpace=false;l.positionCount=3;l.startWidth=l.endWidth=.025f;l.sortingOrder=30000;float x=i%2==0?-.6f:.6f,y=i<2?-.45f:.45f;l.SetPositions(new[]{new Vector3(x-Mathf.Sign(x)*.22f,y),new Vector3(x,y),new Vector3(x,y-Mathf.Sign(y)*.22f)});corners[i]=l;}
        }
        void LateUpdate()
        {
            if(input.PointerMoved||input.AimStick.sqrMagnitude>.08f||input.FlashHeld)visibleUntil=Time.time+1.6f;
            bool show=!input.Paused&&!input.GameplayLocked&&Time.time<visibleUntil;foreach(var l in corners)l.enabled=show;if(!show)return;
            PhotoTarget target=null;foreach(var photo in FindObjectsByType<PhotoTarget>())if(aim.CanPhotograph(photo)){target=photo;break;}
            Vector2 at=(Vector2)transform.position+aim.Direction*3.1f;if(target)at=target.transform.position;
            frame.position=(Vector3)at+Vector3.up*.5f;Color color=target?new Color(.38f,1,.58f):new Color(.94f,.96f,.88f,.85f);if(aim.FlashIntensity>0)color=Color.white;
            foreach(var l in corners)l.startColor=l.endColor=color;
        }
        void OnDestroy(){if(material)Destroy(material);}
    }
}
