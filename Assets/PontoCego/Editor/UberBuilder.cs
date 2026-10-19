using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static void BuildUber(Vector2 entry)
        {
            var npc=new GameObject("Motorista de Uber / seletor de fases");npc.layer=10;npc.transform.position=Free(entry+new Vector2(1.8f,-1.1f));
            npc.AddComponent<CircleCollider2D>().isTrigger=true;npc.AddComponent<UberDriver>();
            Visual("Motorista",civilianFrames[8],npc.transform.position,1200,npc.transform);
            var sign=new GameObject("Uber / placa de destinos");sign.transform.SetParent(npc.transform,false);sign.transform.localPosition=Vector3.up*2.3f;
            var text=sign.AddComponent<TextMesh>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.text="UBER / DESTINOS";text.fontSize=48;text.characterSize=.15f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.65f,1,.8f);
            var renderer=sign.GetComponent<MeshRenderer>();renderer.sharedMaterial=text.font.material;renderer.sortingOrder=2400;var fit=sign.AddComponent<WorldTextLabel>();fit.width=3;fit.height=.4f;fit.visibleDistance=9;
            Vehicle(0,Free(entry+Vector2.down*2f),"Uber / carro do motorista");
        }
    }
}
