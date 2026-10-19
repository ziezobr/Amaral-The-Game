using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static List<Vector2> cityRoadStarts,cityRoadEnds;static Sprite cityAsphalt;
        sealed class District
        {
            public string key;public Vector2 center,size;public float gateX;
            public bool built;public Texture2D map;
            public Vector2 Entry=>center+(key=="road"?new Vector2(-size.x*.5f+1.6f,0):new Vector2(gateX,-size.y*.5f+1.6f));
        }
        static void BuildContinuousCity()
        {
            cityAsphalt=null;LoadQuestReferences();
            var world=EditorSceneManager.OpenScene(Root+"/Scenes/25_Bairro.unity",OpenSceneMode.Single);
            foreach(var root in world.GetRootGameObjects())foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))if(renderer.sortingOrder>=0)renderer.sortingOrder+=19000;
            var taxi=GameObject.Find("Uber / carro do motorista");
            if(taxi){var car=taxi.GetComponentInChildren<SpriteRenderer>();car.sprite=cleanCars[0][2];car.transform.localScale=Vector3.one;taxi.transform.position+=Vector3.left*1.9f;taxi.layer=8;taxi.AddComponent<BoxCollider2D>().size=new Vector2(1.55f,3.4f);foreach(var renderer in taxi.GetComponentsInChildren<SpriteRenderer>())renderer.sortingOrder=20000-Mathf.RoundToInt(taxi.transform.position.y*100);}
            var hub=UnityEngine.Object.FindAnyObjectByType<LevelProgress>();var hud=UnityEngine.Object.FindAnyObjectByType<GameHud>();
            var hubSouth=GameObject.Find("Colisão / limite sul")?.GetComponent<BoxCollider2D>();
            cityRoadStarts=new List<Vector2>();cityRoadEnds=new List<Vector2>();
            hub.title="Patos de Minas • cidade contínua";hub.chapter="MUNDO ABERTO";
            var controller=new GameObject("Cidade persistente / missões por coordenada").AddComponent<WorldHub>();controller.exploration=hub;controller.hud=hud;controller.gameObject.AddComponent<CityNavigation>();
            var chaseArt=controller.gameObject.AddComponent<WorldRoadAssets>();chaseArt.riding=bank.riding;const string musicPath="Assets/PontoCego/Audio/Asphalt_Grip.mp3";AssetDatabase.ImportAsset(musicPath,ImportAssetOptions.ForceSynchronousImport);var audioImporter=AssetImporter.GetAtPath(musicPath) as AudioImporter;if(audioImporter){var audioSettings=audioImporter.defaultSampleSettings;audioSettings.loadType=AudioClipLoadType.Streaming;audioSettings.compressionFormat=AudioCompressionFormat.Vorbis;audioSettings.quality=.85f;audioImporter.defaultSampleSettings=audioSettings;audioImporter.SaveAndReimport();}chaseArt.chaseMusic=AssetDatabase.LoadAssetAtPath<AudioClip>(musicPath);chaseArt.gol=bank.vehicles[1];chaseArt.chain=questIcons[2];chaseArt.bmx=questIcons[3];chaseArt.skate=questIcons[4];chaseArt.cristyan=bank.cast[2];chaseArt.ribeiro=bank.cast[1];chaseArt.smoke=smokePuff;
            new GameObject("Final da história").AddComponent<CampaignEnding>();controller.gameObject.AddComponent<TaxiService>().sprites=new[]{cleanCars[0][0],cleanCars[0][3],cleanCars[0][2],cleanCars[0][1]};
            var districts=new Dictionary<string,District>();
            void Zone(string key,float x,float y,float w=48,float h=32,float gate=0){districts[key]=new District{key=key,center=new Vector2(x,y),size=new Vector2(w,h),gateX=gate};}
            Zone("office",-60,0,36,24);Zone("gala",-60,48,36,24,-5.04f);Zone("home",0,-55);Zone("workshop",60,0);
            Zone("warehouse",60,50);Zone("scrap",0,55);Zone("mocambo",-60,-55,48,32,7.68f);Zone("road",132,-55,96,32);
            var entries=new List<CityMission>();
            var definitions=new List<(string id,string map,string next)>{("01_Riberal","office","02_Gala"),("02_Gala","gala","03_Casa")};
            foreach(var stage in stages)definitions.Add((stage.scene,stage.map,stage.next));
            foreach(var definition in definitions)
            {
                var district=districts[definition.map];var source=EditorSceneManager.OpenScene(Root+"/Scenes/"+definition.id+".unity",OpenSceneMode.Additive);
                EditorSceneManager.SetActiveScene(world);
                var actors=new GameObject("Missão local / "+definition.id);SceneManager.MoveGameObjectToScene(actors,world);actors.SetActive(false);
                LevelProgress level=null;float factor=definition.map=="road"?96f/1440:1;
                foreach(var root in source.GetRootGameObjects())
                {
                    if(root.GetComponent<PlayerMotor>()||root.GetComponent<Camera>()||root.GetComponent<CinemachineCamera>()||root.layer==11||root.GetComponent<GameHud>()||root.GetComponent<GridNavigator>()||root.GetComponent<OfficeAudio>()||root.GetComponent<CampaignEnding>()||root.GetComponent<PrototypeCapture>()||root.GetComponent<UberDriver>()||root.name.StartsWith("Uber /"))continue;
                    bool scenery=root.name.StartsWith("Colisão /")||root.name.StartsWith("Cenário")||root.name.StartsWith("Oclusão /")||root.name.StartsWith("Placa /");
                    if(scenery&&district.built)continue;
                    if(definition.map=="road"&&(root.name.StartsWith("Rodovia / trecho")||scenery&&root.name.StartsWith("Cenário")))continue;
                    SceneManager.MoveGameObjectToScene(root,world);
                    Vector3 position=root.transform.position;position.x*=factor;root.transform.position=position+(Vector3)district.center;
                    if(!scenery)root.transform.SetParent(actors.transform,true);
                    else
                    {
                        if(root.TryGetComponent<BoxCollider2D>(out var box)){box.size=new Vector2(box.size.x*factor,box.size.y);OpenEntrance(box,district);}
                        foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))if(renderer.sortingOrder>=0)renderer.sortingOrder+=19000-Mathf.RoundToInt(district.center.y*100);
                    }
                    if(root.TryGetComponent<LevelProgress>(out var progress))level=progress;
                }
                if(!level)throw new InvalidOperationException("Missing mission definition: "+definition.id);
                level.mapSize=district.size;level.mapCenter=district.center;
                if(!district.built)
                {
                    district.map=level.mapTexture;
                    if(definition.map=="road")Visual("Cenário / rodovia integrada",Patch(district.map,new Rect(0,0,district.map.width,district.map.height),"Rodovia contínua",96),district.center,-5000);
                    ConnectDistrict(district);DistrictBoard(district.Entry,definition.map);
                }
                district.built=true;
                var marker=new GameObject("Ponto de missão / "+definition.id);SceneManager.MoveGameObjectToScene(marker,world);marker.layer=10;marker.transform.position=district.Entry;
                var collider=marker.AddComponent<CircleCollider2D>();collider.isTrigger=true;collider.radius=1.1f;
                var trigger=marker.AddComponent<WorldMissionTrigger>();trigger.missionId=definition.id;

                entries.Add(new CityMission{id=definition.id,next=definition.next,actors=actors,level=level,center=district.center,size=district.size,entry=district.Entry,trigger=trigger,road=definition.map=="road"});
                AddPcQuest(entries[entries.Count-1],entries.Count-1);
                EditorSceneManager.CloseScene(source,true);
            }
            EditorSceneManager.SetActiveScene(world);controller.missions=entries.ToArray();
            if(hubSouth)SplitOpening(hubSouth,-10.88f,true,4.5f);
            // A single global camera boundary replaces every copied per-phase confiner.
            var confiner=UnityEngine.Object.FindAnyObjectByType<CinemachineConfiner2D>();var boundary=(PolygonCollider2D)confiner.BoundingShape2D;
            boundary.isTrigger=true;var r=controller.bounds;boundary.points=new[]{new Vector2(r.xMin,r.yMin),new Vector2(r.xMax,r.yMin),new Vector2(r.xMax,r.yMax),new Vector2(r.xMin,r.yMax)};boundary.name="Limites contínuos de Patos de Minas";
            var ground=Procedural("Solo da cidade",4,(x,y)=>new Color(.27f,.34f,.22f));var terrain=Visual("Terreno contínuo / sem lacunas",ground,r.center,-10000);terrain.transform.localScale=new Vector3(r.width/ground.bounds.size.x,r.height/ground.bounds.size.y,1);
            Street(new Vector2(-10.88f,-30),new Vector2(-10.88f,-20),7);
            // Residential loops make the connectors part of a street network with alternate routes.
            Street(new Vector2(90,29),new Vector2(116,29),7);Street(new Vector2(116,29),new Vector2(116,-30),7);Street(new Vector2(116,-30),new Vector2(90,-30),7);
            Street(new Vector2(-60,-20),new Vector2(-85,-20),7);Street(new Vector2(-85,-20),new Vector2(-85,29),7);Street(new Vector2(-85,29),new Vector2(-65.04f,29),7);
            Street(new Vector2(-85,29),new Vector2(-85,89),7);Street(new Vector2(-85,89),new Vector2(150,89),7);Street(new Vector2(90,89),new Vector2(90,29),7);
            Street(new Vector2(150,89),new Vector2(150,29),7);Street(new Vector2(116,29),new Vector2(198,29),7);Street(new Vector2(198,29),new Vector2(198,-88),7);
            Street(new Vector2(198,-88),new Vector2(-30,-88),7);Street(new Vector2(-30,-88),new Vector2(-30,-76),7);
            Street(new Vector2(-85,29),new Vector2(198,29),7);
            Street(new Vector2(-10.88f,-20),new Vector2(-10.88f,29),7);Street(new Vector2(10.88f,-20),new Vector2(10.88f,20),7);
            Street(new Vector2(-30,9.38f),new Vector2(30,9.38f),7);Street(new Vector2(-30,-5.55f),new Vector2(30,-5.55f),7);
            var textureList=new List<Texture2D>{hub.mapTexture};var rectangles=new List<Rect>{new Rect(-32,-128f/6,64,128f/3)};
            foreach(var d in districts.Values){textureList.Add(d.map);rectangles.Add(new Rect(d.center-d.size*.5f,d.size));}
            controller.districtMaps=textureList.ToArray();controller.districtRects=rectangles.ToArray();
            controller.roadStarts=cityRoadStarts.ToArray();controller.roadEnds=cityRoadEnds.ToArray();
            RebuildCityRoadNetwork(controller);
            BuildStreetConnections(controller);BuildCityRouteScenery(controller);BuildReferenceTraffic(controller);AddPcWorkbench();
            AddExplorationCollectibles();
            BuildLandscapeIntegration(controller);
            LimitWall("norte",new Vector2(r.center.x,r.yMax),new Vector2(r.width,1));LimitWall("sul",new Vector2(r.center.x,r.yMin),new Vector2(r.width,1));
            LimitWall("oeste",new Vector2(r.xMin,r.center.y),new Vector2(1,r.height));LimitWall("leste",new Vector2(r.xMax,r.center.y),new Vector2(1,r.height));
            EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(world,Root+"/Scenes/"+WorldHub.SceneName+".unity");
        }
        static void OpenEntrance(BoxCollider2D box,District d)
        {
            Vector2 local=(Vector2)box.transform.position-d.center;
            if(d.key=="road"){if(local.x< -d.size.x*.43f&&box.size.y>d.size.y*.5f)SplitOpening(box,d.center.y,false,4.5f);}
            else if(local.y< -d.size.y*.30f&&box.size.x>d.size.x*.5f)SplitOpening(box,d.center.x+d.gateX,true,4.5f);
        }
        static void SplitOpening(BoxCollider2D box,float at,bool horizontal,float gap)
        {
            Vector2 center=box.transform.position;float length=horizontal?box.size.x:box.size.y;float middle=horizontal?center.x:center.y;
            float start=middle-length*.5f,end=middle+length*.5f;
            if(at-gap*.5f<start||at+gap*.5f>end)return;
            Vector2 size=box.size;box.enabled=false;
            foreach(var segment in new[]{new Vector2(start,at-gap*.5f),new Vector2(at+gap*.5f,end)})
            {
                Vector2 position=center;float span=segment.y-segment.x;if(horizontal){position.x=(segment.x+segment.y)*.5f;size.x=span;}else{position.y=(segment.x+segment.y)*.5f;size.y=span;}
                LimitWall(box.name+" / abertura",position,size);
            }
        }
        static void LimitWall(string name,Vector2 point,Vector2 size){var go=new GameObject("Colisão contínua / "+name);go.layer=8;go.transform.position=point;go.AddComponent<BoxCollider2D>().size=size;}
        static void ConnectDistrict(District d)
        {
            var path=new List<Vector2>{d.Entry};
            if(d.key=="office")path.AddRange(new[]{new Vector2(-60,-20),new Vector2(-37,-20),new Vector2(-37,-30)});
            else if(d.key=="gala"||d.key=="scrap")path.AddRange(new[]{new Vector2(d.Entry.x,29),new Vector2(-37,29),new Vector2(-37,-30)});
            else if(d.key=="warehouse"||d.key=="workshop")path.AddRange(new[]{new Vector2(60,d.key=="warehouse"?29:-24),new Vector2(90,d.key=="warehouse"?29:-24),new Vector2(90,-30)});
            else if(d.key=="road")path.AddRange(new[]{new Vector2(78,-55),new Vector2(78,-30)});
            else path.AddRange(new[]{new Vector2(d.Entry.x,-76),new Vector2(-30,-76),new Vector2(-30,-30)});
            path.Add(new Vector2(-10.88f,-30));
            for(int i=1;i<path.Count;i++)Street(path[i-1],path[i],7);
            Vector2 direction=(path[1]-path[0]).normalized,side=new Vector2(-direction.y,direction.x);var paint=Procedural("Faixa de pedestres",4,(x,y)=>new Color(.85f,.84f,.74f));
            for(float across=-2.7f;across<=2.7f;across+=.9f){var stripe=Visual("Travessia / acesso à missão",paint,d.Entry+direction*4.3f+side*across,-8998);bool horizontal=Mathf.Abs(direction.x)>.5f;stripe.transform.localScale=new Vector3((horizontal?1.65f:.45f)/paint.bounds.size.x,(horizontal?.45f:1.65f)/paint.bounds.size.y,1);}
        }
        static void Street(Vector2 from,Vector2 to,float width)
        {
            if(Vector2.Distance(from,to)<.01f)return;
            cityRoadStarts.Add(from);cityRoadEnds.Add(to);
            // A seamless asphalt tile avoids repeating painted curbs across the driving lanes.
            if(!cityAsphalt){var tile=Procedural("Asfalto urbano sem emendas",64,(x,y)=>{float grain=((x*73856093^y*19349663)&31)/3100f;return new Color(.16f+grain,.18f+grain,.19f+grain);});cityAsphalt=Sprite.Create(tile.texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),64,0,SpriteMeshType.FullRect);AssetDatabase.AddObjectToAsset(cityAsphalt,bank);}var texture=cityAsphalt;

            var sr=Visual("Rua de ligação",texture,(from+to)*.5f,-9000);bool horizontal=Mathf.Abs(to.x-from.x)>Mathf.Abs(to.y-from.y);
            sr.drawMode=SpriteDrawMode.Tiled;sr.size=new Vector2(horizontal?Mathf.Abs(to.x-from.x)+width:width,horizontal?width:Mathf.Abs(to.y-from.y)+width);
            var paint=Procedural("Sinalização viária",4,(x,y)=>new Color(.87f,.68f,.26f));
            for(float distance=1;distance<Vector2.Distance(from,to);distance+=4)
            {
                var stripe=Visual("Faixa da rua",paint,Vector2.MoveTowards(from,to,distance),-8999);
                stripe.transform.localScale=new Vector3((horizontal?1.6f:.12f)/paint.bounds.size.x,(horizontal?.12f:1.6f)/paint.bounds.size.y,1);
            }
        }
    }
}
