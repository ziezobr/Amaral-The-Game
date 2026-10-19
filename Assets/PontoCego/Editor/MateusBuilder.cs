using System.IO;
using UnityEditor;
using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static Sprite MateusCell(Texture2D source,RectInt rect,string name)
        {
            var t=new Texture2D(rect.width,rect.height,TextureFormat.RGBA32,false);var pixels=source.GetPixels(rect.x,source.height-rect.y-rect.height,rect.width,rect.height);
            var seen=new bool[pixels.Length];var queue=new System.Collections.Generic.Queue<int>();
            void Seed(int i){if(seen[i])return;var c=pixels[i];if(Mathf.Max(c.r,c.g,c.b)>.17f||Mathf.Max(c.r,c.g,c.b)-Mathf.Min(c.r,c.g,c.b)>.035f)return;seen[i]=true;queue.Enqueue(i);}
            for(int x=0;x<rect.width;x++){Seed(x);Seed((rect.height-1)*rect.width+x);}for(int y=0;y<rect.height;y++){Seed(y*rect.width);Seed(y*rect.width+rect.width-1);}
            while(queue.Count>0){int i=queue.Dequeue(),x=i%rect.width,y=i/rect.width;pixels[i]=Color.clear;if(x>0)Seed(i-1);if(x<rect.width-1)Seed(i+1);if(y>0)Seed(i-rect.width);if(y<rect.height-1)Seed(i+rect.width);}
            t.name=name;t.SetPixels(pixels);t.Apply();AssetDatabase.AddObjectToAsset(t,bank);var s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.1f),t.height/2.1f);s.name=name;AssetDatabase.AddObjectToAsset(s,bank);return s;
        }
        static RectInt MateusAlphaBounds(Texture2D texture,RectInt cell,Color32[] pixels)
        {
            int minX=cell.xMax,minY=cell.yMax,maxX=cell.xMin-1,maxY=cell.yMin-1;
            for(int y=cell.yMin;y<cell.yMax;y++)for(int x=cell.xMin;x<cell.xMax;x++)
            {
                if(pixels[y*texture.width+x].a<40)continue;minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);
            }
            if(maxX<minX)throw new System.InvalidOperationException("Empty transparent Mateus sprite: "+cell);
            return new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);
        }
        static Sprite[] MateusCleanFrames()
        {
            string path=Art+"/References/mateus-clean-v2.png";
            if(!File.Exists(path))
            {
                var old=Import(Art+"/References/mateus.png");var front=MateusCell(old,new RectInt(410,5,155,287),"Mateus parado / referência original");
                return new[]{front,front,front,MateusCell(old,new RectInt(290,820,287,370),"Mateus contra-ataque / referência original")};
            }
            var sheet=Import(path);var pixels=sheet.GetPixels32();var rects=new RectInt[4];
            for(int i=0;i<4;i++){int x=Mathf.FloorToInt(i*sheet.width/4f),next=Mathf.FloorToInt((i+1)*sheet.width/4f);rects[i]=MateusAlphaBounds(sheet,new RectInt(x,0,next-x,sheet.height),pixels);}
            float basePpu=rects[0].height/2.05f;var poses=new Sprite[4];string[] names={"frente","lado","costas","contra-ataque"};
            for(int i=0;i<4;i++)
            {
                var rect=rects[i];long sumX=0;int count=0;
                // Anchor under the shoes, not the hand/attack silhouette. All states keep fixed feet.
                for(int y=rect.yMin;y<rect.yMin+Mathf.Max(2,rect.height/12);y++)for(int x=rect.xMin;x<rect.xMax;x++)if(pixels[y*sheet.width+x].a>80){sumX+=x;count++;}
                float feetX=count>0?sumX/(float)count:rect.center.x;
                float ppu=Mathf.Max(basePpu,rect.height/2.2f);
                var pose=Sprite.Create(sheet,new Rect(rect.x,rect.y,rect.width,rect.height),new Vector2(Mathf.Clamp01((feetX-rect.x)/rect.width),0),ppu,0,SpriteMeshType.FullRect);
                pose.name="Mateus HD / "+names[i];AssetDatabase.AddObjectToAsset(pose,bank);poses[i]=pose;
            }
            return poses;
        }
        static void AddMateus()
        {
            var poses=MateusCleanFrames();var go=new GameObject("Mateus / armas douradas / clareira escondida");go.layer=10;go.transform.position=Mateus.HomePosition;
            var interaction=go.AddComponent<CircleCollider2D>();interaction.radius=.5f;interaction.isTrigger=true;
            var hurt=new GameObject("Pés / alvo de ataque / Mateus");hurt.transform.SetParent(go.transform,false);hurt.layer=LayerMask.NameToLayer("Enemy");hurt.AddComponent<CircleCollider2D>().radius=.28f;
            var hp=go.AddComponent<Health>();hp.team=CombatTeam.Enemy;hp.maximum=1000;
            var mateus=go.AddComponent<Mateus>();mateus.cameraIcon=MateusCell(Import(Art+"/References/xuxa.png"),new RectInt(174,452,127,84),"Câmera da Xuxa");mateus.idle=poses[0];mateus.side=poses[1];mateus.back=poses[2];mateus.fighting=poses[3];
            mateus.visual=Visual("Mateus / personagem HD",poses[0],Mateus.HomePosition,WorldSort.At(Mateus.HomePosition.y)+1,go.transform);
            var feet=Visual("Mateus / sombra apoiada no chão",shadow,Mateus.HomePosition,WorldSort.At(Mateus.HomePosition.y)-2,go.transform);feet.transform.localScale=new Vector3(1.05f/shadow.bounds.size.x,.27f/shadow.bounds.size.y,1);
            var quest=go.AddComponent<GoldenWeaponQuest>();quest.parcel=uiArt.icons[13];quest.glock=questIcons[0];quest.razor=questIcons[1];quest.bush=questIcons[5];
            BuildMateusHideout(go,mateus);
        }
        static SpriteRenderer MateusGroundProp(string name,Sprite sprite,Vector2 feet,float width,Transform parent)
        {
            var sr=Visual(name,sprite,feet,WorldSort.At(feet.y),parent);float scale=width/sprite.bounds.size.x;sr.transform.localScale=Vector3.one*scale;sr.transform.position=(Vector3)feet+Vector3.up*(-sprite.bounds.min.y*scale);return sr;
        }
        static void BuildMateusHideout(GameObject npc,Mateus mateus)
        {
            var root=new GameObject("Mateus / clareira e trilha agrupadas");root.transform.position=Mateus.HomePosition;
            var terrain=Import(Art+"/Landscape/terrain-atlas-v1.png");var dirt=LandscapeCell(terrain,1,"Mateus / terra batida",12);
            void Floor(string name,Vector2 at,Vector2 size,float angle=0)
            {
                var sr=Visual("Mateus / solo / "+name,dirt,at,-9180,root.transform);sr.transform.localScale=new Vector3(size.x/dirt.bounds.size.x,size.y/dirt.bounds.size.y,1);sr.transform.rotation=Quaternion.Euler(0,0,angle);sr.sharedMaterial=GroundMaterial(dirt,Vector2.one,.22f);
            }
            Floor("clareira",Mateus.HomePosition+Vector2.up*.4f,new Vector2(9,7));
            Vector2 prior=Mateus.TrailEntrance;foreach(var next in new[]{new Vector2(-95,72.5f),Mateus.ApproachPosition})
            {Vector2 delta=next-prior;Floor("trilha de entrada",(prior+next)*.5f,new Vector2(delta.magnitude+1.4f,2.5f),Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);prior=next;}
            var bushSource=questIcons[5];var sourceRect=bushSource.rect;var bushTexture=bushSource.texture;var bushRect=MateusAlphaBounds(bushTexture,new RectInt((int)sourceRect.x,(int)sourceRect.y,(int)sourceRect.width,(int)sourceRect.height),bushTexture.GetPixels32());
            var bush=Sprite.Create(bushTexture,new Rect(bushRect.x,bushRect.y,bushRect.width,bushRect.height),new Vector2(.5f,0),bushRect.width/2.3f,0,SpriteMeshType.FullRect);bush.name="Mateus / moita recortada";AssetDatabase.AddObjectToAsset(bush,bank);
            foreach(var offset in new[]{new Vector2(-4,2.6f),new Vector2(-1.5f,3),new Vector2(1.3f,3),new Vector2(4,2),new Vector2(5.4f,-.1f),new Vector2(-5,0),new Vector2(-4.8f,-2.4f),new Vector2(-2.7f,-4.5f),new Vector2(1,-5.6f),new Vector2(4.5f,-6)})
            {
                var leaf=MateusGroundProp("Mateus / moita da clareira",bush,Mateus.HomePosition+offset,2.4f,root.transform);leaf.gameObject.AddComponent<WorldVegetation2D>();
            }
            var cover=MateusGroundProp("Mateus / moita discreta à frente",bush,Mateus.HomePosition+Vector2.down*.12f,2.05f,npc.transform);
            var hide=npc.AddComponent<CollectibleCover>();hide.cover=cover;hide.item=mateus.visual;
            // Crates stand together beside the character; neither the trail nor the reward is obstructed.
            MateusGroundProp("Mateus / caixa de encomendas",uiArt.icons[13],Mateus.HomePosition+new Vector2(-1.8f,1.2f),.85f,root.transform);
            MateusGroundProp("Mateus / estojo de ferramentas",uiArt.icons[7],Mateus.HomePosition+new Vector2(-2.5f,1.4f),.6f,root.transform);
        }
    }
}
