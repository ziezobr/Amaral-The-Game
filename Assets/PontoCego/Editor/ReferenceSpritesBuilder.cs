using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        // Crop cells from the supplied reference sheets. Flood only the dark background
        // connected to each cell border, preserving enclosed clothing and facial details.
        static Sprite ReferenceCell(Texture2D source,RectInt top,string name,float height)
        {
            int w=top.width,h=top.height;var pixels=source.GetPixels(top.x,source.height-top.y-h,w,h);var seen=new bool[w*h];var queue=new Queue<int>();
            void Seed(int i){if(seen[i])return;var c=pixels[i];if(c.r>.16f||c.g>.19f||c.b>.22f||c.b<c.r+.01f||c.g<c.r+.006f)return;seen[i]=true;queue.Enqueue(i);}
            for(int x=0;x<w;x++){Seed(x);Seed((h-1)*w+x);}for(int y=0;y<h;y++){Seed(y*w);Seed(y*w+w-1);}
            while(queue.Count>0){int i=queue.Dequeue(),x=i%w,y=i/w;pixels[i]=Color.clear;if(x>0)Seed(i-1);if(x<w-1)Seed(i+1);if(y>0)Seed(i-w);if(y<h-1)Seed(i+w);}
            var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name};t.SetPixels(pixels);t.Apply();AssetDatabase.AddObjectToAsset(t,bank);
            var s=Sprite.Create(t,new Rect(0,0,w,h),new Vector2(.5f,.1f),h/height);s.name=name;AssetDatabase.AddObjectToAsset(s,bank);return s;
        }
        static Sprite[] pcIcons,higorFrames;
        static void LoadQuestReferences()
        {
            referenceActors.Clear();LoadFixedAssets();
            var pc=Import(Art+"/References/pc.png");pcIcons=new Sprite[6];
            var rects=new[]{new RectInt(12,12,190,160),new RectInt(20,182,177,136),new RectInt(12,326,202,179),new RectInt(20,518,198,144),new RectInt(437,671,143,143),new RectInt(14,833,190,172)};
            for(int i=0;i<6;i++)pcIcons[i]=ReferenceCell(pc,rects[i],PcCollectible.Names[i],1);
            var h=Import(Art+"/References/higor.png");higorFrames=new Sprite[7];for(int i=0;i<7;i++)higorFrames[i]=ReferenceCell(h,new RectInt(77+i*54,190,51,84),"Higor / movimento "+i,2.05f);
        }
        static GameObject Higor(Vector2 at,bool fighter)
        {
            var go=new GameObject("Higor / "+(fighter?"combo de artes marciais":"motorista do Uber"));go.transform.position=at;go.layer=fighter?13:10;
            var sr=Visual("Higor",higorFrames[0],at,20000,go.transform);sr.transform.localPosition=Vector3.up*.65f;
            if(fighter){go.AddComponent<Rigidbody2D>().gravityScale=0;go.AddComponent<CircleCollider2D>().radius=.25f;var hp=go.AddComponent<Health>();hp.team=CombatTeam.Ally;hp.maximum=240;var martial=go.AddComponent<HigorMartialArts>();martial.frames=higorFrames;martial.visual=sr;}
            else{var c=go.AddComponent<CircleCollider2D>();c.radius=.45f;c.isTrigger=true;go.AddComponent<UberDriver>();}
            return go;
        }
        static void AddPcQuest(CityMission mission,int index)
        {
            var go=new GameObject("Peça de PC / "+mission.id);go.layer=10;go.transform.SetParent(mission.actors.transform);go.transform.position=SafeCollectible(mission.center+new Vector2(-mission.size.x*.32f,-mission.size.y*.28f));
            var c=go.AddComponent<CircleCollider2D>();c.radius=.55f;c.isTrigger=true;var part=go.AddComponent<PcCollectible>();part.id="pc:mission:"+mission.id;part.kind=index%6;
            var sr=Visual(PcCollectible.Names[part.kind],pcIcons[part.kind],go.transform.position,WorldSort.At(go.transform.position.y),go.transform);sr.transform.localPosition=Vector3.up*.45f;HideInBush(go,sr);
            foreach(var enemy in mission.actors.GetComponentsInChildren<EnemyAgent>(true))if(enemy.GetComponent<Health>().boss){if(!enemy.GetComponent<BossAbilities>())enemy.gameObject.AddComponent<BossAbilities>();var drop=enemy.gameObject.AddComponent<BossRareDrop>();drop.foodSprites=foodIcons;}
            foreach(var boss in mission.actors.GetComponentsInChildren<EnemyAgent>(true))if(boss.role==EnemyRole.Cenoura){var ability=boss.GetComponent<BossAbilities>();if(!ability)ability=boss.gameObject.AddComponent<BossAbilities>();ability.higor=Higor(mission.entry+Vector2.up*3,true);ability.higor.transform.SetParent(mission.actors.transform,true);ability.higor.SetActive(false);}
        }
        static Dictionary<string,(Sprite[] idle,Sprite[] walk)> referenceActors=new Dictionary<string,(Sprite[],Sprite[])>();
        static void AdaptActors(GameObject root)
        {
            foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var owner=sr.transform.parent;if(!owner||owner.GetComponent<ReferenceActorAnimation>())continue;
                string name=owner.name.ToLowerInvariant(),key=null;
                if(owner.GetComponent<PlayerMotor>())continue;else if(owner.GetComponent<EnemyAgent>() is EnemyAgent e)key=e.role==EnemyRole.Cenoura?"kevyn":e.role==EnemyRole.Paulo?"paulo":null;
                else if(owner.GetComponent<AllyAgent>()||owner.GetComponent<NpcIdle>())foreach(var n in new[]{"ribeiro","cristyan","henzo","martins","paulo"})if(name.Contains(n)){key=n;break;}
                if(key==null)continue;bool primary=owner.GetComponent<PlayerMotor>()?sr==owner.GetComponent<DirectionalAnimation>().bodySprite:owner.GetComponent<EnemyAgent>()?sr==owner.GetComponent<EnemyAgent>().visual:owner.GetComponent<AllyAgent>()?sr==owner.GetComponent<AllyAgent>().visual:!sr.name.Contains("Sombra");if(!primary)continue;
                if(!referenceActors.TryGetValue(key,out var frames))
                {
                    var sheet=Import(Art+"/References/"+key+".png");var idle=new Sprite[8];var walk=new Sprite[24];
                    for(int d=0;d<8;d++)
                    {
                        bool amaral=key=="amaral";int left=amaral?10:11,top=amaral?239:268,h=amaral?106:92;
                        idle[d]=ReferenceCell(sheet,new RectInt(left+d*94,top,89,h),key+" / parado "+d,2.05f);
                        for(int frame=0;frame<3;frame++)walk[d*3+frame]=ReferenceCell(sheet,new RectInt(779+d*93,top+frame*(amaral?109:85),88,amaral?103:84),key+" / andar "+d+" / "+frame,2.05f);
                    }
                    frames=(idle,walk);referenceActors[key]=frames;
                }
                var animation=owner.gameObject.AddComponent<ReferenceActorAnimation>();animation.visual=sr;animation.idle=frames.idle;animation.walk=frames.walk;sr.sprite=frames.idle[0];sr.transform.localScale=Vector3.one;
            }
        }
        static void AddPcWorkbench()
        {
            AddMateus();
            var seller=UnityEngine.Object.FindAnyObjectByType<EquipmentShop>();if(!seller)return;var go=new GameObject("Bancada do PC / reunião com Henzo");go.layer=10;go.transform.position=seller.transform.position+Vector3.left*2;go.AddComponent<CircleCollider2D>().isTrigger=true;go.AddComponent<PcWorkbench>();var sr=Visual("PC em montagem",pcIcons[2],go.transform.position,20000,go.transform);sr.transform.localPosition=Vector3.up*.5f;
            var oldDriver=GameObject.Find("Motorista de Uber / seletor de fases");Vector2 driverAt=oldDriver?(Vector2)oldDriver.transform.position:(Vector2)go.transform.position+Vector2.down*1.5f;if(oldDriver)UnityEngine.Object.DestroyImmediate(oldDriver);Higor(driverAt,false);
            foreach(var name in new[]{"Ribeiro","Cristyan","Martins"}){int idx=name=="Ribeiro"?1:name=="Cristyan"?2:4;var friend=new GameObject(name+" / reunião do PC");friend.transform.SetParent(go.transform);friend.transform.localPosition=new Vector3(-2+idx*.8f,1.8f,0);Visual(name,bank.cast[idx],friend.transform.position,20000,friend.transform);friend.SetActive(false);}
            var player=UnityEngine.Object.FindAnyObjectByType<PlayerMotor>();var artifact=player.gameObject.AddComponent<MotherboardArtifact>();artifact.icon=pcIcons[2];
        }
        static void BuildReferenceTraffic(WorldHub world)
        {
            var routes=new[]{new[]{new Vector2(-36,-31.5f),new Vector2(88.5f,-31.5f),new Vector2(88.5f,-28.5f),new Vector2(-36,-28.5f)},new[]{new Vector2(-38.5f,-29),new Vector2(-38.5f,27.5f),new Vector2(-35.5f,27.5f),new Vector2(-35.5f,-29)},new[]{new Vector2(-31.5f,-74),new Vector2(-31.5f,-32),new Vector2(-28.5f,-32),new Vector2(-28.5f,-74)},new[]{new Vector2(-83.5f,87.5f),new Vector2(88.5f,87.5f),new Vector2(88.5f,30.5f),new Vector2(-83.5f,30.5f)},new[]{new Vector2(91.5f,27.5f),new Vector2(196.5f,27.5f),new Vector2(196.5f,-86.5f),new Vector2(-28.5f,-86.5f),new Vector2(-28.5f,-31.5f),new Vector2(91.5f,-31.5f)}};
            for(int model=0;model<16;model++)
            {
                var frames=cleanCars[model%8];
                /*
                frames[0]=ReferenceCell(sheet,new RectInt(x+22,y+43,115,103),"Carro "+model+" frente",3.25f);
                frames[1]=ReferenceCell(sheet,new RectInt(x+143,y+43,181,103),"Carro "+model+" esquerda",3.25f);
                frames[2]=ReferenceCell(sheet,new RectInt(x+332,y+43,111,103),"Carro "+model+" traseira",3.25f);
                frames[3]=ReferenceCell(sheet,new RectInt(x+450,y+43,192,103),"Carro "+model+" direita",3.25f);
*/
                var route=routes[model%routes.Length];var go=new GameObject("TrÃ¢nsito / "+new[]{"Peugeot 206","Fiat Uno","Gol G5","Celta","Palio","Fox","Onix","HB20","Corolla","Strada"}[model%10]);go.layer=8;
                int segment=model/routes.Length;float t=.15f+segment*.22f;go.transform.position=Vector2.Lerp(route[0],route[1],t);
                var sr=Visual("Carro",frames[0],go.transform.position,20000,go.transform);sr.transform.localPosition=Vector3.zero;
                var body=go.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.mass=12;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
                go.AddComponent<BoxCollider2D>().size=new Vector2(1.4f,3.25f);var traffic=go.AddComponent<CityTraffic>();traffic.views=frames;traffic.route=route;traffic.speed=3.5f+(model%3)*.35f;
            }
            int index=0;foreach(var walker in UnityEngine.Object.FindObjectsByType<StreetWalker>())
            {walker.frames=cleanPeople[index++%8];walker.visual.sprite=walker.frames[0];walker.visual.transform.localScale=Vector3.one;}

        }
    }
}
