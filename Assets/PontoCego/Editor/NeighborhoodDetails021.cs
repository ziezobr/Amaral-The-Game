using System.Linq;
using UnityEditor;
using UnityEngine;
namespace PontoCego.Editor
{
    /// <summary>Local landmarks preserve mission gates, road graph and existing save coordinates.</summary>
    public static class NeighborhoodDetails021
    {
        const string Name="Cemitério / vizinho da casa de Amaral";
        public static void Apply(WorldHub world)
        {
            foreach(var t in world.gameObject.scene.GetRootGameObjects())if(t.name==Name||t.name=="Riberal / endereço José de Santana")Object.DestroyImmediate(t);
            foreach(var text in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include))
                if(text.text=="RUA DA RIBERAL")text.text="RUA JOSÉ DE SANTANA";
            var home=world.Find("03_Casa");if(home==null)return;
            var root=new GameObject(Name);var center=home.center+new Vector2(15.5f,-23);
            var yard=new Rect(center-new Vector2(8,5),new Vector2(16,10));
            foreach(var building in world.gameObject.scene.GetRootGameObjects().Where(g=>g.name.StartsWith("Cidade / quarteirão da rota /")).ToArray())
            {
                bool overlap=building.GetComponentsInChildren<SceneryFootprint2D>().Any(c=>c.groundBounds.Overlaps(yard));
                foreach(var facade in building.GetComponentsInChildren<SpriteRenderer>())if(facade.name=="Fachada e telhado")
                {var b=facade.bounds;if(new Rect(b.min,b.size).Overlaps(yard))overlap=true;}
                if(overlap)Object.DestroyImmediate(building);
            }
            var texture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Pedra do cemitério / textura"};
            var pixels=new Color[1024];for(int i=0;i<pixels.Length;i++){float n=((i*73+i/32*17)%29)/290f;pixels[i]=new Color(.48f+n,.47f+n,.42f+n);}
            texture.SetPixels(pixels);texture.Apply();
            string path="Assets/PontoCego/Urban/Cemetery021.asset";
            var stone=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if(!stone){AssetDatabase.CreateAsset(texture,path);stone=Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(.5f,.5f),32);stone.name="Pedra / cemitério";AssetDatabase.AddObjectToAsset(stone,texture);}else Object.DestroyImmediate(texture);
            SpriteRenderer Shape(string name,Vector2 at,Vector2 size,Color tint,int order,bool solid=false)
            {
                var go=new GameObject(name);go.transform.SetParent(root.transform,false);go.transform.position=at;go.transform.localScale=new Vector3(size.x,size.y,1);
                var sr=go.AddComponent<SpriteRenderer>();sr.sprite=stone;sr.color=tint;sr.sortingOrder=order;
                if(solid){go.layer=LayerMask.NameToLayer("World");go.AddComponent<BoxCollider2D>().size=Vector2.one;}return sr;
            }
            Shape("Pátio de cascalho",center,new Vector2(15,9),new Color(.7f,.75f,.66f),-4980);
            int depth=WorldSort.At(center.y-4.5f);
            Shape("Muro oeste",center+new Vector2(-7.5f,0),new Vector2(.22f,9),Color.gray,depth,true);
            Shape("Muro leste",center+new Vector2(7.5f,0),new Vector2(.22f,9),Color.gray,depth,true);
            Shape("Muro sul",center+new Vector2(0,-4.5f),new Vector2(15,.22f),Color.gray,depth,true);
            foreach(float side in new[]{-1f,1f})Shape("Muro / portão aberto",center+new Vector2(side*4.5f,4.5f),new Vector2(6,.22f),Color.gray,depth,true);
            Shape("Caminho do portão",center,new Vector2(2.6f,9),new Color(.9f,.87f,.75f),-4979);
            foreach(float x in new[]{-5.6f,-3.2f,3.2f,5.6f})foreach(float y in new[]{-2.8f,0f,2.5f})
            {
                Vector2 at=center+new Vector2(x,y);int order=WorldSort.At(at.y);
                Shape("Jazigo / sombra",at+new Vector2(.13f,-.17f),new Vector2(1.4f,1.9f),new Color(.18f,.21f,.18f),order-2);
                Shape("Jazigo / laje",at,new Vector2(1.2f,1.65f),new Color(.9f,.93f,.88f),order-1);
                Shape("Lápide",at+Vector2.up*.65f,new Vector2(.94f,.65f),new Color(.68f,.71f,.70f),order,true);
                Shape("Cruz vertical",at+Vector2.up*.7f,new Vector2(.09f,.38f),new Color(.2f,.23f,.22f),order+1);
                Shape("Cruz horizontal",at+Vector2.up*.75f,new Vector2(.3f,.08f),new Color(.2f,.23f,.22f),order+2);
                Shape("Flores",at+new Vector2(.35f,-.48f),new Vector2(.22f,.26f),new Color(.95f,.65f,.4f),order+2);
            }
            Label(root.transform,center+new Vector2(-4.5f,4.9f),"CEMITÉRIO",3.2f,stone);
            var office=world.Find("01_Riberal");if(office!=null)
            {
                var address=new GameObject("Riberal / endereço José de Santana");
                Label(address.transform,office.entry+new Vector2(-5.5f,.5f),"RUA JOSÉ DE SANTANA\n351 • AGÊNCIA RIBERAL",4.4f,stone);
            }
            Debug.Log("NEIGHBORHOOD021 / cemetery="+center+" home="+home.center+" / Rua José de Santana, 351");
        }
        static void Label(Transform root,Vector2 at,string text,float width,Sprite sprite)
        {
            var go=new GameObject(text);go.transform.SetParent(root,false);go.transform.position=at;
            var board=new GameObject("Placa");board.transform.SetParent(go.transform,false);board.transform.localScale=new Vector3(width,1,1);var sr=board.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.color=new Color(.08f,.23f,.19f);sr.sortingOrder=WorldSort.At(at.y)+1;
            var label=new GameObject("Texto");label.transform.SetParent(go.transform,false);var t=label.AddComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=64;t.characterSize=.08f;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;var mesh=t.GetComponent<MeshRenderer>();mesh.sharedMaterial=t.font.material;mesh.sortingOrder=sr.sortingOrder+1;var fit=label.AddComponent<WorldTextLabel>();fit.width=width-.2f;fit.height=.8f;fit.visibleDistance=15;
        }
    }
}
