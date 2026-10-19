using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static Dictionary<string,Texture2D> campaignMaps=new Dictionary<string,Texture2D>();
        static Sprite shotSprite,smokePuff;
        sealed class StageDef
        {
            public string scene,title,act,map,next;
            public Vector2 spawn=new Vector2(.5f,.79f);
            public bool allies;
            public Color tint=Color.white;
            public string[] intro;
            public List<StepDef> steps=new List<StepDef>();
        }
        sealed class StepDef
        {
            public string kind,text,speaker="Amaral",message;
            public Vector2 uv;
            public EnemyRole[] roles;
            public string[] choices;public int correct;
            public SequenceKind sequence;
            public StepDef(string kind,string text,float u,float v,string message=null,string speaker="Amaral")
            {this.kind=kind;this.text=text;uv=new Vector2(u,v);this.message=message;this.speaker=speaker;}
        }
        static List<StageDef> stages;
        static void LoadCampaignArt()
        {
            civilianFrames=Slice(Import(Art+"/Campaign/pedestrians.png"),4,4,2.05f,"Pedestre",true);
            campaignMaps.Clear();foreach(string map in new[]{"home","workshop","warehouse","scrap","road","mocambo"})campaignMaps[map]=Import(Art+"/Campaign/"+map+".png");
            bank.enemies=Slice(Import(Art+"/Campaign/enemies.png"),3,2,2.05f,"Enemy");
            bank.bossMotion=Slice(Import(Art+"/Campaign/boss-combat-v3.png"),4,2,2.05f,"BossCombat",true);
            var texture=Import(Art+"/Campaign/vehicles.png");
            var rects=new[]{new Rect(.018f,.51f,.172f,.465f),new Rect(.195f,.51f,.315f,.36f),new Rect(.52f,.51f,.16f,.44f),new Rect(.69f,.51f,.307f,.36f),
                new Rect(.03f,.02f,.15f,.45f),new Rect(.195f,.02f,.315f,.44f),new Rect(.54f,.02f,.14f,.45f),new Rect(.69f,.02f,.307f,.44f)};
            var raw=new Sprite[8];for(int i=0;i<8;i++)
            {
                var r=rects[i];r=new Rect(r.x*texture.width,r.y*texture.height,r.width*texture.width,r.height*texture.height);
                raw[i]=Sprite.Create(texture,r,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);raw[i].name="Vehicle_"+i;AssetDatabase.AddObjectToAsset(raw[i],bank);
            }
            bank.vehicles=new[]{raw[2],raw[1],raw[0],raw[3],raw[6],raw[5],raw[4],raw[7]};
            var riding=Slice(Import(Art+"/Campaign/riding.png"),2,2,3.6f,"Riding",true);bank.riding=new[]{riding[2],riding[1],riding[0],riding[3]};
            bank.allyCombat=Slice(Import(Art+"/Campaign/ally-combat.png"),4,2,2.05f,"AllyCombat",true);
            bank.enemyMotion=Slice(Import(Art+"/Campaign/enemy-motion.png"),4,4,2.05f,"EnemyMotion",true);
            shotSprite=Procedural("Traço projétil",32,(x,y)=>Mathf.Abs(y-15.5f)<6?Color.white:Color.clear);
            smokePuff=Procedural("Fumaça suave",64,(x,y)=>new Color(1,1,1,Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f)));
        }
        static EditorBuildSettingsScene[] CampaignBuildScenes()
        {
            var list=new List<EditorBuildSettingsScene>{new EditorBuildSettingsScene(Root+"/Scenes/00_Menu.unity",true),new EditorBuildSettingsScene(Root+"/Scenes/"+WorldHub.SceneName+".unity",true),new EditorBuildSettingsScene(Root+"/Scenes/01_Riberal.unity",true),new EditorBuildSettingsScene(Root+"/Scenes/02_Gala.unity",true)};
            list.Add(new EditorBuildSettingsScene(Root+"/Scenes/25_Bairro.unity",true));foreach(var stage in stages)list.Add(new EditorBuildSettingsScene(Root+"/Scenes/"+stage.scene+".unity",true));list.Add(new EditorBuildSettingsScene(Root+"/Scenes/00_Laboratorio.unity",true));return list.ToArray();
        }
        static void BuildMainMenu()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var menu=new GameObject("Ponto Cego • campanha e demo").AddComponent<MainMenu>();menu.art=uiArt;const string menuTrack="Assets/PontoCego/Audio/Menu_RidersOnTheStorm.mp3";AssetDatabase.ImportAsset(menuTrack,ImportAssetOptions.ForceSynchronousImport);var menuImporter=AssetImporter.GetAtPath(menuTrack) as AudioImporter;if(menuImporter){var sample=menuImporter.defaultSampleSettings;sample.loadType=AudioClipLoadType.Streaming;sample.compressionFormat=AudioCompressionFormat.Vorbis;sample.quality=.85f;menuImporter.defaultSampleSettings=sample;menuImporter.SaveAndReimport();}menu.menuMusic=AssetDatabase.LoadAssetAtPath<AudioClip>(menuTrack);menu.background=office;menu.portrait=Portrait("Amaral");
            var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.gameObject.tag="MainCamera";camera.gameObject.AddComponent<AudioListener>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            new GameObject("Visual QA").AddComponent<PrototypeCapture>();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/Scenes/00_Menu.unity");
        }
        static Vector2 Free(Vector2 desired)
        {
            Physics2D.SyncTransforms();int mask=LayerMask.GetMask("World");if(!Physics2D.OverlapCircle(desired,.34f,mask))return desired;
            for(float r=.45f;r<5;r+=.45f)for(int i=0;i<16;i++)
            {float angle=i*Mathf.PI/8;var p=desired+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r;if(!Physics2D.OverlapCircle(p,.34f,mask))return p;}
            return desired;
        }
        static EnemyAgent Enemy(EnemyRole role,Vector2 point,string id,int stage)
        {
            var go=new GameObject("Inimigo / "+role+" / "+id);go.layer=12;go.transform.position=Free(point);
            int index=role==EnemyRole.Gunner?1:role==EnemyRole.Elite?2:role==EnemyRole.Heavy?3:role==EnemyRole.Cenoura?5:0;
            var sprite=role==EnemyRole.Paulo?bank.cast[0]:bank.enemies[index];var visual=Visual("Visual",sprite,go.transform.position,1000,go.transform);
            Visual("Sombra",shadow,go.transform.position,-2000,go.transform);
            var body=go.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;
            var collider=go.AddComponent<CircleCollider2D>();collider.radius=role==EnemyRole.Heavy ? .36f : .26f;
            var hp=go.AddComponent<Health>();hp.team=CombatTeam.Enemy;hp.boss=role==EnemyRole.Cenoura||role==EnemyRole.Paulo||role==EnemyRole.Pivo;
            hp.maximum=role==EnemyRole.Pivo?780:role==EnemyRole.Cenoura?900:role==EnemyRole.Paulo?1200:role==EnemyRole.Heavy?125:role==EnemyRole.Elite?105:65+Mathf.Min(30,stage*2);
            var enemy=go.AddComponent<EnemyAgent>();enemy.role=role;enemy.visual=visual;enemy.projectileSprite=shotSprite;enemy.rewardId=id;
            if(hp.boss)
            {
                var combat=go.AddComponent<BossCombatSprites>();combat.poses=new Sprite[4];if(role==EnemyRole.Pivo){var art=Import(Art+"/Campaign/pivo.png");var frames=Slice(art,4,1,2.5f,"Pivo",true);Array.Copy(frames,combat.poses,4);enemy.projectileSprite=BasketballSprite();}
                else Array.Copy(bank.bossMotion,role==EnemyRole.Cenoura?0:4,combat.poses,0,4);
                float ratio=visual.sprite.bounds.size.y/combat.poses[0].bounds.size.y;visual.transform.localScale=Vector3.one*ratio;visual.sprite=combat.poses[0];
            }
            if((int)role<4){enemy.poses=new Sprite[4];Array.Copy(bank.enemyMotion,(int)role*4,enemy.poses,0,4);visual.sprite=enemy.poses[0];}
            enemy.damage=role==EnemyRole.Heavy||hp.boss?20:role==EnemyRole.Elite?14:10;
            enemy.speed=role==EnemyRole.Heavy?1.8f:role==EnemyRole.Cenoura?2.6f:2.4f;
            enemy.attackInterval=role==EnemyRole.Elite ? .85f : 1.2f;enemy.cashReward=hp.boss?200:14;
            return enemy;
        }
        static AllyAgent Ally(string name,int index,Vector2 p)
        {
            var go=new GameObject(name+" • companheiro");go.layer=13;go.transform.position=Free(p);
            var visual=Visual(name+" / sprite",bank.cast[index],go.transform.position,1000,go.transform);Visual("Sombra",shadow,go.transform.position,-2000,go.transform);
            go.AddComponent<Rigidbody2D>().gravityScale=0;go.AddComponent<CircleCollider2D>().radius=.24f;
            var hp=go.AddComponent<Health>();hp.team=CombatTeam.Ally;hp.maximum=160;
            var ally=go.AddComponent<AllyAgent>();ally.characterName=name;ally.visual=visual;ally.ranged=name=="Ribeiro";ally.projectileSprite=shotSprite;
            ally.poses=new Sprite[4];Array.Copy(bank.allyCombat,name=="Ribeiro"?0:4,ally.poses,0,4);visual.sprite=ally.poses[0];
            ally.formationOffset=name=="Ribeiro"?new Vector2(-1.4f,-1):new Vector2(1.3f,-1);return ally;
        }
        static Transform Point(string name,Vector2 uv)
        {
            var go=new GameObject(name);go.transform.position=Free(W(uv.x,uv.y));return go.transform;
        }
        static Transform Vehicle(int spriteIndex,Vector2 position,string name)
        {
            var root=new GameObject(name);root.transform.position=position;var sr=Visual("Veículo",bank.vehicles[spriteIndex],position,1200,root.transform);
            float length=spriteIndex<4?3.2f:3.0f;float scale=length/Mathf.Max(sr.sprite.bounds.size.x,sr.sprite.bounds.size.y);sr.transform.localScale=Vector3.one*scale;
            return root.transform;
        }
        static void ConfigureGalaEscape(LevelProgress level)
        {
            var mission=level.gameObject.AddComponent<CombatMission>();mission.stageNumber=2;mission.nextScene="03_Casa";
            var a=Enemy(EnemyRole.Baton,W(.53f,.57f),"gala:security:1",2);var b=Enemy(EnemyRole.Gunner,W(.64f,.49f),"gala:security:2",2);
            a.gameObject.SetActive(false);b.gameObject.SetActive(false);
            mission.rules=new[]{new MissionRule(),new MissionRule(),new MissionRule{enemies=new[]{a,b},kind=MissionRuleKind.Manual,arrivalMessage="Os seguranças ouviram o flash. Use a esquiva e corra para a entrada!"}};
        }
        static void Geometry(string map)
        {
            if(map=="office"){OfficeGeometry();return;}
            Block("limite norte",.5f,.015f,.98f,.025f);Block("limite sul",.5f,.975f,.98f,.025f);Block("limite oeste",.015f,.5f,.025f,.98f);Block("limite leste",.985f,.5f,.025f,.98f);
            if(map=="mocambo")
            {
                Block("mata norte",.5f,.025f,.97f,.05f);Block("mata oeste",.025f,.5f,.05f,.95f);Block("mata leste",.975f,.5f,.05f,.95f);Block("mata sudoeste",.19f,.84f,.36f,.12f);
                Block("suporte da cesta norte",.67f,.17f,.025f,.035f);Block("suporte da cesta sul",.67f,.60f,.025f,.035f);
                Block("lago do parque",.16f,.55f,.31f,.24f);Block("quiosque",.225f,.25f,.115f,.15f);
                Block("banco oeste",.145f,.365f,.05f,.025f);Block("banco sul",.49f,.75f,.055f,.025f);Block("banco leste",.93f,.49f,.025f,.09f);
            }
            if(map=="home")
            {
                Block("casas oeste",.166f,.16f,.30f,.30f);Block("casas leste",.842f,.16f,.29f,.30f);
                Block("casa / parede esquerda",.338f,.15f,.014f,.28f);Block("casa / parede direita",.656f,.15f,.014f,.28f);
                Block("casa / fachada esquerda",.417f,.295f,.145f,.035f);Block("casa / fachada direita",.586f,.295f,.135f,.035f);
                Block("casa / geladeira",.39f,.08f,.04f,.12f);Block("casa / bancada",.45f,.068f,.10f,.08f);Block("casa / cama",.58f,.10f,.12f,.12f);Block("casa / sofá",.47f,.22f,.04f,.075f);
                Block("casa / armário",.59f,.22f,.11f,.065f);Block("padaria",.15f,.735f,.27f,.18f);Block("muro terreno",.55f,.808f,.39f,.025f);Block("casa inferior direita",.94f,.74f,.09f,.22f);
            }
            if(map=="workshop")
            {
                Block("oficina / bancada",.50f,.125f,.28f,.13f);Block("oficina / estoque",.23f,.15f,.21f,.19f);Block("oficina / pneus esquerdos",.115f,.565f,.12f,.23f);
                Block("oficina / pneus direitos",.90f,.56f,.12f,.23f);Block("oficina / caixas esquerdas",.31f,.59f,.12f,.1f);Block("oficina / caixas direitas",.70f,.59f,.12f,.1f);
                Block("oficina / poste elevador 1",.31f,.30f,.025f,.10f);Block("oficina / poste elevador 2",.39f,.28f,.025f,.10f);Block("oficina / poste elevador 3",.615f,.285f,.025f,.10f);
                Block("oficina / balcão Henzo",.855f,.34f,.14f,.15f);Block("oficina / escritório",.80f,.14f,.16f,.12f);
            }
            if(map=="warehouse")
            {
                Block("galpão / parede esquerda",.087f,.46f,.025f,.80f);Block("galpão / fundo",.36f,.055f,.55f,.025f);Block("galpão / frente",.36f,.825f,.55f,.025f);
                Block("palete 1",.276f,.29f,.12f,.17f);Block("palete 2",.477f,.29f,.13f,.17f);Block("palete 3",.276f,.512f,.12f,.17f);Block("palete 4",.477f,.512f,.13f,.17f);
                Block("galpão / caixas parede",.13f,.40f,.04f,.49f);Block("galpão / armários fundo",.55f,.12f,.15f,.10f);
                Block("escritório / parede norte",.76f,.05f,.25f,.025f);Block("escritório / parede oeste",.645f,.13f,.02f,.17f);Block("escritório / parede sul",.76f,.30f,.25f,.025f);
                Block("escritório / mesa",.78f,.18f,.09f,.07f);Block("escritório / armários",.75f,.10f,.15f,.075f);Block("pátio / caixas",.865f,.58f,.055f,.32f);
            }
            if(map=="scrap")
            {
                Block("sucata noroeste",.265f,.245f,.25f,.22f);Block("sucata nordeste",.727f,.245f,.25f,.22f);Block("sucata sudoeste",.265f,.60f,.29f,.23f);Block("sucata sudeste",.73f,.60f,.30f,.23f);
                Block("sucata fundo",.50f,.085f,.18f,.08f);Block("guarita",.845f,.095f,.12f,.085f);Block("guindaste esquerdo",.075f,.16f,.055f,.22f);Block("guindaste direito",.93f,.22f,.055f,.28f);
            }
            if(map=="road")
            {
                Block("barreira norte",.5f,.238f,.98f,.035f);Block("barreira sul",.5f,.706f,.98f,.035f);
                int tiles=Mathf.RoundToInt(currentMapSize.x/48);for(int i=0;i<tiles;i++)Block("caixa na rodovia "+i,(i+.58f)/tiles,i%2==0 ? .46f : .58f,.026f/tiles,.043f);
            }
        }
        static void BuildCampaignScenes()
        {
            stages=AuthoredStages();for(int i=0;i<stages.Count;i++)BuildStage(stages[i],i+3);
        }
        static void BuildStage(StageDef stage,int number)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);currentMapSize=stage.map=="office"?new Vector2(36,24):stage.map=="road"?new Vector2(1440,32):new Vector2(48,32);
            var map=stage.map=="office"?office:campaignMaps[stage.map];
            if(stage.scene=="23_Amanhecer")map=Import(Art+"/Campaign/home-dawn.png");
            var sprite=Patch(map,new Rect(0,0,map.width,map.height),stage.scene+" / cenário",stage.map=="road"?48:currentMapSize.x);
            if(stage.map=="road")for(int tile=0;tile<30;tile++)Visual("Rodovia / trecho "+tile,sprite,new Vector2(-720+24+tile*48,0),-5000);
            else{var background=Visual("Cenário ilustrado • "+stage.scene,sprite,Vector2.zero,-5000);background.color=stage.tint;}
            Geometry(stage.map);
            if(stage.map=="mocambo")MocamboSign();
            var level=new GameObject("Missão / "+stage.title).AddComponent<LevelProgress>();level.title=stage.title;level.chapter=stage.act;level.mapTexture=map;level.mapSize=currentMapSize;level.amaralPortrait=Portrait("Amaral");
            level.objectives=stage.steps.ConvertAll(s=>s.text).ToArray();level.objectiveTargets=new Transform[stage.steps.Count];level.showIntroduction=stage.intro!=null;level.introductionLines=stage.intro;
            var mission=level.gameObject.AddComponent<CombatMission>();mission.nextScene=stage.next;mission.stageNumber=number;mission.rules=new MissionRule[stage.steps.Count];
            var player=Player(Free(W(stage.spawn.x,stage.spawn.y)));CameraRig(player.transform);
            if(stage.map!="road")BuildUber(player.transform.position);
            if(stage.map=="road")player.AddComponent<RoadDriver>().ridingSprites=bank.riding;
            if(stage.allies){Ally("Ribeiro",1,player.transform.position+new Vector3(-1,-1));Ally("Cristyan",2,player.transform.position+new Vector3(1,-1));}
            if(stage.scene=="22_Fuga"||stage.scene=="23_Amanhecer")
            {
                var adryan=new GameObject("Adryan / junto de Amaral");adryan.layer=13;adryan.transform.position=Free(player.transform.position+Vector3.right);
                Visual("Adryan",bank.enemies[4],adryan.transform.position,1000,adryan.transform);adryan.AddComponent<CivilianFollower>();
            }
            if(stage.scene=="22_Fuga")
            {
                foreach(var uv in new[]{new Vector2(.27f,.44f),new Vector2(.64f,.59f),new Vector2(.72f,.76f)})
                {
                    var fire=new GameObject("Curto-circuito / perigo");fire.transform.position=Free(W(uv.x,uv.y));fire.AddComponent<FireHazard>();
                    var glow=Visual("Brasa",smokePuff,fire.transform.position,1200,fire.transform);glow.color=new Color(1,.24f,.025f,.65f);glow.transform.localScale=Vector3.one*5;
                }
            }
            for(int index=0;index<stage.steps.Count;index++)
            {
                var spec=stage.steps[index];var rule=new MissionRule{kind=MissionRuleKind.Manual};mission.rules[index]=rule;Transform target;
                if(spec.kind=="talk")
                {
                    int cast=spec.speaker=="Paulo"?0:spec.speaker=="Ribeiro"?1:spec.speaker=="Cristyan"?2:spec.speaker=="Henzo"?3:4;
                    var npc=Npc(spec.speaker,cast,spec.uv.x,spec.uv.y,index,new[]{spec.message});npc.transform.position=Free(npc.transform.position);target=npc.transform;
                }
                else if(spec.kind=="inspect"||spec.kind=="exit")
                {
                    var item=Item(spec.kind=="exit"?"Seguir • "+spec.text:spec.text,spec.uv.x,spec.uv.y,index,spec.speaker,new[]{spec.message??spec.text},spec.kind!="exit",spec.kind=="exit"?stage.next:null);
                    item.transform.position=Free(item.transform.position);if(spec.kind=="exit")item.completesStage=true;target=item.transform;
                    item.keepEvidence=spec.kind=="inspect";
                }
                else if(spec.kind=="puzzle")
                {
                    target=Point(spec.text,spec.uv);target.gameObject.layer=10;target.gameObject.AddComponent<CircleCollider2D>().isTrigger=true;
                    var puzzle=target.gameObject.AddComponent<EvidencePuzzle>();puzzle.question=spec.message;puzzle.options=spec.choices;puzzle.correctIndex=spec.correct;puzzle.requiredStep=index;
                }
                else if(spec.kind=="photo")
                {
                    var photo=Photo(spec.text,spec.uv.x,spec.uv.y,index);photo.transform.position=Free(photo.transform.position);photo.completedMessage=spec.message??"A prova foi registrada. Precisamos juntar todas as rotas.";target=photo.transform;
                }
                else
                {
                    target=Point(spec.text,spec.uv);rule.target=target;
                    if(spec.kind=="reach")rule.kind=MissionRuleKind.Reach;
                    if(spec.kind=="fight")
                    {
                        rule.kind=MissionRuleKind.Fight;var enemies=new List<EnemyAgent>();
                        for(int e=0;e<spec.roles.Length;e++)
                        {
                            float angle=e*2.399963f;var point=(Vector2)target.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(1.8f+(e%3)*1.1f);
                            var enemy=Enemy(spec.roles[e],point,stage.scene+":wave:"+index+":"+e,number);enemies.Add(enemy);enemy.gameObject.SetActive(false);
                        }
                        rule.enemies=enemies.ToArray();rule.arrivalMessage="Flash atordoa; F neutraliza de perto. Esquive dos ataques anunciados.";
                    }
                    if(spec.kind=="sequence")CreateSequence(spec.sequence,index,target,stage.map);
                }
                level.objectiveTargets[index]=target;
            }
            if(stage.map=="workshop")
            {
                var seller=GameObject.Find("Henzo")?.GetComponent<StoryInteractable>();
                if(!seller)seller=Npc("Henzo Ratão",3,.79f,.45f,-1,new[]{"Tenho o que vocês precisam."},false);
                var shop=seller.gameObject.AddComponent<EquipmentShop>();shop.storyStep=seller.requiredStep;shop.storyLines=seller.lines;shop.portrait=Portrait("Henzo");UnityEngine.Object.DestroyImmediate(seller);
            }
            for(int k=0;k<2;k++)
            {
                var p=Free(W(.49f+k*.12f,.64f-k*.27f));var med=new GameObject("Kit de primeiros socorros");med.transform.position=p;
                var icon=Visual("Kit / indicador",uiArt.icons[4],p,1300,med.transform);icon.color=new Color(.3f,1,.5f);icon.transform.localScale=Vector3.one*.8f;
                med.AddComponent<MedkitPickup>().pickupId=stage.scene+":kit:"+k;
            }
            var hud=new GameObject("HUD / combate e campanha").AddComponent<GameHud>();hud.input=player.GetComponent<PlayerInputReader>();hud.motor=player.GetComponent<PlayerMotor>();hud.interaction=player.GetComponent<Interaction>();hud.level=level;hud.art=uiArt;
            MissionProps();
            if(stage.map!="road")new GameObject("Navegação local A estrela").AddComponent<GridNavigator>();new GameObject("Áudio").AddComponent<OfficeAudio>();new GameObject("Fim da campanha").AddComponent<CampaignEnding>();
            new GameObject("Visual QA").AddComponent<PrototypeCapture>();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/Scenes/"+stage.scene+".unity");
        }
        static void CreateSequence(SequenceKind kind,int step,Transform point,string map)
        {
            var sequence=new GameObject("Encenação em gameplay / "+kind).AddComponent<GameplaySequence>();sequence.kind=kind;sequence.triggerStep=step;sequence.triggerPoint=point;
            sequence.smokeSprite=smokePuff;
            if(kind==SequenceKind.Kidnapping)
            {
                sequence.vehicle=Vehicle(1,W(.62f,.48f),"Gol branco / Cenoura");sequence.vehicleExit=Point("Fuga do Gol",new Vector2(.96f,.48f));
                var actor=new GameObject("Adryan / sequestro");actor.transform.position=W(.53f,.35f);Visual("Adryan",bank.enemies[4],actor.transform.position,1100,actor.transform);sequence.actor=actor.transform;sequence.actorGoal=sequence.vehicle;
            }
            else if(kind==SequenceKind.MartinsRescue)
            {
                sequence.vehicle=Vehicle(5,W(.08f,.50f),"Ninja 300 / Martins");sequence.vehicleExit=Point("Resgate da Ninja",new Vector2(.60f,.50f));sequence.speed=5;
                Visual("Martins pilotando",bank.cast[4],sequence.vehicle.position+Vector3.up*.4f,1300,sequence.vehicle).transform.localScale=Vector3.one*.65f;
            }
            else
            {
                var actor=new GameObject("Adryan / resgatado");actor.transform.position=point.position;Visual("Adryan",bank.enemies[4],point.position,1200,actor.transform);sequence.actor=actor.transform;
            }
        }
    }
}
