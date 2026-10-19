using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
namespace PontoCego.Editor
{
    /// <summary>Authors the supplied visual designs into real Unity sprites, animation and collision geometry.</summary>
    public static partial class UrbanSceneBuilder
    {
        public const string Root="Assets/PontoCego/Urban";
        const string Art="Assets/PontoCego/UrbanArt";
        static UrbanSpriteBank bank;static AnimatorController controller;static MovementSettings tuning;static InputActionAsset bindings;
        static Texture2D office,gala;static Sprite ring,shadow;
        static Vector2 currentMapSize=new Vector2(36,24);
        static readonly Vector2[] directions={Vector2.down,new Vector2(-1,-1).normalized,Vector2.left,new Vector2(-1,1).normalized,Vector2.up,new Vector2(1,1).normalized,Vector2.right,new Vector2(1,-1).normalized};

        public static void Build()
        {
            Directory.CreateDirectory(Root+"/Scenes");Directory.CreateDirectory(Root+"/Animation");AssetDatabase.Refresh();
            Layers();office=Import(Art+"/riberal-painted-v2.png");gala=Import(Art+"/gala-painted-v2.png");
            foreach(var f in Directory.GetFiles(Art+"/Portraits","*.png"))Import(f.Replace('\\','/'));
            Import(Art+"/opening-storyboard.png");
            var walk=Import(Art+"/amaral-walk-v2.png");var cast=Import(Art+"/cast-v2.png");
            if(AssetDatabase.LoadAssetAtPath<UrbanSpriteBank>(Root+"/Sprites.asset"))AssetDatabase.DeleteAsset(Root+"/Sprites.asset");
            bank=ScriptableObject.CreateInstance<UrbanSpriteBank>();AssetDatabase.CreateAsset(bank,Root+"/Sprites.asset");
            bank.amaralWalk=Slice(walk,4,8,2.05f,"Amaral");bank.cast=Slice(cast,3,2,2.05f,"Cast");
            bank.amaralMelee=Slice(Import(Art+"/amaral-combat-v3.png"),4,8,2.05f,"AmaralCombat",true);
            LoadCampaignArt();LoadCleanUi();
            ring=Procedural("Aim ring",64,(x,y)=>{float r=new Vector2(x-31.5f,y-31.5f).magnitude;return r>24&&r<28?Color.white:Color.clear;});
            shadow=Procedural("Foot shadow",64,(x,y)=>{float d=new Vector2((x-32)/30f,(y-32)/12f).magnitude;return new Color(0,0,0,Mathf.Max(0,1-d)*.32f);});
            tuning=AssetDatabase.LoadAssetAtPath<MovementSettings>("Assets/PontoCego/Generated/Movement.asset");
            if(!tuning){tuning=ScriptableObject.CreateInstance<MovementSettings>();AssetDatabase.CreateAsset(tuning,Root+"/Movement.asset");}
            tuning.acceleration=45;tuning.deceleration=62;tuning.turnAcceleration=78;tuning.sprintCapacity=3.8f;tuning.sprintRecovery=1.25f;
            tuning.dodgeDuration=.22f;tuning.dodgeSpeed=11.5f;tuning.dodgeCooldown=.7f;EditorUtility.SetDirty(tuning);
            var defaults=PlayerInputReader.CreateDefaults();File.WriteAllText(Root+"/PontoCego.inputactions",defaults.ToJson());UnityEngine.Object.DestroyImmediate(defaults);
            AssetDatabase.ImportAsset(Root+"/PontoCego.inputactions");bindings=AssetDatabase.LoadAssetAtPath<InputActionAsset>(Root+"/PontoCego.inputactions");
            controller=Animator();Scene(0);Scene(1);Scene(2);
            BuildCampaignScenes();BuildNeighborhood();BuildContinuousCity();BuildMainMenu();
            EditorBuildSettings.scenes=CampaignBuildScenes();
            PlayerSettings.companyName="Amaral Studio";PlayerSettings.productName="Ponto Cego";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.runInBackground=false;PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(Root+"/Scenes/01_Riberal.unity");
            Debug.Log("PONTO_CEGO_URBAN_BUILD_OK • 24 missions / open neighborhood / finite resources / drivable cars");
        }
        static Texture2D Import(string path)
        {
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.isReadable=true;importer.mipmapEnabled=false;
            // Unity's default NPOT rescaling distorts 3:2 maps and breaks pixel-aligned occlusion/collision.
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.alphaIsTransparency=true;importer.maxTextureSize=4096;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Sprite[] Slice(Texture2D texture,int cols,int rows,float height,string prefix,bool isolate=false)
        {
            var sprites=new Sprite[cols*rows];int cw=texture.width/cols,ch=texture.height/rows;var pixels=texture.GetPixels32();
            float ppu=ch*.91f/height;
            for(int row=0;row<rows;row++)for(int col=0;col<cols;col++)
            {
                int x0=col*cw,y0=texture.height-(row+1)*ch,minX=cw,minY=ch,maxX=0,maxY=0;
                for(int y=0;y<ch;y++)for(int x=0;x<cw;x++)if(pixels[(y+y0)*texture.width+x+x0].a>40)
                {minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
                if(isolate)
                {
                    // Select the main silhouette when an adjacent atlas cell contains a tiny overhang.
                    // This changes sprite rectangles only; source art is preserved.
                    var visited=new bool[cw*ch];var queue=new System.Collections.Generic.Queue<int>();int largest=0;
                    for(int seed=0;seed<visited.Length;seed++)
                    {
                        int sx=seed%cw,sy=seed/cw;if(visited[seed]||pixels[(sy+y0)*texture.width+sx+x0].a<=40)continue;
                        queue.Enqueue(seed);visited[seed]=true;int count=0,l=cw,b=ch,r=0,t=0;
                        while(queue.Count>0)
                        {
                            int at=queue.Dequeue(),x=at%cw,y=at/cw;count++;l=Mathf.Min(l,x);b=Mathf.Min(b,y);r=Mathf.Max(r,x);t=Mathf.Max(t,y);
                            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                            {
                                int nx=x+dx,ny=y+dy;if(nx<0||ny<0||nx>=cw||ny>=ch)continue;int ni=ny*cw+nx;
                                if(visited[ni]||pixels[(ny+y0)*texture.width+nx+x0].a<=40)continue;visited[ni]=true;queue.Enqueue(ni);
                            }
                        }
                        if(count>largest){largest=count;minX=Mathf.Max(0,l-1);minY=Mathf.Max(0,b-1);maxX=Mathf.Min(cw-1,r+1);maxY=Mathf.Min(ch-1,t+1);}
                    }
                }
                if(minX>maxX)throw new Exception("Empty sprite cell "+prefix+row+":"+col);
                int width=maxX-minX+1,h=maxY-minY+1;
                var sprite=Sprite.Create(texture,new Rect(x0+minX,y0+minY,width,h),new Vector2(Mathf.Clamp01((cw*.5f-minX)/width),0),ppu,0,SpriteMeshType.FullRect);
                sprite.name=prefix+"_"+row+"_"+col;AssetDatabase.AddObjectToAsset(sprite,bank);sprites[row*cols+col]=sprite;
            }
            return sprites;
        }
        static Sprite Procedural(string name,int n,Func<int,int,Color> pixel)
        {
            var t=new Texture2D(n,n,TextureFormat.RGBA32,false){name=name};
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)t.SetPixel(x,y,pixel(x,y));t.Apply();AssetDatabase.AddObjectToAsset(t,bank);
            var s=Sprite.Create(t,new Rect(0,0,n,n),new Vector2(.5f,.5f),64);s.name=name;AssetDatabase.AddObjectToAsset(s,bank);return s;
        }
        static Sprite Patch(Texture2D tex,Rect rect,string name,float mapWidth=36)
        {
            var s=Sprite.Create(tex,rect,new Vector2(.5f,.5f),tex.width/mapWidth,0,SpriteMeshType.FullRect);s.name=name;AssetDatabase.AddObjectToAsset(s,bank);return s;
        }
        static AnimatorController Animator()
        {
            string path=Root+"/Animation/Amaral.controller";if(AssetDatabase.LoadAssetAtPath<AnimatorController>(path))AssetDatabase.DeleteAsset(path);
            var c=AnimatorController.CreateAnimatorControllerAtPath(path);
            foreach(string key in new[]{"FaceX","FaceY","Speed"})c.AddParameter(key,AnimatorControllerParameterType.Float);
            c.AddParameter("Sprint",AnimatorControllerParameterType.Bool);c.AddParameter("Dodge",AnimatorControllerParameterType.Bool);
            var sm=c.layers[0].stateMachine;
            foreach(string name in new[]{"Idle","Walk","Run","Dodge"})
            {
                var state=sm.AddState(name);var tree=new BlendTree{name=name+" / 8 directions",blendType=BlendTreeType.SimpleDirectional2D,blendParameter="FaceX",blendParameterY="FaceY"};
                AssetDatabase.AddObjectToAsset(tree,c);state.motion=tree;
                for(int d=0;d<8;d++)
                {
                    int frames=name=="Idle"?1:4;float fps=name=="Run"?12:name=="Dodge"?22:8;
                    var clip=new AnimationClip{name=name+"_"+d,frameRate=fps};var keys=new ObjectReferenceKeyframe[frames+1];
                    for(int i=0;i<frames;i++)keys[i]=new ObjectReferenceKeyframe{time=i/fps,value=bank.amaralWalk[d*4+(name=="Idle"?1:i)]};
                    keys[frames]=new ObjectReferenceKeyframe{time=frames/fps,value=keys[0].value};
                    AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys);
                    clip.SetCurve("",typeof(Transform),"localScale.x",AnimationCurve.Constant(0,frames/fps,name=="Dodge"?1.08f:1));
                    clip.SetCurve("",typeof(Transform),"localScale.y",AnimationCurve.Constant(0,frames/fps,name=="Dodge" ? .9f : 1));
                    var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
                    string cp=Root+"/Animation/"+clip.name+".anim";if(AssetDatabase.LoadAssetAtPath<AnimationClip>(cp))AssetDatabase.DeleteAsset(cp);AssetDatabase.CreateAsset(clip,cp);tree.AddChild(clip,directions[d]);
                }
                var tr=sm.AddAnyStateTransition(state);tr.hasExitTime=false;tr.duration=.025f;tr.canTransitionToSelf=false;
                tr.AddCondition(name=="Dodge"?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"Dodge");
                if(name!="Dodge")
                {
                    tr.AddCondition(name=="Idle"?AnimatorConditionMode.Less:AnimatorConditionMode.Greater,.05f,"Speed");
                    if(name!="Idle")tr.AddCondition(name=="Run"?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"Sprint");
                }
                if(name=="Idle")sm.defaultState=state;
            }
            return c;
        }
        static void Layers()
        {
            var serialized=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var a=serialized.FindProperty("layers");
            a.GetArrayElementAtIndex(8).stringValue="World";a.GetArrayElementAtIndex(9).stringValue="Player";a.GetArrayElementAtIndex(10).stringValue="Interactable";a.GetArrayElementAtIndex(11).stringValue="CameraBounds";serialized.ApplyModifiedProperties();
            a.GetArrayElementAtIndex(12).stringValue="Enemy";a.GetArrayElementAtIndex(13).stringValue="Ally";a.GetArrayElementAtIndex(14).stringValue="Projectile";serialized.ApplyModifiedProperties();
            Physics2D.IgnoreLayerCollision(9,11,true);Physics2D.IgnoreLayerCollision(8,11,true);
            Physics2D.IgnoreLayerCollision(12,11,true);Physics2D.IgnoreLayerCollision(13,11,true);Physics2D.IgnoreLayerCollision(9,13,true);Physics2D.IgnoreLayerCollision(13,13,true);
        }
        static Vector2 W(float u,float v)=>new Vector2((u-.5f)*currentMapSize.x,(.5f-v)*currentMapSize.y);
        static SpriteRenderer Visual(string name,Sprite sprite,Vector2 p,int order,Transform parent=null)
        {
            var go=new GameObject(name);go.transform.position=p;if(parent)go.transform.SetParent(parent,true);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=order;return sr;
        }
        static void Block(string name,float u,float v,float width,float height)
        {
            var go=new GameObject("Colisão / "+name);go.layer=8;go.transform.position=W(u,v);go.AddComponent<BoxCollider2D>().size=new Vector2(width*currentMapSize.x,height*currentMapSize.y);
        }
        static void Overlay(Texture2D tex,string name,float x,float y,float w,float h)
        {
            var s=Patch(tex,new Rect(x,tex.height-y-h,w,h),name);
            float bottom=.5f-(y+h)/tex.height;int order=1000-Mathf.RoundToInt(bottom*24*100);
            Visual("Oclusão / "+name,s,W((x+w*.5f)/tex.width,(y+h*.5f)/tex.height),order);
        }
        static Texture2D Portrait(string name)=>AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Portraits/"+name+".png");
        static StoryInteractable Item(string name,float u,float v,int step,string speaker,string[] lines,bool advance=true,string next=null)
        {
            var go=new GameObject(name);go.layer=10;go.transform.position=W(u,v);
            var circle=go.AddComponent<CircleCollider2D>();circle.isTrigger=true;circle.radius=.18f;
            var item=go.AddComponent<StoryInteractable>();item.displayName=name;item.speaker=speaker;item.lines=lines;item.requiredStep=step;item.advancesObjective=advance;item.nextScene=next;
            item.portrait=Portrait(speaker);return item;
        }
        static StoryInteractable Npc(string name,int castIndex,float u,float v,int step,string[] lines,bool advance=true)
        {
            var item=Item(name,u,v,step,name,lines,advance);var sr=Visual(name+" / sprite fiel à referência",bank.cast[castIndex],item.transform.position,1000,item.transform);
            var idle=item.gameObject.AddComponent<NpcIdle>();idle.visual=sr;
            var body=item.gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Static;
            var col=item.gameObject.AddComponent<CircleCollider2D>();col.radius=.25f;
            Visual(name+" / sombra",shadow,item.transform.position+Vector3.up*.06f,-2000,item.transform).transform.localScale=new Vector3(1.3f,1,1);
            return item;
        }
        static PhotoTarget Photo(string name,float u,float v,int step,bool reveals=false)
        {
            var go=new GameObject(name);go.transform.position=W(u,v);var photo=go.AddComponent<PhotoTarget>();photo.requiredStep=step;photo.revealsCrime=reveals;return photo;
        }
        static void OfficeGeometry()
        {
            Block("parede do fundo",.5f,.022f,.96f,.025f);Block("lateral esquerda",.021f,.48f,.022f,.92f);Block("lateral direita",.979f,.48f,.022f,.92f);Block("frente",.5f,.936f,.98f,.025f);
            Block("recepção / painel",.112f,.407f,.164f,.145f);Block("balcão",.112f,.515f,.17f,.095f);
            Block("sofá",.141f,.707f,.16f,.12f);Block("mesa baixa",.143f,.8f,.08f,.075f);Block("poltrona",.064f,.82f,.06f,.09f);
            Block("mesa criação 1",.344f,.445f,.163f,.128f);Block("cadeira 1 frente",.35f,.533f,.045f,.05f);Block("cadeira 1 fundo",.322f,.36f,.045f,.05f);
            Block("mesa criação 2",.566f,.442f,.15f,.13f);Block("cadeira 2 frente",.569f,.516f,.05f,.04f);Block("cadeira 2 fundo",.559f,.355f,.04f,.05f);
            Block("mesa criação 3",.343f,.63f,.16f,.13f);Block("cadeira 3 frente",.346f,.7f,.045f,.05f);Block("cadeira 3 fundo",.348f,.555f,.045f,.04f);
            Block("acervo / cases",.932f,.665f,.09f,.44f);Block("copa / bancada",.513f,.09f,.206f,.13f);Block("copa / mesa",.522f,.2f,.11f,.11f);
            Block("frigorífico",.411f,.09f,.045f,.12f);Block("softbox esquerdo",.13f,.17f,.06f,.09f);Block("softbox direito",.325f,.17f,.055f,.09f);Block("tripé",.10f,.245f,.05f,.09f);
            Block("parede diretoria norte",.675f,.135f,.018f,.25f);Block("parede diretoria sul",.675f,.392f,.018f,.09f);
            Block("vidro diretoria",.825f,.417f,.295f,.035f);Block("mesa Paulo",.819f,.239f,.15f,.105f);Block("armário Paulo",.809f,.09f,.2f,.10f);
            Block("vaso entrada esquerdo",.382f,.838f,.055f,.09f);Block("vaso entrada direito",.616f,.836f,.06f,.09f);
            Overlay(office,"mesa criação 1",405,393,244,132);Overlay(office,"mesa criação 2",760,389,215,139);Overlay(office,"mesa criação 3",399,578,250,137);
            Overlay(office,"vidro diretoria",1040,355,445,100);Overlay(office,"mesa diretoria",1141,184,236,114);
        }
        static void GalaGeometry()
        {
            Block("parede norte",.5f,.03f,.94f,.025f);Block("parede esquerda",.025f,.45f,.025f,.84f);Block("parede direita",.973f,.45f,.025f,.84f);Block("frente do evento",.5f,.945f,.94f,.025f);
            Block("bar do fundo",.36f,.111f,.205f,.072f);Block("buffet",.076f,.36f,.07f,.32f);
            Block("mesa gala 1",.23f,.292f,.12f,.17f);Block("mesa gala 2",.503f,.306f,.12f,.17f);Block("mesa gala 3",.216f,.506f,.12f,.18f);
            Block("divisória restrita norte",.66f,.22f,.02f,.34f);Block("divisória restrita sul",.66f,.545f,.02f,.1f);
            Block("mesa de contrabando",.842f,.285f,.145f,.12f);Block("equipamento restrito",.925f,.47f,.10f,.12f);
            Block("parede serviço",.824f,.608f,.3f,.026f);Block("caixas serviço",.93f,.735f,.09f,.12f);
            Overlay(gala,"mesa da maleta",1193,227,213,129);
        }
        static void Scene(int kind)
        {
            currentMapSize=new Vector2(36,24);
            bool isGala=kind==2,lab=kind==0;var tex=isGala?gala:office;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var background=Patch(tex,new Rect(0,0,tex.width,tex.height),isGala?"Gala painted map":"Riberal painted map");
            Visual("Cenário ilustrado • direção urbana",background,Vector2.zero,-5000);
            if(isGala)GalaGeometry();else OfficeGeometry();
            var level=new GameObject("Roteiro / progressão linear").AddComponent<LevelProgress>();level.mapTexture=tex;level.amaralPortrait=Portrait("Amaral");
            level.title=isGala?"O click fatal":lab?"Estúdio de testes":"A rotina na Riberal";level.chapter=isGala?"ATO 1":lab?"EXTRAS":"PRÓLOGO";level.showIntroduction=!isGala&&!lab;
            if(!isGala)
            {
                var clock=Item("Bater ponto",.218f,.405f,lab?-1:0,"Amaral",new[]{"Mais um expediente. Tenho que juntar dinheiro para cuidar do Adryan. Vamos nessa."},!lab);
                var paulo=Npc("Paulo",0,.826f,.331f,lab?-1:1,new[]{"Amaral, você cobre a gala da alta sociedade esta noite. Quero todas as fotos entregues. Sem desculpas e sem atraso.","E confira esse equipamento. A Riberal não pode passar vergonha na frente dos clientes."},!lab);
                var ribeiro=Npc("Ribeiro",1,.45f,.58f,lab?-1:2,new[]{"Mais uma cobrança do Paulo? Esse cara não desliga nunca. E o Adryan, tá bem?","Se apertar, conta comigo, Amaral. A gente se ajuda por aqui."},!lab);
                var cristyan=Npc("Cristyan",2,.595f,.61f,lab?-1:3,new[]{"A lente nova chegou. Deixei com os equipamentos do acervo. Testa o foco antes de fotografar o evento.","Boa sorte com os clientes de hoje. E com o chefe também."},!lab);
                var lens=Item("Pegar a lente nova",.867f,.635f,lab?-1:4,"Amaral",new[]{"Lente nova na mochila. O cachê dessa cobertura vai ajudar em casa. Agora é partir para a gala."},!lab);
                Item("Ver mensagem de Adryan",.535f,.73f,-1,"Amaral",new[]{"Adryan mandou mensagem perguntando se volto cedo. Respondo que tenho um evento para fotografar e que volto assim que terminar."},false);
                var exit=Item("Sair para o evento de gala",.5f,.85f,lab?-1:5,"Amaral",new[]{"Fim do expediente. Mais uma noite atrás da câmera. Patos de Minas está tranquila... por enquanto."},false,"02_Gala");
                level.objectives=lab?new[]{"Teste oito direções, corrida, esquiva e interações."}:new[]{"Bata o ponto na recepção.","Receba as instruções de Paulo na diretoria.","Converse com Ribeiro no corredor.","Converse com Cristyan na produção.","Pegue a lente nova no acervo.","Saia para cobrir o evento de gala."};
                level.objectiveTargets=lab?new Transform[0]:new[]{clock.transform,paulo.transform,ribeiro.transform,cristyan.transform,lens.transform,exit.transform};
            }
            else
            {
                var reception=Photo("Cobertura / recepção da gala",.36f,.5f,0);
                var deal=Photo("Flagrante / Paulo e a maleta",.803f,.477f,1,true);
                Npc("Paulo",0,.78f,.473f,-1,new[]{"Esta área está fechada para funcionários do evento."},false);
                Npc("Kevyn",5,.838f,.486f,-1,new[]{"Circulando. Não tem nada para fotografar aqui."},false).portrait=Portrait("Kevyn");
                var exit=Item("Fugir e voltar para casa",.36f,.82f,2,"Amaral",new[]{"O flash! Eles me viram. Tenho que sair daqui e chegar em casa."},false);exit.endsOpening=true;
                level.objectives=new[]{"Fotografe a recepção do evento.","Teste o foco perto da área restrita.","O flash denunciou você. Fuja pela entrada!"};level.objectiveTargets=new[]{reception.transform,deal.transform,exit.transform};
            }
            var player=Player(isGala?W(.36f,.77f):W(.49f,.785f));
            CameraRig(player.transform);if(!lab)BuildUber(player.transform.position);
            var hud=new GameObject("HUD / identidade e roteiro").AddComponent<GameHud>();hud.input=player.GetComponent<PlayerInputReader>();hud.motor=player.GetComponent<PlayerMotor>();hud.interaction=player.GetComponent<Interaction>();hud.level=level;hud.art=uiArt;
            if(isGala)ConfigureGalaEscape(level);
            new GameObject("Navegação da fase").AddComponent<GridNavigator>();
            new GameObject("Ambiência e passos").AddComponent<OfficeAudio>();new GameObject("Visual QA (opt-in)").AddComponent<PrototypeCapture>();
            if(!lab)MissionProps();
            PrefabUtility.SaveAsPrefabAsset(player,Root+"/Amaral.prefab");
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/Scenes/"+(isGala?"02_Gala":lab?"00_Laboratorio":"01_Riberal")+".unity");
        }
        static GameObject Player(Vector2 position)
        {
            var go=new GameObject("Amaral");go.layer=9;go.transform.position=position;
            var input=go.AddComponent<PlayerInputReader>();input.bindings=bindings;
            var rb=go.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.freezeRotation=true;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var col=go.AddComponent<CircleCollider2D>();col.radius=.23f;col.offset=new Vector2(0,.10f);
            var mat=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Root+"/Amaral.physicsMaterial2D");if(!mat){mat=new PhysicsMaterial2D("Pés / sem atrito"){friction=0,bounciness=0};AssetDatabase.CreateAsset(mat,Root+"/Amaral.physicsMaterial2D");}col.sharedMaterial=mat;
            var motor=go.AddComponent<PlayerMotor>();motor.settings=tuning;
            var sr=Visual("Amaral / 32 sprites desenhados",bank.amaralWalk[1],position,1000,go.transform);
            var anim=sr.gameObject.AddComponent<UnityEngine.Animator>();anim.runtimeAnimatorController=controller;
            var directional=go.AddComponent<DirectionalAnimation>();directional.animator=anim;directional.bodySprite=sr;
            Visual("Sombra nos pés",shadow,position+Vector2.up*.04f,-2000,go.transform).transform.localScale=new Vector3(1.3f,1,1);
            var reticle=Visual("Mira independente",ring,position,4500,go.transform);reticle.transform.localScale=Vector3.one*.3f;
            var prop=Visual("Âncora câmera • câmera já desenhada no sprite",ring,position,4500,go.transform);prop.color=Color.clear;
            var aim=go.AddComponent<PlayerAim>();aim.reticle=reticle.transform;aim.cameraProp=prop.transform;
            go.AddComponent<StoryDialogue>();go.AddComponent<Interaction>();go.AddComponent<PlayerPhone>();go.AddComponent<CameraViewfinder>();go.AddComponent<MobileControls>().input=input;
            go.AddComponent<Health>().team=CombatTeam.Player;go.AddComponent<CombatPlayer>();go.AddComponent<AmmoInventory>().bullet=shotSprite;go.AddComponent<CampaignClock>();
            var strikes=go.AddComponent<DirectionalCombatSprites>();strikes.body=sr;strikes.poses=bank.amaralMelee;strikes.locomotion=bank.amaralWalk;
            return go;
        }
        static void CameraRig(Transform player)
        {
            var go=new GameObject("Main Camera");go.tag="MainCamera";go.transform.position=(Vector3)player.position+Vector3.back*10;
            var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=6.2f;camera.backgroundColor=new Color(.025f,.035f,.05f);camera.clearFlags=CameraClearFlags.SolidColor;
            go.AddComponent<AudioListener>();go.AddComponent<CinemachineBrain>();
            var bounds=new GameObject("Limites da fase • sem mundo aberto");bounds.layer=11;var polygon=bounds.AddComponent<PolygonCollider2D>();polygon.isTrigger=true;
            float bx=currentMapSize.x*.5f-.1f,by=currentMapSize.y*.5f-.1f;
            polygon.points=new[]{new Vector2(-bx,-by),new Vector2(bx,-by),new Vector2(bx,by),new Vector2(-bx,by)};
            var cmgo=new GameObject("Cinemachine / câmera elevada 2D");cmgo.transform.position=go.transform.position;
            var cm=cmgo.AddComponent<CinemachineCamera>();cm.Follow=player;cm.Lens.OrthographicSize=6.2f;cm.Lens.ModeOverride=LensSettings.OverrideModes.Orthographic;
            var composer=cmgo.AddComponent<CinemachinePositionComposer>();composer.CameraDistance=10;composer.Damping=new Vector3(.24f,.24f,0);
            var confiner=cmgo.AddComponent<CinemachineConfiner2D>();confiner.BoundingShape2D=polygon;confiner.Damping=.1f;
        }
    }
}
