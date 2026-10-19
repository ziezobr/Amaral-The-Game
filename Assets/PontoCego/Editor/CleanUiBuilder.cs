using UnityEditor;
using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static GameUiArt uiArt;
        static void LoadCleanUi()
        {
            uiArt=AssetDatabase.LoadAssetAtPath<GameUiArt>(Root+"/GameUi.asset");if(!uiArt){uiArt=ScriptableObject.CreateInstance<GameUiArt>();AssetDatabase.CreateAsset(uiArt,Root+"/GameUi.asset");}
            uiArt.icons=Slice(Import(Art+"/UI/icons-atlas.png"),4,4,1.1f,"Interface",true);
            var note=Import(Art+"/UI/caramelo-gold.png");uiArt.caramelo=Patch(note,new Rect(0,0,note.width,note.height),"Nota caramelo",3);
            uiArt.uber=Import(Art+"/UI/uber-user.png");uiArt.phone=Import(Art+"/UI/phone-user.png");uiArt.mainMenu=Import(Art+"/UI/main-menu-user.png");EditorUtility.SetDirty(uiArt);
        }
        static void MissionProps()
        {
            foreach(var photo in UnityEngine.Object.FindObjectsByType<PhotoTarget>())AddProp(photo.gameObject,photo.name.ToLowerInvariant().Contains("caixa")?13:14,photo.requiredStep,1.1f);
            foreach(var item in UnityEngine.Object.FindObjectsByType<StoryInteractable>())
            {
                if(item.GetComponent<NpcIdle>()||item.transform.childCount>0)continue;
                if(item.completesStage||!string.IsNullOrEmpty(item.nextScene)||item.endsOpening)continue;
                AddProp(item.gameObject,item.name.ToLowerInvariant().Contains("carga")?13:12,item.requiredStep,.95f);
            }
        }
        static void AddProp(GameObject root,int icon,int step,float height)
        {
            // Documents rest on a desk, boxes on a pallet, and noticeboards on their own feet.
            // The interaction root and trigger remain at the original authored quest coordinate.
            var support=new System.Collections.Generic.List<SpriteRenderer>();
            var orders=new System.Collections.Generic.List<int>();
            var wood=Procedural("Objetivo / madeira de apoio",16,(x,y)=>new Color(.39f+(y%4==0?-.05f:.015f),.25f,.13f));
            var metal=Procedural("Objetivo / estrutura",4,(x,y)=>new Color(.20f,.23f,.23f));
            var contact=Procedural("Objetivo / sombra no chão",48,(x,y)=>
            {float d=new Vector2((x-23.5f)/23.5f,(y-23.5f)/23.5f).magnitude;return new Color(.025f,.035f,.025f,Mathf.Clamp01(1-d)*.28f);});
            void Part(string label,Sprite sprite,Vector2 at,Vector2 size,int order)
            {
                var visual=Visual(label,sprite,(Vector2)root.transform.position+at,1000+order,root.transform);
                visual.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);
                support.Add(visual);orders.Add(order);
            }
            Part("Sombra de contato / objeto",contact,Vector2.zero,new Vector2(height*1.16f,height*.25f),-3);
            bool document=icon==12||icon==14;
            if(document)
            {
                foreach(float x in new[]{-.43f,.43f})Part("Mesa / pé",metal,new Vector2(x*height,.23f*height),new Vector2(.07f*height,.48f*height),0);
                Part("Mesa / tampo",wood,Vector2.up*height*.46f,new Vector2(1.16f*height,.25f*height),1);
                Part("Mesa / borda frontal",wood,Vector2.up*height*.35f,new Vector2(1.16f*height,.07f*height),2);
            }
            else if(icon==13)
            {
                Part("Pallet / base",wood,Vector2.up*height*.06f,new Vector2(1.14f*height,.12f*height),0);
                foreach(float x in new[]{-.36f,.36f})Part("Pallet / travessa",wood,new Vector2(x*height,.04f*height),new Vector2(.12f*height,.13f*height),1);
            }
            var sr=Visual("Objeto / "+root.name,uiArt.icons[icon],root.transform.position,1004,root.transform);
            float ratio=(document?height*.62f:height)/sr.sprite.bounds.size.y;
            sr.transform.localScale=new Vector3(ratio,document?ratio*.58f:ratio,1);
            sr.transform.localPosition=Vector3.up*height*(document?.57f:icon==13?.62f:.5f);
            if(document)sr.transform.localRotation=Quaternion.Euler(0,0,-7);
            var prop=root.GetComponent<WorldObjectiveProp>();if(!prop)prop=root.AddComponent<WorldObjectiveProp>();
            prop.visual=sr;prop.requiredStep=step;prop.supports=support.ToArray();prop.supportOrders=orders.ToArray();prop.visualOrderOffset=4;
        }
        static void DistrictBoard(Vector2 at,string name)
        {
            // Decorative boards belong beside the entrance, outside the vehicle lane.
            var board=new GameObject("Quadro de missões / "+name);
            board.transform.position=at+(name=="road"?Vector2.up:Vector2.right)*4.8f;
            AddProp(board,15,-1,1.7f);
        }
    }
}
