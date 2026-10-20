using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego.Editor
{
    /// <summary>Incremental migration: preserve authored missions and IDs, replace child art and ground collision contracts.</summary>
    public static class Polish021Builder
    {
        const string BankPath="Assets/PontoCego/Urban/Polish022h.asset",Art="Assets/PontoCego/UrbanArt/References/";
        static UrbanSpriteBank bank;static Dictionary<string,Sprite> cells;static Dictionary<string,Sprite[][]> actors;
        internal static Texture2D Read(string path){var imp=AssetImporter.GetAtPath(path) as TextureImporter;if(imp!=null&&(!imp.isReadable||imp.npotScale!=TextureImporterNPOTScale.None||imp.textureCompression!=TextureImporterCompression.Uncompressed)){imp.isReadable=true;imp.npotScale=TextureImporterNPOTScale.None;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=4096;imp.SaveAndReimport();}return AssetDatabase.LoadAssetAtPath<Texture2D>(path);}
        internal static Sprite Cell(Texture2D source,RectInt top,string name,float ppu)
        {
            if(cells.TryGetValue(name,out var found))return found;
            var pixels=source.GetPixels(top.x,source.height-top.y-top.height,top.width,top.height);int w=top.width,h=top.height;
            var queue=new Queue<int>();var seen=new bool[pixels.Length];bool alphaSource=source.GetPixel(0,0).a<.5f;
            void Seed(int i){if(seen[i])return;var c=pixels[i];if(c.a>.15f&&(alphaSource||!(c.r<(name.StartsWith("amaral",StringComparison.Ordinal)?.075f:.025f)&&c.g<.105f&&c.b<.14f&&c.b>c.r+.01f&&c.g>c.r+.008f)))return;seen[i]=true;queue.Enqueue(i);}
            for(int x=0;x<w;x++){Seed(x);Seed((h-1)*w+x);}for(int y=0;y<h;y++){Seed(y*w);Seed(y*w+w-1);}
            while(queue.Count>0){int i=queue.Dequeue(),x=i%w,y=i/w;pixels[i]=Color.clear;if(x>0)Seed(i-1);if(x<w-1)Seed(i+1);if(y>0)Seed(i-w);if(y<h-1)Seed(i+w);}
            var visited=new bool[pixels.Length];var largest=new List<int>();var parts=new List<List<int>>();
            for(int seed=0;seed<pixels.Length;seed++)
            {
                if(visited[seed]||pixels[seed].a<=.35f)continue;var component=new List<int>();queue.Enqueue(seed);visited[seed]=true;
                while(queue.Count>0){int current=queue.Dequeue();component.Add(current);int x=current%w,y=current/w;void Neighbor(int n){if(!visited[n]&&pixels[n].a>.35f){visited[n]=true;queue.Enqueue(n);}}if(x>0)Neighbor(current-1);if(x<w-1)Neighbor(current+1);if(y>0)Neighbor(current-w);if(y<h-1)Neighbor(current+w);}
                parts.Add(component);if(component.Count>largest.Count)largest=component;
            }
            Rect Box(List<int> part){int lx=w,rx=0,by=h,ty=0;foreach(int p in part){lx=Mathf.Min(lx,p%w);rx=Mathf.Max(rx,p%w);by=Mathf.Min(by,p/w);ty=Mathf.Max(ty,p/w);}return Rect.MinMaxRect(lx,by,rx+1,ty+1);}
            var selected=new HashSet<List<int>>{largest};var union=Box(largest);for(int pass=0;pass<4;pass++)foreach(var part in parts){if(selected.Contains(part)||part.Count<3)continue;var b=Box(part);if(b.width<=4&&b.height>h*.65f||b.height<=6&&b.width>w*.65f||b.yMin<=1||b.yMax>=h-1)continue;var expanded=Rect.MinMaxRect(union.xMin-3,union.yMin-3,union.xMax+3,union.yMax+3);if(expanded.Overlaps(b)){selected.Add(part);union=Rect.MinMaxRect(Mathf.Min(union.xMin,b.xMin),Mathf.Min(union.yMin,b.yMin),Mathf.Max(union.xMax,b.xMax),Mathf.Max(union.yMax,b.yMax));}}
            var mask=new bool[pixels.Length];foreach(var part in selected)foreach(int point in part)mask[point]=true;
            for(int i=0;i<pixels.Length;i++)if(!mask[i])pixels[i]=Color.clear;
            int left=w,right=-1,bottom=h,upper=-1;
            for(int y=2;y<h-2;y++)for(int x=2;x<w-2;x++)if(pixels[y*w+x].a>.35f){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);upper=Mathf.Max(upper,y);}
            if(right<left)throw new Exception("Empty character cell: "+name);
            var texture=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+" / pixels",filterMode=FilterMode.Bilinear};texture.SetPixels(pixels);texture.Apply();AssetDatabase.AddObjectToAsset(texture,bank);
            
            var sprite=Sprite.Create(texture,new Rect(left,bottom,right-left+1,upper-bottom+1),new Vector2(.5f,name.StartsWith("022 Carro",StringComparison.Ordinal)?.5f:0),ppu,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,bank);cells[name]=sprite;return sprite;
        }
        static RectInt[] RowActors(Texture2D source,int left,int top,int width,int height)
        {
            var pixels=source.GetPixels(left,source.height-top-height,width,height);var spans=new List<RectInt>();int start=-1,last=-1;
            for(int x=0;x<=width+4;x++)
            {
                int count=0;if(x<width)for(int y=0;y<height;y++){var c=pixels[y*width+x];if((c.r>.075f||c.g>.11f||c.b>.17f)&&!(c.b>c.r*2&&c.g>c.r*1.7f))count++;}
                if(count>3){if(start<0)start=x;last=x;}
                if(start>=0&&x-last>3){if(last-start>=16){int a=Mathf.Max(0,start-3),b=Mathf.Min(width,last+4);spans.Add(new RectInt(left+a,top,b-a,height));}start=-1;}
            }
            if(spans.Count<8)throw new Exception("Cannot isolate eight full actors: "+source.name+" row "+top+" found "+spans.Count);
            Debug.Log("SPRITE022_ROW / "+source.name+" / "+top+" / "+string.Join(",",spans.Select(r=>r.x+":"+r.width)));
            return spans.ToArray();
        }
        static Sprite[][] Frames(string key)
        {
            if(actors.TryGetValue(key,out var known))return known;
            var source=Read(Art+(key=="adryan"?"adryan-child-v1":key)+".png");if(!source)return null;
            var idle=new Sprite[8];var walk=new Sprite[24];var run=new Sprite[24];var idleCycle=new Sprite[24];
            bool child=key=="adryan",amaral=key=="amaral";
            RectInt[][] idleRows=null,walkRows=null;
            if(!child)
            {
                idleRows=new RectInt[3][];walkRows=new RectInt[3][];
                for(int f=0;f<3;f++)
                {
                    idleRows[f]=f==0?RowActors(source,16,amaral?235:270,740,amaral?113:key=="henzo"?101:key=="martins"?96:94):idleRows[0];
                    walkRows[f]=RowActors(source,785,amaral?new[]{235,335,435}[f]:new[]{270,360,444}[f],738,amaral?(f==2?99:105):new[]{95,89,90}[f]);
                }
            }
            for(int d=0;d<8;d++)
            {
                if(child){idle[d]=Cell(source,new RectInt(d*192,0,192,256),"Adryan criança / idle / "+d,172);for(int f=0;f<3;f++){walk[d*3+f]=Cell(source,new RectInt(d*192,(f+1)*256,192,256),"Adryan criança / andar / "+d+" / "+f,172);run[d*3+f]=walk[d*3+f];idleCycle[d*3+f]=idle[d];}continue;}
                int top=amaral?239:270,step=amaral?109:90,height=amaral?96:90;float ppu=(amaral?100:90)/2.05f;
                int Index(RectInt[] row)=>Mathf.RoundToInt(d*(row.Length-1)/7f);
                idle[d]=Cell(source,idleRows[0][Index(idleRows[0])],key+" / idle alinhado / "+d,ppu);
                for(int f=0;f<3;f++)
                {
                    walk[d*3+f]=Cell(source,walkRows[f][Index(walkRows[f])],key+" / andar alinhado / "+d+" / "+f,ppu);
                    run[d*3+f]=walk[d*3+f];
                    idleCycle[d*3+f]=idle[d];
                }
            }
            var result=new[]{idle,walk,run,idleCycle};actors[key]=result;return result;
        }
        public static void Apply()
        {
            AssetDatabase.Refresh();bank=AssetDatabase.LoadAssetAtPath<UrbanSpriteBank>(BankPath);if(!bank){bank=ScriptableObject.CreateInstance<UrbanSpriteBank>();AssetDatabase.CreateAsset(bank,BankPath);}
            cells=AssetDatabase.LoadAllAssetsAtPath(BankPath).OfType<Sprite>().ToDictionary(s=>s.name);actors=new Dictionary<string,Sprite[][]>();
            var energyImporter=AssetImporter.GetAtPath("Assets/PontoCego/Resources/henzo-stamina-white-v1.png") as TextureImporter;if(energyImporter){energyImporter.textureType=TextureImporterType.Sprite;energyImporter.spriteImportMode=SpriteImportMode.Single;energyImporter.alphaIsTransparency=true;energyImporter.mipmapEnabled=false;energyImporter.SaveAndReimport();}
            var originals=AssetDatabase.LoadAllAssetsAtPath("Assets/PontoCego/Urban/Sprites.asset").OfType<Sprite>().ToArray();
            var foods=Enumerable.Range(0,5).Select(i=>originals.FirstOrDefault(s=>s.name=="Alimento "+i+" dimensionado")).ToArray();
            if(foods.Any(s=>!s))throw new Exception("Missing food sprites for common-enemy drops");
            var enemySheet=Read(Art+"enemy-variety-v1.png");var enemyPoses=new Sprite[6][];
            for(int k=0;k<6;k++){enemyPoses[k]=new Sprite[4];for(int f=0;f<4;f++){int top=Mathf.Max(0,f*256-12),bottom=Mathf.Min(1024,(f+1)*256+12);enemyPoses[k][f]=Cell(enemySheet,new RectInt(k*256,top,256,bottom-top),"Variedade inimiga / "+k+" / "+f,120);}}
            int children=0,animated=0,walls=0,varietyCount=0;
            foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
            {
                var scene=EditorSceneManager.OpenScene(entry.path);var all=Resources.FindObjectsOfTypeAll<Transform>().Where(t=>t.gameObject.scene==scene).ToArray();
                foreach(var t in all)
                {
                    string key=null;bool child=t.name.StartsWith("Adryan /",StringComparison.Ordinal)||t.name.StartsWith("Adryan resgatado",StringComparison.Ordinal);
                    var existing=t.GetComponent<ReferenceActorAnimation>();
                    if(t.GetComponent<EnemyAgent>() is EnemyAgent common&&(int)common.role<4&&!common.GetComponent<Health>().boss)
                    {
                        int kind=varietyCount++%6;var variant=t.GetComponent<EnemyVariety>()??t.gameObject.AddComponent<EnemyVariety>();variant.archetype=(EnemyArchetype)kind;
                        common.role=kind==4?EnemyRole.Gunner:kind==3||kind==5?EnemyRole.Heavy:EnemyRole.Baton;
                        common.poses=enemyPoses[kind];common.visual.sprite=common.poses[0];
                        var drop=t.GetComponent<CommonEnemyFoodDrop>()??t.gameObject.AddComponent<CommonEnemyFoodDrop>();drop.foodSprites=foods;
                    }
                    if(child)key="adryan";else if(t.GetComponent<PlayerMotor>())key="amaral";
                    else if(existing||t.GetComponent<AllyAgent>()||t.GetComponent<NpcIdle>())foreach(var candidate in new[]{"ribeiro","cristyan","henzo","martins","paulo","kevyn"})if(t.name.ToLowerInvariant().Contains(candidate)){key=candidate;break;}
                    if(t.GetComponent<EnemyAgent>() is EnemyAgent enemy)key=enemy.role==EnemyRole.Cenoura?"kevyn":enemy.role==EnemyRole.Paulo?"paulo":key;
                    if(key!=null)
                    {
                        var frames=Frames(key);var visual=existing?existing.visual:t.GetComponent<DirectionalAnimation>()?t.GetComponent<DirectionalAnimation>().bodySprite:t.GetComponent<EnemyAgent>()?t.GetComponent<EnemyAgent>().visual:t.GetComponent<AllyAgent>()?t.GetComponent<AllyAgent>().visual:t.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(s=>!s.name.Contains("ombra"));
                        if(frames!=null&&visual){var animation=existing?existing:t.gameObject.AddComponent<ReferenceActorAnimation>();animation.visual=visual;animation.idle=frames[0];animation.walk=frames[1];animation.run=frames[2];animation.idleCycle=frames[3];visual.sprite=frames[0][0];visual.transform.localScale=Vector3.one;visual.transform.localPosition=Vector3.zero;if(child){visual.transform.localPosition=Vector3.zero;children++;}animated++;}
                    }
                    SpriteRenderer bodyVisual=t.GetComponent<ReferenceActorAnimation>()?.visual;
                    if(!bodyVisual&&t.GetComponent<EnemyAgent>())bodyVisual=t.GetComponent<EnemyAgent>().visual;
                    if(!bodyVisual&&t.GetComponent<StreetWalker>())bodyVisual=t.GetComponent<StreetWalker>().visual;
                    if(!bodyVisual&&t.GetComponent<HigorMartialArts>())bodyVisual=t.GetComponent<HigorMartialArts>().visual;
                    if(!bodyVisual&&t.GetComponent<Mateus>())bodyVisual=t.GetComponent<Mateus>().visual;
                    if(bodyVisual&&!t.GetComponent<ActorSecondaryMotion2D>())t.gameObject.AddComponent<ActorSecondaryMotion2D>().visual=bodyVisual;
                }
                SpriteRepair022.Apply(scene);var world=UnityEngine.Object.FindAnyObjectByType<WorldHub>();if(world){walls+=RepairWorld(world);NeighborhoodDetails021.Apply(world);}
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            bank.cast=actors["adryan"][0];EditorUtility.SetDirty(bank);PlayerSettings.bundleVersion="0.21.0";AssetDatabase.SaveAssets();
            Debug.Log("POLISH021_APPLIED / child actors="+children+" animated actors="+animated+" footprint walls="+walls+" varied enemies="+varietyCount);
        }
        static int RepairWorld(WorldHub world)
        {
            int count=0;var scene=world.gameObject.scene;var all=Resources.FindObjectsOfTypeAll<Transform>().Where(t=>t.gameObject.scene==scene).ToArray();
            foreach(var t in all.Where(t=>t&&t.name.StartsWith("Cidade / quarteirão da rota /")).ToArray())
            {
                var old=t.GetComponent<BoxCollider2D>();if(!old)continue;old.enabled=false;
                var rect=new Rect((Vector2)t.position+new Vector2(-3.25f,.18f),new Vector2(6.5f,3.5f));
                int model=int.TryParse(t.name.Substring(t.name.LastIndexOf('/')+1).Trim(),out int n)?n%6:0;
                var gates=model==3?new[]{rect.center.x-1.7f,rect.center.x+1.7f}:new[]{rect.center.x+(model==2?-.8f:model==0?-.3f:0)};
                count+=Outline(t,rect,gates,model==3?2.15f:1.35f);
                var facade=t.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(s=>s.name=="Fachada e telhado");if(facade){var cut=t.GetComponent<StreetDoorCutaway2D>()??t.gameObject.AddComponent<StreetDoorCutaway2D>();cut.facade=facade;cut.room=rect;}
            }
            foreach(var facade in Resources.FindObjectsOfTypeAll<InteriorFacade2D>().Where(x=>x.gameObject.scene==scene))
            {
                var rect=facade.footprint;float gate=rect.center.x;var mission=Array.Find(world.missions,m=>Vector2.Distance(m.center,rect.center)<.2f);if(mission!=null)gate=mission.entry.x;
                foreach(var box in Resources.FindObjectsOfTypeAll<BoxCollider2D>().Where(x=>x.gameObject.scene==scene&&x.enabled&&!x.attachedRigidbody&&x.gameObject.layer==8).ToArray())
                {
                    if(box.GetComponentInParent<SceneryFootprint2D>())continue;var b=box.bounds;
                    bool horizontal=b.size.x>rect.width*.4f&&b.size.y<1.7f&&Mathf.Abs(b.center.x-rect.center.x)<3&&(Mathf.Abs(b.center.y-rect.yMin)<2.3f||Mathf.Abs(b.center.y-rect.yMax)<2.3f);
                    bool vertical=b.size.y>rect.height*.4f&&b.size.x<1.7f&&Mathf.Abs(b.center.y-rect.center.y)<3&&(Mathf.Abs(b.center.x-rect.xMin)<2.3f||Mathf.Abs(b.center.x-rect.xMax)<2.3f);
                    if(horizontal||vertical)box.enabled=false;
                }
                count+=Outline(facade.transform,rect,new[]{gate},4.5f);
            }
            foreach(var box in Resources.FindObjectsOfTypeAll<BoxCollider2D>().Where(x=>x&&x.gameObject.scene==scene&&x.name=="Colisão / quarteirão central reconstruído"))
            {
                var rect=new Rect((Vector2)box.transform.position-box.size*.5f,box.size);box.enabled=false;
                count+=Outline(box.transform,rect,new[]{rect.center.x-rect.width*.24f,rect.center.x+rect.width*.24f},1.6f);
            }
            // These are physical world solids for every actor, including allies and projectiles.
            int worldLayer=LayerMask.NameToLayer("World");foreach(int layer in new[]{9,10,12,13,14})Physics2D.IgnoreLayerCollision(worldLayer,layer,false);
            Physics2D.SyncTransforms();return count;
        }
        static int Outline(Transform parent,Rect rect,float[] gates,float width)
        {
            var prior=parent.Find("Colisão alinhada 0.21");if(prior)UnityEngine.Object.DestroyImmediate(prior.gameObject);
            var root=new GameObject("Colisão alinhada 0.21");root.transform.SetParent(parent,true);root.transform.position=Vector3.zero;
            var contract=root.AddComponent<SceneryFootprint2D>();contract.groundBounds=rect;contract.entrances=gates.Select(x=>new Vector2(x,rect.yMin)).ToArray();contract.entranceWidths=gates.Select(x=>width).ToArray();int count=0;const float thickness=.22f;
            void Wall(string name,Vector2 center,Vector2 size){if(size.x<=0||size.y<=0)return;var go=new GameObject(name);go.transform.SetParent(root.transform,true);go.transform.position=center;go.layer=LayerMask.NameToLayer("World");go.AddComponent<BoxCollider2D>().size=size;count++;}
            Wall("Parede esquerda",new Vector2(rect.xMin,rect.center.y),new Vector2(thickness,rect.height));Wall("Parede direita",new Vector2(rect.xMax,rect.center.y),new Vector2(thickness,rect.height));Wall("Parede do fundo",new Vector2(rect.center.x,rect.yMax),new Vector2(rect.width,thickness));
            float start=rect.xMin;foreach(float gate in gates.OrderBy(x=>x)){float left=Mathf.Clamp(gate-width*.5f,rect.xMin,rect.xMax),right=Mathf.Clamp(gate+width*.5f,rect.xMin,rect.xMax);Wall("Fachada / parede sólida",new Vector2((start+left)*.5f,rect.yMin),new Vector2(left-start,thickness));start=right;}Wall("Fachada / parede sólida",new Vector2((start+rect.xMax)*.5f,rect.yMin),new Vector2(rect.xMax-start,thickness));return count;
        }
    }
}
