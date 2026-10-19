using System;using System.IO;using System.Linq;using System.Collections.Generic;
using UnityEngine;using UnityEngine.AI;using UnityEngine.InputSystem;using Unity.Cinemachine;
using UnityEditor;using UnityEditor.Animations;using UnityEditor.SceneManagement;using UnityEditor.Build.Reporting;
namespace PontoCego3D.Editor
{
    public static partial class DistrictBuilder
    {
        const string Root="Assets/PontoCego3D",Gen=Root+"/Generated";public const string ScenePath=Gen+"/CentroAberto.unity";
        static Dictionary<string,Material> mats;static AnimatorController animator;static InputActionAsset input;static Mesh male,female;
        static Material skin,shirt,pants,shoes,hair,glass,city;static List<Vector3> sidewalks;static System.Random random;
        [MenuItem("Ponto Cego/3D • Gerar bairro aberto")]
        public static void Build()
        {
            Directory.CreateDirectory(Gen+"/Materials");Directory.CreateDirectory(Gen+"/Animation");Directory.CreateDirectory(Gen+"/Meshes");AssetDatabase.Refresh();
            mats=new Dictionary<string,Material>();random=new System.Random(713);sidewalks=new List<Vector3>();
            skin=Mat("Pele Amaral",new Color(.66f,.40f,.25f));shirt=Mat("Camiseta carvão",new Color(.095f,.11f,.125f));pants=Mat("Calça cargo bege",new Color(.59f,.52f,.36f));shoes=Mat("Tênis",new Color(.09f,.10f,.11f));hair=Mat("Cabelo castanho",new Color(.085f,.045f,.022f));glass=Mat("Lentes",new Color(.11f,.17f,.20f));
            city=Mat("Cidade / Kenney CC0",Color.white);city.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/City/Textures/colormap.png");
            animator=CreateAnimator();input=CreateInput();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.53f,.65f,.80f);RenderSettings.ambientEquatorColor=new Color(.51f,.48f,.41f);RenderSettings.ambientGroundColor=new Color(.22f,.23f,.24f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.64f,.72f,.79f);RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=95;RenderSettings.fogEndDistance=225;
            var sky=Mat("Céu",new Color(.6f,.7f,.85f),"Skybox/Procedural");sky.SetColor("_SkyTint",new Color(.51f,.65f,.84f));sky.SetColor("_GroundColor",new Color(.47f,.42f,.35f));sky.SetFloat("_AtmosphereThickness",.8f);RenderSettings.skybox=sky;
            var sun=new GameObject("Fim da tarde / sol").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.86f,.67f);sun.intensity=1.25f;sun.shadows=LightShadows.Soft;sun.shadowBias=.035f;sun.transform.rotation=Quaternion.Euler(38,-37,0);RenderSettings.sun=sun;
            QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowDistance=85;QualitySettings.shadowCascades=2;QualitySettings.antiAliasing=4;QualitySettings.vSyncCount=1;
            World();CityDetails();SurfaceDetails();BakeNavigation();
            var district=new GameObject("Ponto Cego / bairro de Patos").AddComponent<OpenDistrict>();district.ui=AssetDatabase.LoadAssetAtPath<PontoCego.GameUiArt>("Assets/PontoCego/Urban/GameUi.asset");district.spawn=new Vector3(7,.3f,-15);
            var hero=Character("Amaral",district.spawn,Role3D.Player,false,0);var reader=hero.AddComponent<ThirdPersonInput>();reader.template=input;
            var controller=hero.AddComponent<CharacterController>();controller.height=1.8f;controller.radius=.28f;controller.center=Vector3.up*.9f;controller.stepOffset=.28f;controller.skinWidth=.035f;
            var motor=hero.AddComponent<ThirdPersonMotor>();
            var actor=hero.GetComponent<CombatActor>();motor.visual=actor.visual;motor.animator=actor.animator;
            var main=new GameObject("Main Camera");main.tag="MainCamera";var camera=main.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=10.5f;camera.nearClipPlane=.08f;camera.farClipPlane=260;main.AddComponent<AudioListener>();main.AddComponent<CinemachineBrain>();
            var rig=new GameObject("Cinemachine / terceira pessoa").AddComponent<CinemachineCamera>();rig.Lens.OrthographicSize=10.5f;rig.Lens.ModeOverride=LensSettings.OverrideModes.Orthographic;rig.Lens.FieldOfView=63;rig.Lens.NearClipPlane=.08f;rig.Lens.FarClipPlane=260;
            var orbit=new GameObject("Órbita / colisão da câmera").AddComponent<OrbitCamera>();orbit.isometric=true;orbit.yaw=-32;orbit.pitch=52;orbit.distance=30;orbit.target=hero.transform;orbit.input=reader;orbit.cameraRig=rig;motor.orbit=orbit;district.player=motor;
            district.goals=new[]{Goal("Riberal / ponto",new Vector3(22,.2f,11.5f)),Goal("Padaria / teste de lente",new Vector3(-22,.2f,-10)),Goal("Oficina / Martins",new Vector3(22,.2f,-11.5f)),Goal("Pátio / capangas",new Vector3(63,.2f,17)),Goal("Documento / rota",new Vector3(63,.2f,22))};
            district.objectives=new[]{"Bata o ponto na entrada da Riberal.","Teste a lente fotografando a fachada da padaria.","Converse com Martins na oficina.","Afaste os capangas do pátio. Use o flash e a esquiva.","Recolha a rota no pátio da distribuidora."};
            var enemies=new List<GameObject>();for(int i=0;i<4;i++)
            {
                var enemy=Character("Capanga "+(i+1),new Vector3(58+i*3,.3f,17+i%2*3),Role3D.Enemy,false,i+1);Pedestrian(enemy,true);enemy.SetActive(false);enemies.Add(enemy);
            }
            district.encounter=enemies.ToArray();
            for(int i=0;i<36;i++)
            {
                Vector3 point=sidewalks[(i*13+5)%sidewalks.Count];if(NavMesh.SamplePosition(point,out var hit,3,NavMesh.AllAreas))point=hit.position;
                var npc=Character(district.names[i%district.names.Length],point,Role3D.Civilian,i%3==0,i+2);Pedestrian(npc,false);
            }
            AddPhoneServices(district);
            new GameObject("Validação 3D / opcional").AddComponent<DistrictCapture>();new GameObject("Apresentação da cidade 3D").AddComponent<CityStartMenu>().art=district.ui.mainMenu;new GameObject("Validação dia e noite").AddComponent<CityEnvironmentCapture>();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),ScenePath);
            var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();Debug.Log("PONTO_CEGO_3D_SCENE_OK / rigged humans, open district, 36 pedestrians, third-person combat");
        }
        static Material Mat(string name,Color color,string shader="Standard")
        {
            if(mats.TryGetValue(name,out var cached))return cached;string path=Gen+"/Materials/"+name.Replace('/','-')+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.color=color;if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",.12f);mats[name]=m;EditorUtility.SetDirty(m);return m;
        }
        static GameObject Box(string name,Vector3 at,Vector3 size,Material mat,bool solid=false,Transform parent=null)=>Part(PrimitiveType.Cube,name,at,size,mat,solid,parent);
        static GameObject Part(PrimitiveType kind,string name,Vector3 at,Vector3 size,Material mat,bool solid=false,Transform parent=null)
        {
            var go=GameObject.CreatePrimitive(kind);go.name=name;go.transform.position=at;go.transform.localScale=size;if(parent)go.transform.SetParent(parent,true);go.GetComponent<Renderer>().sharedMaterial=mat;
            if(!solid)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());else go.layer=8;
            if(!parent)GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);return go;
        }
        static void Sign(string text,Vector3 at,float width,Quaternion rotation,Color color)
        {
            var go=new GameObject(text);go.transform.SetPositionAndRotation(at,rotation);var mesh=go.AddComponent<TextMesh>();mesh.text=text;mesh.anchor=TextAnchor.MiddleCenter;mesh.alignment=TextAlignment.Center;mesh.fontSize=70;mesh.characterSize=.1f;mesh.color=color;
            go.AddComponent<StreetSignFacing>();var renderer=go.GetComponent<MeshRenderer>();float measured=renderer.bounds.size.x;if(rotation.eulerAngles.y>89&&rotation.eulerAngles.y<271)measured=Mathf.Max(measured,renderer.bounds.size.z);
            if(measured>.001f)go.transform.localScale=Vector3.one*(width/measured);
        }
        static void World()
        {
            var asphalt=Mat("Asfalto",new Color(.19f,.20f,.215f));var concrete=Mat("Calçada",new Color(.65f,.63f,.57f));var markings=Mat("Faixa amarela",new Color(.91f,.67f,.23f));var white=Mat("Faixa branca",new Color(.86f,.86f,.79f));var grass=Mat("Vegetação",new Color(.27f,.37f,.18f));
            Box("Pavimento / bairro",new Vector3(0,-.5f,0),new Vector3(224,1,224),asphalt,true);
            for(int ix=-2;ix<2;ix++)for(int iz=-2;iz<2;iz++)
            {
                float x=ix*42+21,z=iz*42+21;Box("Quarteirão / passeio",new Vector3(x,.025f,z),new Vector3(32,.25f,32),concrete,true);
                bool special=(ix==0&&iz==0)||(ix==-1&&iz==-1)||(ix==0&&iz==-1)||(ix==1&&iz==0);
                if(!special)
                {
                    foreach(int a in new[]{-1,1})foreach(int b in new[]{-1,1})Building(new Vector3(x+a*8,0,z+b*8));
                    Box("Canteiro interno",new Vector3(x,.18f,z),new Vector3(4,.12f,7),grass);Tree(new Vector3(x,.2f,z));
                }
                for(int i=0;i<4;i++)
                {
                    float v=-12+i*8;sidewalks.Add(new Vector3(x+v,.2f,z-14.3f));sidewalks.Add(new Vector3(x+v,.2f,z+14.3f));sidewalks.Add(new Vector3(x-14.3f,.2f,z+v));sidewalks.Add(new Vector3(x+14.3f,.2f,z+v));
                }
                Tree(new Vector3(x-13,.2f,z-11));Tree(new Vector3(x+13,.2f,z+11));Lamp(new Vector3(x-14,.2f,z-14));Lamp(new Vector3(x+14,.2f,z+14));
            }
            for(int street=-2;street<=2;street++)for(int segment=-16;segment<=16;segment++)
            {
                float along=segment*6;bool crossing=false;for(int a=-2;a<=2;a++)if(Mathf.Abs(along-a*42)<8)crossing=true;if(crossing)continue;
                Box("Eixo da rua",new Vector3(street*42,.011f,along),new Vector3(.12f,.014f,3),markings);Box("Eixo da avenida",new Vector3(along,.012f,street*42),new Vector3(3,.014f,.12f),markings);
            }
            for(int x=-2;x<=2;x++)for(int z=-2;z<=2;z++)for(int i=-3;i<=3;i++)
            {
                Box("Faixa de pedestres",new Vector3(x*42+i*.9f,.014f,z*42+7.5f),new Vector3(.5f,.015f,2.1f),white);
                Box("Faixa de pedestres",new Vector3(x*42+7.5f,.014f,z*42+i*.9f),new Vector3(2.1f,.015f,.5f),white);
            }
            Agency();Shop("PADARIA PÃO DE MINAS",new Vector3(-22,0,-22),new Color(.72f,.47f,.23f));Shop("MARTINS / OFICINA",new Vector3(22,0,-22),new Color(.29f,.40f,.42f));
            var warehouse=Mat("Galpão / tijolo",new Color(.52f,.39f,.29f));Box("Distribuidora / galpão",new Vector3(63,4,33),new Vector3(28,8,16),warehouse,true);Box("Portão do galpão",new Vector3(63,2.5f,24.94f),new Vector3(7,5,.08f),shoes);Sign("DISTRIBUIDORA",new Vector3(63,6.2f,24.85f),18,Quaternion.identity,Color.white);
            for(int i=0;i<7;i++)Box("Caixote",new Vector3(53+i%2*2,1,22+i/2*2),new Vector3(1.6f,1.8f,1.6f),Mat("Madeira",new Color(.48f,.31f,.16f)),true);
            for(int i=-2;i<2;i++)for(int j=-2;j<2;j++){Car(new Vector3(i*42+3.6f,.05f,j*42+20),(i+j)%2==0?new Color(.75f,.75f,.68f):new Color(.25f,.33f,.43f));}
            foreach(var p in new[]{new Vector3(14,.2f,5.8f),new Vector3(-33,.2f,-8),new Vector3(32,.2f,-7)})Bench(p);
            for(int i=-2;i<=2;i++){Box("Limite bairro",new Vector3(-111,2,i*42),new Vector3(2,4,44),concrete,true);Box("Limite bairro",new Vector3(111,2,i*42),new Vector3(2,4,44),concrete,true);Box("Limite bairro",new Vector3(i*42,2,111),new Vector3(44,4,2),concrete,true);Box("Limite bairro",new Vector3(i*42,2,-111),new Vector3(44,4,2),concrete,true);}
        }
        static void Building(Vector3 at)
        {
            string name="building-"+(char)('a'+random.Next(0,12));var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Art/City/"+name+".fbx");
            if(!source)throw new Exception("Missing city model "+name);var go=UnityEngine.Object.Instantiate(source);go.name="Prédio / "+name;
            var renderers=go.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers){bounds.Encapsulate(r.bounds);r.sharedMaterial=city;}
            float height=7+(float)random.NextDouble()*10;go.transform.localScale=new Vector3(11/bounds.size.x,height/bounds.size.y,11/bounds.size.z);go.transform.position=at+Vector3.up*.15f;
            var col=go.AddComponent<BoxCollider>();col.center=new Vector3(0,bounds.size.y*.5f,0);col.size=new Vector3(bounds.size.x,bounds.size.y,bounds.size.z);go.layer=8;
            GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);
        }
        static void Agency()
        {
            var wall=Mat("Riberal / creme",new Color(.74f,.70f,.61f));var trim=Mat("Riberal / azul",new Color(.12f,.24f,.31f));var windows=Mat("Vidro fachada",new Color(.20f,.34f,.41f));
            Box("Riberal / volume sólido",new Vector3(22,5,26),new Vector3(24,10,23),wall,true);Box("Riberal / marquise",new Vector3(22,3.1f,13.2f),new Vector3(25,.45f,3),trim);
            for(int floor=0;floor<3;floor++)for(int w=0;w<6;w++)Box("Riberal / janela",new Vector3(12+w*4,1.8f+floor*3.1f,14.43f),new Vector3(2.7f,1.9f,.12f),windows);
            Box("Riberal / entrada",new Vector3(22,1.4f,14.3f),new Vector3(3.2f,2.7f,.12f),windows);Box("Riberal / testeira",new Vector3(22,10.1f,26),new Vector3(24,.45f,23),trim);
            Sign("RIBERAL",new Vector3(22,4.1f,14.27f),11,Quaternion.identity,new Color(.98f,.79f,.35f));Sign("MARKETING / FOTOGRAFIA",new Vector3(22,3.5f,14.26f),9,Quaternion.identity,Color.white);
        }
        static void Shop(string name,Vector3 at,Color color)
        {
            var wall=Mat(name,color);Box(name,at+Vector3.up*2.4f,new Vector3(24,4.8f,20),wall,true);Box("Porta / "+name,at+new Vector3(0,1.5f,10.06f),new Vector3(5,3,.1f),glass);
            Box("Vitrine / "+name,at+new Vector3(-6.7f,1.8f,10.07f),new Vector3(6,2.1f,.12f),glass);Box("Vitrine / "+name,at+new Vector3(6.7f,1.8f,10.07f),new Vector3(6,2.1f,.12f),glass);
            Box("Marquise / "+name,at+new Vector3(0,3.3f,11),new Vector3(25,.25f,3),Mat("Telha",new Color(.57f,.22f,.14f)));
            // TextMesh faces -Z by default; rotate to read from the street to the north.
            Sign(name,at+new Vector3(0,4.1f,10.17f),21,Quaternion.Euler(0,180,0),Color.white);
        }
        static void Tree(Vector3 at)
        {
            Part(PrimitiveType.Cylinder,"Tronco",at+Vector3.up*1.7f,new Vector3(.4f,1.7f,.4f),Mat("Tronco",new Color(.32f,.23f,.14f)),true);
            var leaf=Mat("Copa",new Color(.24f,.36f,.18f));Part(PrimitiveType.Sphere,"Copa",at+Vector3.up*4,new Vector3(4.5f,4,4.2f),leaf);
            Part(PrimitiveType.Sphere,"Copa detalhe",at+new Vector3(1.2f,3.5f,.4f),new Vector3(3,2.8f,3.2f),leaf);
        }
        static void Lamp(Vector3 at)
        {
            var metal=Mat("Metal urbano",new Color(.23f,.25f,.24f));Part(PrimitiveType.Cylinder,"Poste",at+Vector3.up*3,new Vector3(.13f,3,.13f),metal,true);
            Box("Braço do poste",at+new Vector3(0,6,.75f),new Vector3(.14f,.14f,1.6f),metal);Box("Luminária",at+new Vector3(0,5.9f,1.45f),new Vector3(.5f,.14f,.8f),Mat("Luz da luminária",new Color(1,.88f,.59f)));
        }
        static void Bench(Vector3 at)
        {
            var wood=Mat("Madeira banco",new Color(.42f,.29f,.18f));Box("Banco / assento",at+Vector3.up*.55f,new Vector3(2.6f,.13f,.75f),wood,true);Box("Banco / encosto",at+new Vector3(0,.95f,.3f),new Vector3(2.6f,.7f,.13f),wood);
            foreach(float x in new[]{-.9f,.9f})Box("Banco / pé",at+new Vector3(x,.27f,0),new Vector3(.12f,.5f,.6f),shoes);
        }
        static void Car(Vector3 at,Color color){CreateVehicle(at,color,false,0);}
        static Transform Goal(string name,Vector3 point){var go=new GameObject(name);go.transform.position=point;return go.transform;}
        static void BakeNavigation()
        {
            var sources=new List<NavMeshBuildSource>();var marks=new List<NavMeshBuildMarkup>();foreach(var moving in UnityEngine.Object.FindObjectsByType<CityVehicle>())marks.Add(new NavMeshBuildMarkup{root=moving.transform,ignoreFromBuild=true});NavMeshBuilder.CollectSources(null,1<<8,NavMeshCollectGeometry.PhysicsColliders,0,marks,sources);
            var settings=NavMesh.GetSettingsByIndex(0);settings.agentRadius=.33f;settings.agentHeight=1.8f;settings.agentClimb=.3f;settings.agentSlope=45;settings.overrideVoxelSize=true;settings.voxelSize=.16f;
            var data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(Vector3.zero,new Vector3(224,50,224)),Vector3.zero,Quaternion.identity);if(!data)throw new Exception("NavMesh generation failed");
            string path=Gen+"/DistrictNavMesh.asset";AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(data,path);var nav=new GameObject("Navegação do bairro").AddComponent<DistrictNavigation>();nav.data=data;
        }
        static void Pedestrian(GameObject go,bool enemy)
        {
            var actor=go.GetComponent<CombatActor>();var agent=go.AddComponent<NavMeshAgent>();agent.radius=.3f;agent.height=1.8f;agent.speed=1.4f;agent.acceleration=12;agent.angularSpeed=0;agent.updateRotation=false;agent.obstacleAvoidanceType=ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            agent.enabled=false;
            var capsule=go.AddComponent<CapsuleCollider>();capsule.center=Vector3.up*.9f;capsule.height=1.8f;capsule.radius=.29f;
            var npc=go.AddComponent<StreetPedestrian>();npc.hostile=enemy;npc.animator=actor.animator;npc.visual=actor.visual;npc.stops=sidewalks.ToArray();
        }
        static GameObject Character(string name,Vector3 at,Role3D role,bool isFemale,int variant)
        {
            var root=new GameObject(name);root.layer=role==Role3D.Player?9:role==Role3D.Enemy?12:13;root.transform.position=at;
            var visual=new GameObject("Visual");visual.transform.SetParent(root.transform,false);
            string source=Root+"/Art/Characters/"+(isFemale?"Female":"Male")+".fbx";var body=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source),visual.transform);body.name="Humano / esqueleto";body.transform.localPosition=Vector3.zero;
            var anim=body.GetComponent<Animator>();if(!anim)anim=body.AddComponent<Animator>();anim.avatar=AssetDatabase.LoadAllAssetsAtPath(source).OfType<Avatar>().First();anim.runtimeAnimatorController=animator;anim.applyRootMotion=false;anim.cullingMode=AnimatorCullingMode.CullUpdateTransforms;
            if(role==Role3D.Player)anim.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var eyeRenderer=body.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Eyes");bool flip=eyeRenderer.bounds.center.z<at.z;
            if(flip)body.transform.localRotation=Quaternion.Euler(0,180,0)*body.transform.localRotation;
            Material top=shirt,bottom=pants,skinTone=skin;
            if(role!=Role3D.Player){float h=(variant*.18f)%1;top=Mat("Roupa "+variant,Color.HSVToRGB(h,.35f,.47f));bottom=Mat("Calça NPC "+variant,Color.HSVToRGB((h+.4f)%1,.18f,.24f));skinTone=Mat("Pele "+variant,Color.Lerp(new Color(.38f,.22f,.14f),new Color(.88f,.68f,.49f),(variant%5)/4f));}
            foreach(var renderer in body.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if(renderer.sharedMesh.vertexCount>1500)
                {
                    Mesh mesh=isFemale?female:male;if(!mesh){mesh=ClothesMesh(renderer.sharedMesh,isFemale);if(isFemale)female=mesh;else male=mesh;}renderer.sharedMesh=mesh;renderer.sharedMaterials=new[]{skinTone,top,bottom,shoes};
                }
                else renderer.sharedMaterial=renderer.name.ToLower().Contains("eye")&&!renderer.name.ToLower().Contains("brow")?glass:hair;
            }
            var head=body.GetComponentsInChildren<Transform>().First(t=>t.name=="Head");
            AttachHair(isFemale?"Hair_Buns":variant%3==0?"Hair_SimpleParted":"Hair_Buzzed",visual.transform,head,flip);
            if(name=="Higor")AttachHair("Hair_Beard",visual.transform,head,flip);
            if(role==Role3D.Player)
            {
                AttachHair("Hair_Beard",visual.transform,head,flip);
                var spine=body.GetComponentsInChildren<Transform>().First(t=>t.name=="spine_03");
                // Accessories are authored in the bind pose and attached to the actual animated bones.
                foreach(float side in new[]{-1f,1f})
                {
                    Vector3 eye=at+new Vector3(side*.036f,1.70f,.105f);
                    Box("Óculos / topo",eye+Vector3.up*.018f,new Vector3(.064f,.009f,.014f),shoes,false,head);
                    Box("Óculos / base",eye-Vector3.up*.018f,new Vector3(.064f,.009f,.014f),shoes,false,head);
                    foreach(float x in new[]{-.031f,.031f})Box("Óculos / aro",eye+Vector3.right*x,new Vector3(.009f,.043f,.014f),shoes,false,head);
                }
                Box("Óculos / ponte",at+new Vector3(0,1.70f,.11f),new Vector3(.024f,.008f,.012f),shoes,false,head);
                Box("Câmera de Amaral",at+new Vector3(0,1.17f,.245f),new Vector3(.24f,.16f,.11f),shoes,false,spine);
                var lens=Part(PrimitiveType.Cylinder,"Lente da câmera",at+new Vector3(.035f,1.18f,.34f),new Vector3(.11f,.08f,.11f),glass,false,spine);lens.transform.rotation=Quaternion.Euler(90,0,0);
                Box("Bolsa fotográfica",at+new Vector3(.22f,.95f,-.15f),new Vector3(.25f,.30f,.18f),shirt,false,spine);
            }
            var actor=root.AddComponent<CombatActor>();actor.role=role;actor.animator=anim;actor.visual=visual.transform;actor.displayName=name;actor.maximum=role==Role3D.Enemy?90:100;
            return root;
        }
        static Mesh ClothesMesh(Mesh source,bool isFemale)
        {
            var mesh=UnityEngine.Object.Instantiate(source);mesh.name=(isFemale?"Female":"Male")+" / roupas";var groups=new List<int>[] {new List<int>(),new List<int>(),new List<int>(),new List<int>()};var v=mesh.vertices;var triangles=mesh.triangles;
            for(int i=0;i<triangles.Length;i+=3)
            {
                Vector3 p=(v[triangles[i]]+v[triangles[i+1]]+v[triangles[i+2]])/3;float h=p.z;int group=h<.13f?3:h<.99f?2:h>1.54f||Mathf.Abs(p.x)>.60f?0:1;
                groups[group].Add(triangles[i]);groups[group].Add(triangles[i+1]);groups[group].Add(triangles[i+2]);
            }
            mesh.subMeshCount=4;for(int i=0;i<4;i++)mesh.SetTriangles(groups[i],i);string path=Gen+"/Meshes/"+(isFemale?"Female":"Male")+".asset";AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static void AttachHair(string file,Transform visual,Transform head,bool flip)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Art/Characters/"+file+".fbx");if(!source)return;
            var go=UnityEngine.Object.Instantiate(source,visual);go.transform.localPosition=Vector3.zero;
            if(flip)go.transform.localRotation=Quaternion.Euler(0,180,0)*go.transform.localRotation;
            foreach(var r in go.GetComponentsInChildren<Renderer>())r.sharedMaterial=hair;go.transform.SetParent(head,true);
        }
        static AnimatorController CreateAnimator()
        {
            string path=Gen+"/Animation/ThirdPerson.controller";AssetDatabase.DeleteAsset(path);var c=AnimatorController.CreateAnimatorControllerAtPath(path);c.AddParameter("Speed",AnimatorControllerParameterType.Float);
            var clips=AssetDatabase.LoadAllAssetsAtPath(Root+"/Art/Characters/Animations.fbx").OfType<AnimationClip>().Where(x=>!x.name.StartsWith("__preview__")).ToArray();
            AnimationClip Get(string name)
            {
                var src=clips.First(x=>x.name.EndsWith("|"+name));var clip=UnityEngine.Object.Instantiate(src);clip.name=name;
                var setting=AnimationUtility.GetAnimationClipSettings(clip);setting.loopTime=name.EndsWith("Loop");setting.keepOriginalOrientation=true;setting.keepOriginalPositionXZ=true;setting.keepOriginalPositionY=true;setting.loopBlendOrientation=true;setting.loopBlendPositionXZ=true;
                AnimationUtility.SetAnimationClipSettings(clip,setting);string p=Gen+"/Animation/"+name+".anim";AssetDatabase.DeleteAsset(p);AssetDatabase.CreateAsset(clip,p);return clip;
            }
            var sm=c.layers[0].stateMachine;var loco=sm.AddState("Locomotion");sm.defaultState=loco;var blend=new BlendTree{name="Idle / caminhada / corrida",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(blend,c);
            blend.AddChild(Get("Idle_Loop"),0);blend.AddChild(Get("Walk_Loop"),1.5f);blend.AddChild(Get("Jog_Fwd_Loop"),3.6f);blend.AddChild(Get("Sprint_Loop"),6.2f);loco.motion=blend;
            foreach(var spec in new[]{("Jab","Punch_Jab",1.4f), ("Cross","Punch_Cross",1.5f),("Hit","Hit_Chest",1f),("Roll","Roll",3f),("Jump","Jump_Start",2f),("Death","Death01",1f),("Talk","Idle_Talking_Loop",1f)})
            {
                var state=sm.AddState(spec.Item1);state.motion=Get(spec.Item2);state.speed=spec.Item3;state.writeDefaultValues=true;
                if(spec.Item1!="Death"){var transition=state.AddTransition(loco);transition.hasExitTime=true;transition.exitTime=.9f;transition.duration=.12f;}
            }
            return c;
        }
        static InputActionAsset CreateInput()
        {
            var asset=ScriptableObject.CreateInstance<InputActionAsset>();var map=new InputActionMap("ThirdPerson");asset.AddActionMap(map);
            var move=map.AddAction("Move",InputActionType.Value);move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");move.AddBinding("<Gamepad>/leftStick");
            map.AddAction("Look",InputActionType.Value).AddBinding("<Mouse>/delta");
            void Button(string name,string key,string pad){var a=map.AddAction(name,InputActionType.Button);a.AddBinding(key);a.AddBinding(pad);}
            Button("Sprint","<Keyboard>/leftShift","<Gamepad>/leftStickPress");Button("Dodge","<Keyboard>/leftCtrl","<Gamepad>/buttonEast");Button("Jump","<Keyboard>/space","<Gamepad>/buttonSouth");Button("Attack","<Mouse>/leftButton","<Gamepad>/buttonWest");Button("Aim","<Mouse>/rightButton","<Gamepad>/leftTrigger");Button("Flash","<Keyboard>/q","<Gamepad>/rightTrigger");Button("Interact","<Keyboard>/e","<Gamepad>/buttonNorth");Button("Heal","<Keyboard>/h","<Gamepad>/dpad/up");Button("Pause","<Keyboard>/escape","<Gamepad>/start");
            string path=Gen+"/ThirdPerson.inputactions";File.WriteAllText(path,asset.ToJson());UnityEngine.Object.DestroyImmediate(asset);AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }
        [MenuItem("Ponto Cego/3D • Build Windows")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory("Builds/Windows3D");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/Windows3D/PontoCego3D.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("3D build failed "+report.summary.result);Debug.Log("PONTO_CEGO_3D_WINDOWS_OK");
        }
        public static void BuildAll(){Build();BuildWindows();}
        public static void Open(){EditorSceneManager.OpenScene(ScenePath);EditorApplication.ExecuteMenuItem("Window/General/Game");Debug.Log("PONTO_CEGO_3D_EDITOR_READY");}
    }
}
