using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    /// <summary>Reuses the workshop's physical footprint and cutaway while replacing its exterior art.</summary>
    public static class WorkshopAppearance2D
    {
        static Sprite art;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;Apply();}
        static void Loaded(Scene scene,LoadSceneMode mode){Apply();}
        static Sprite signQuad;
        static void FinishGroundedSigns()
        {
            GameObject legacy=null;foreach(var candidate in Object.FindObjectsByType<Transform>()){if(candidate.name=="Placa / PARQUE DO MOCAMBO")legacy=candidate.gameObject;if(candidate.name=="Bairro / cenário próprio"){var oldMap=candidate.GetComponent<SpriteRenderer>();if(oldMap)oldMap.enabled=false;}}
            if(legacy){foreach(var fit in legacy.GetComponentsInChildren<WorldTextLabel>())fit.enabled=false;foreach(var render in legacy.GetComponentsInChildren<Renderer>())render.enabled=false;Debug.Log("PONTO_GROUNDED_SIGNS_V2");}
            foreach(var renderer in Object.FindObjectsByType<SpriteRenderer>())if(renderer.name.StartsWith("Placa do parque / suporte até o chão"))renderer.enabled=false;
            foreach(var box in Object.FindObjectsByType<BoxCollider2D>())if(box.name=="Colisão visível / suporte da placa Mocambo")box.enabled=false;
            if(!signQuad)signQuad=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f),Texture2D.whiteTexture.width);
            foreach(var label in Object.FindObjectsByType<TextMesh>())
            {
                if(label.name!="Parque / orientação"&&label.name!="Identificação do ferro-velho"&&label.name!="Triagem / placas")continue;
                if(GameObject.Find("Suporte da orientação "+label.name.Replace('/','-')))continue;
                Vector3 feet=label.transform.position;float width=label.name=="Identificação do ferro-velho"?4.6f:3.4f;
                var root=new GameObject("Suporte da orientação "+label.name.Replace('/','-'));root.transform.SetParent(label.transform.parent,true);root.transform.position=feet;
                void Part(string name,Vector2 at,Vector2 size,Color color,int order)
                {var go=new GameObject(name);go.transform.SetParent(root.transform,false);go.transform.position=(Vector3)at;var sr=go.AddComponent<SpriteRenderer>();sr.sprite=signQuad;sr.color=color;sr.sortingOrder=order;go.transform.localScale=new Vector3(size.x,size.y,1);}
                int sorting=WorldSort.At(feet.y);label.transform.position=feet+Vector3.up*1.3f;
                foreach(float side in new[]{-1f,1f})Part("Pé apoiado",(Vector2)feet+new Vector2(side*width*.34f,.65f),new Vector2(.12f,1.3f),new Color(.24f,.26f,.21f),sorting);
                Part("Painel de orientação",(Vector2)feet+Vector2.up*1.3f,new Vector2(width,.8f),new Color(.13f,.27f,.22f),sorting+1);
                var mesh=label.GetComponent<MeshRenderer>();mesh.sortingOrder=sorting+2;var fit=label.GetComponent<WorldTextLabel>();if(fit){fit.width=width-.2f;fit.height=.5f;}
            }
        }
        static void Apply()
        {
            if(!WorldHub.Instance)return;
            FinishGroundedSigns();
            if(!art)
            {
                var texture=Resources.Load<Texture2D>("workshop-henzo-v2");if(!texture)return;
                art=Sprite.Create(texture,new Rect(47,91,1454,885),new Vector2(.5f,0),1454f,0,SpriteMeshType.FullRect);art.name="Oficina Henzo / fachada HD v2";
            }
            foreach(var state in Object.FindObjectsByType<InteriorFacade2D>())
            {
                if(Vector2.Distance(state.footprint.center,new Vector2(60,0))>1)continue;
                foreach(var sr in state.exterior)if(sr&&sr.name.StartsWith("Fachada /"))
                {
                    sr.sprite=art;float width=Mathf.Min(state.footprint.width-3,(state.footprint.height-2)/art.bounds.size.y);
                    sr.transform.position=new Vector3(state.footprint.center.x,state.footprint.yMin+.35f,0);
                    sr.transform.localScale=Vector3.one*width;sr.sortingOrder=WorldSort.At(sr.transform.position.y);
                }
            }
        }
    }
}
