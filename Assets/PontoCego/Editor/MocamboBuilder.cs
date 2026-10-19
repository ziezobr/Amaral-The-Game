using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static void MocamboSign()
        {
            var root=new GameObject("Placa / PARQUE DO MOCAMBO");root.transform.position=W(.54f,.86f);
            var board=Procedural("Placa verde",64,(x,y)=>x<3||x>60||y<3||y>60?new Color(.84f,.74f,.43f):new Color(.055f,.23f,.16f));
            var sr=Visual("Placa de identificação",board,root.transform.position,2300,root.transform);
            sr.transform.localScale=new Vector3(6.5f/board.bounds.size.x,1.2f/board.bounds.size.y,1);
            var label=new GameObject("Nome do parque");label.transform.SetParent(root.transform,false);
            var text=label.AddComponent<TextMesh>();text.text="PARQUE DO MOCAMBO";text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=64;text.characterSize=.28f;
            text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(1,.96f,.82f);
            var renderer=label.GetComponent<MeshRenderer>();renderer.sharedMaterial=text.font.material;renderer.sortingOrder=2301;var fit=label.AddComponent<WorldTextLabel>();fit.width=6;fit.height=.65f;
            var collision=root.AddComponent<BoxCollider2D>();collision.size=new Vector2(6.5f,.45f);root.layer=8;
        }
        static Sprite BasketballSprite()=>Procedural("Bola de basquete",64,(x,y)=>
        {
            Vector2 p=new Vector2(x-31.5f,y-31.5f);float r=p.magnitude;
            if(r>30)return Color.clear;
            bool seam=Mathf.Abs(p.x)<1.5f||Mathf.Abs(p.y)<1.5f||Mathf.Abs(Mathf.Abs(p.x)-18+p.y*p.y*.015f)<1.5f;
            return seam||r>28?new Color(.16f,.075f,.025f):new Color(.94f,.39f,.075f);
        });
    }
}
