using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        // Exterior districts are authored from separate ground, objects and matching solids.
        // Mission actors and saved coordinates remain owned by the original campaign definitions.
        static Sprite outWhite,outWater,outCourt,outMetal,outWood,outGlow;
        static Sprite[] outGrounds;static readonly List<Vector2> outProtected=new List<Vector2>();
        struct OutMapShape { public Rect rect;public Color color; public bool ellipse; }
        static readonly List<OutMapShape> outMapShapes=new List<OutMapShape>();
        static Rect outArea;static Transform outRoot;

        static void BuildRebuiltOutdoorDistricts(WorldHub world,Sprite[] grounds)
        {
            outGrounds=grounds;
            outWhite=Procedural("Exterior / tinta",8,(x,y)=>Color.white);
            outMetal=Procedural("Exterior / metal escovado",32,(x,y)=>new Color(.35f+(x%6==0?.10f:0),.39f,.40f));
            outWood=Procedural("Exterior / ripas de madeira",32,(x,y)=>new Color(.42f+(y%7==0?-.1f:0),.25f+(x%3)*.008f,.12f));
            outGlow=Procedural("Exterior / luz suave",64,(x,y)=>{float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;return new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2));});
            outWater=Procedural("Mocambo / água de superfície",96,(x,y)=>
            {
                var p=new Vector2((x-47.5f)/47.5f,(y-47.5f)/47.5f);float d=p.magnitude;
                if(d>1)return Color.clear;float ripple=Mathf.Sin(y*.45f+x*.1f)*.025f;
                return d>.92f?new Color(.28f,.42f,.31f):new Color(.10f+ripple,.34f+ripple,.38f+ripple,1);
            });
            outCourt=Procedural("Mocambo / pintura da quadra",96,(x,y)=>{float grain=((x*73856093^y*19349663)&15)*.0014f;return new Color(.65f+grain,.30f+grain,.17f+grain);});
            RebuildMocambo(world);RebuildScrapyard(world);RebuildHighway(world);
            Debug.Log("PONTO_OUTDOORS_REBUILT / Mocambo, ferro-velho e estrada por objetos 2D");
        }
        static bool OutMissionObject(Transform target,WorldHub world)
        {
            foreach(var mission in world.missions)if(mission.actors&&target.IsChildOf(mission.actors.transform))return true;
            return false;
        }
        static void BeginOutdoor(WorldHub world,string id,string label,int ground)
        {
            var mission=world.Find(id);outArea=new Rect(mission.center-mission.size*.5f,mission.size);
            outProtected.Clear();outMapShapes.Clear();
            foreach(var entry in world.missions)
            {
                if(Vector2.Distance(entry.center,mission.center)>.1f)continue;
                outProtected.Add(entry.entry);
                if(entry.level&&entry.level.objectiveTargets!=null)foreach(var goal in entry.level.objectiveTargets)if(goal)outProtected.Add(goal.position);
                if(entry.actors)foreach(var item in entry.actors.GetComponentsInChildren<PcCollectible>(true))outProtected.Add(item.transform.position);
            }
            // Only the static scenery in this district is replaced. Inactive mission templates are untouched.
            foreach(var sr in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())
            {
                if(!outArea.Contains(sr.transform.position)||OutMissionObject(sr.transform,world))continue;
                if(sr.name.StartsWith("Cenário")||sr.name.StartsWith("Oclusão")||sr.name.StartsWith("Rodovia / trecho"))UnityEngine.Object.DestroyImmediate(sr.gameObject);
            }
            foreach(var collider in UnityEngine.Object.FindObjectsByType<BoxCollider2D>())
            {
                if(collider.gameObject.layer!=8||collider.attachedRigidbody||OutMissionObject(collider.transform,world))continue;
                if(!collider.name.StartsWith("Colisão")||!outArea.Contains(collider.transform.position))continue;
                UnityEngine.Object.DestroyImmediate(collider.gameObject);
            }
            var root=new GameObject("Distrito reconstruído / "+label);outRoot=root.transform;
            OutGround(label+" / solo contínuo",outArea.center,outArea.size+Vector2.one*1.8f,ground,-9200,.06f);
        }
        static SpriteRenderer OutGround(string name,Vector2 center,Vector2 size,int kind,int order,float feather=.07f)
        {
            var sprite=outGrounds[kind];var sr=Visual("Exterior / "+name,sprite,center,order,outRoot);
            sr.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);
            sr.sharedMaterial=GroundMaterial(sprite,size/8,feather);
            Color color=kind==0?new Color(.26f,.38f,.22f):kind==1?new Color(.46f,.32f,.20f):kind==2?new Color(.56f,.55f,.48f):kind==4?new Color(.40f,.40f,.35f):new Color(.22f,.25f,.26f);
            OutMap(new Rect(center-size*.5f,size),color);return sr;
        }
        static SpriteRenderer OutBox(string name,Vector2 center,Vector2 size,Color color,int order)
        {
            var sr=Visual(name,outWhite,center,order,outRoot);sr.transform.localScale=new Vector3(size.x/outWhite.bounds.size.x,size.y/outWhite.bounds.size.y,1);sr.color=color;return sr;
        }
        static void OutMap(Rect rect,Color color,bool ellipse=false){outMapShapes.Add(new OutMapShape{rect=rect,color=color,ellipse=ellipse});}
        static bool OutClear(Rect rect,float radius=1.05f)
        {
            var padded=rect;padded.min-=Vector2.one*radius;padded.max+=Vector2.one*radius;
            foreach(var point in outProtected)if(padded.Contains(point))return false;return true;
        }
        static BoxCollider2D OutSolid(string name,Rect rect,bool requireClear=true)
        {
            if(requireClear&&!OutClear(rect))return null;
            var root=new GameObject("Colisão visível / "+name);root.transform.SetParent(outRoot,true);root.transform.position=rect.center;root.layer=8;
            var box=root.AddComponent<BoxCollider2D>();box.size=rect.size;return box;
        }
        static void OutLabel(string name,Vector2 point,string text,float width=6)
        {
            var root=new GameObject(name);root.transform.SetParent(outRoot,true);root.transform.position=point;
            var label=root.AddComponent<TextMesh>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.text=text;label.fontSize=48;label.characterSize=.15f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.96f,.92f,.77f);
            var renderer=label.GetComponent<MeshRenderer>();renderer.sharedMaterial=label.font.material;renderer.sortingOrder=WorldSort.At(point.y)+4;
            var fit=root.AddComponent<WorldTextLabel>();fit.width=width;fit.height=.45f;fit.visibleDistance=18;
        }
        static void OutLine(string name,Vector2 a,Vector2 b,float thickness,Color color,int order)
        {
            var sr=OutBox(name,(a+b)*.5f,new Vector2(Vector2.Distance(a,b),thickness),color,order);sr.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
        }
        static void OutArc(string name,Vector2 center,float radius,float begin,float end,Color color,int order)
        {
            int count=Mathf.CeilToInt(Mathf.Abs(end-begin)/12);Vector2 prior=center+new Vector2(Mathf.Cos(begin*Mathf.Deg2Rad),Mathf.Sin(begin*Mathf.Deg2Rad))*radius;
            for(int i=1;i<=count;i++){float angle=Mathf.Lerp(begin,end,i/(float)count)*Mathf.Deg2Rad;Vector2 next=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;OutLine(name,prior,next,.065f,color,order);prior=next;}
        }
        static void OutFence(string name,Vector2 a,Vector2 b)
        {
            bool horizontal=Mathf.Abs(b.x-a.x)>Mathf.Abs(b.y-a.y);float length=Vector2.Distance(a,b);
            var solid=OutSolid(name,new Rect((a+b)*.5f-(horizontal?new Vector2(length,.25f):new Vector2(.25f,length))*.5f,horizontal?new Vector2(length,.25f):new Vector2(.25f,length)));
            if(!solid)return;
            int order=WorldSort.At(Mathf.Min(a.y,b.y));
            if(horizontal){OutLine(name+" / corrimão",a+Vector2.up*.35f,b+Vector2.up*.35f,.08f,new Color(.31f,.37f,.35f),order);OutLine(name+" / corrimão",a+Vector2.up*.8f,b+Vector2.up*.8f,.08f,new Color(.40f,.45f,.42f),order);}
            else OutLine(name+" / tela",a,b,.16f,new Color(.31f,.37f,.35f),order);
            for(float d=0;d<=length;d+=2){Vector2 p=Vector2.Lerp(a,b,d/length);OutBox(name+" / poste",p+Vector2.up*.45f,new Vector2(.14f,1),new Color(.24f,.29f,.27f),WorldSort.At(p.y)+1);}
            var bounds=solid.bounds;OutMap(Rect.MinMaxRect(bounds.min.x,bounds.min.y,bounds.max.x,bounds.max.y),new Color(.25f,.30f,.26f));
        }
        static Sprite OutCrop(Texture2D texture,Rect topRect,string name,float width)
        {
            var sprite=Sprite.Create(texture,new Rect(topRect.x,texture.height-topRect.y-topRect.height,topRect.width,topRect.height),Vector2.one*.5f,topRect.width/width,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,bank);return sprite;
        }
        static SpriteRenderer OutPicture(string name,Sprite sprite,Vector2 center,float width,int order)
        {
            var sr=Visual(name,sprite,center,order,outRoot);sr.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);sr.sharedMaterial=GroundMaterial(sprite,Vector2.one,.035f);return sr;
        }
        static void OutPlant(Vector2 center,float width)
        {
            var footprint=new Rect(center-new Vector2(.45f,.55f),new Vector2(.9f,.75f));if(!OutClear(footprint,1.4f))return;
            var root=new GameObject("Vegetação / canteiro do distrito");root.transform.SetParent(outRoot,true);root.transform.position=center;root.layer=8;
            var solid=root.AddComponent<BoxCollider2D>();solid.size=footprint.size;solid.offset=footprint.center-center;
            var sr=Visual("Moita / copa interativa",questIcons[5],center+Vector2.up*.55f,WorldSort.At(center.y),root.transform);sr.transform.localScale=Vector3.one*(width/questIcons[5].bounds.size.x);sr.gameObject.AddComponent<WorldVegetation2D>();
            OutMap(new Rect(center-Vector2.one*width*.45f,Vector2.one*width*.9f),new Color(.19f,.34f,.17f),true);
        }
        static void OutBench(Vector2 center)
        {
            var rect=new Rect(center-new Vector2(.95f,.25f),new Vector2(1.9f,.5f));if(!OutSolid("banco do parque",rect))return;
            var sr=Visual("Banco / ripas",outWood,center+Vector2.up*.12f,WorldSort.At(center.y),outRoot);sr.transform.localScale=new Vector3(1.9f/outWood.bounds.size.x,.55f/outWood.bounds.size.y,1);
            OutBox("Banco / encosto",center+Vector2.up*.55f,new Vector2(1.9f,.22f),new Color(.37f,.22f,.12f),WorldSort.At(center.y)+1);
            foreach(float x in new[]{-.7f,.7f})OutBox("Banco / pé",center+new Vector2(x,-.22f),new Vector2(.13f,.4f),new Color(.18f,.21f,.20f),WorldSort.At(center.y));
        }
        static void OutLamp(Vector2 center)
        {
            var root=new GameObject("Iluminação / poste do distrito");root.transform.SetParent(outRoot,true);root.transform.position=center;
            var pole=OutBox("Poste / metal",center+Vector2.up*1.1f,new Vector2(.12f,2.2f),new Color(.20f,.25f,.26f),WorldSort.At(center.y));pole.transform.SetParent(root.transform,true);
            var bulb=OutBox("Poste / lâmpada",center+Vector2.up*2.15f,new Vector2(.32f,.22f),Color.white,WorldSort.At(center.y)+2);bulb.transform.SetParent(root.transform,true);
            var glow=Visual("Halo noturno",outGlow,center,WorldSort.At(center.y)+1,root.transform);glow.transform.localScale=Vector3.one*6;
            var lamp=root.AddComponent<WorldLamp2D>();lamp.glow=glow;lamp.bulb=bulb;
        }
        static void SaveOutdoorMap(WorldHub world,string id,Color baseColor)
        {
            int width=240,height=160;var texture=new Texture2D(width,height,TextureFormat.RGBA32,false){name="Mapa reconstruído / "+id,filterMode=FilterMode.Bilinear};var pixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                Vector2 point=outArea.min+Vector2.Scale(new Vector2((x+.5f)/width,(y+.5f)/height),outArea.size);Color color=baseColor;
                foreach(var shape in outMapShapes)
                {
                    if(!shape.rect.Contains(point))continue;
                    if(shape.ellipse){var uv=(point-shape.rect.center)/(shape.rect.size*.5f);if(uv.sqrMagnitude>1)continue;}
                    color=shape.color;
                }
                pixels[y*width+x]=color;
            }
            texture.SetPixels(pixels);texture.Apply();AssetDatabase.AddObjectToAsset(texture,bank);
            for(int i=0;i<world.districtRects.Length;i++)if(Vector2.Distance(world.districtRects[i].center,outArea.center)<.1f)world.districtMaps[i]=texture;
            foreach(var mission in world.missions)if(Vector2.Distance(mission.center,outArea.center)<.1f&&mission.level)mission.level.mapTexture=texture;
        }
        static void RebuildMocambo(WorldHub world)
        {
            BeginOutdoor(world,"24_Mocambo","Parque do Mocambo",0);Vector2 c=outArea.center;
            OutGround("Mocambo / mata e serrapilheira",c+new Vector2(-13,7),new Vector2(19,14),3,-9185,.18f);
            OutGround("Mocambo / alameda sul",c+new Vector2(0,-10.2f),new Vector2(43,3),2,-9160);
            OutGround("Mocambo / trilha do lago",c+new Vector2(-5.2f,1),new Vector2(2.6f,23),2,-9159);
            OutGround("Mocambo / acesso da quadra",c+new Vector2(7.68f,-9),new Vector2(3.3f,11),2,-9158);
            OutGround("Mocambo / alameda leste",c+new Vector2(20,1.5f),new Vector2(2.3f,24),2,-9158);
            OutGround("Mocambo / quiosque e praça",c+new Vector2(-11.5f,5),new Vector2(12,3.5f),2,-9158);
            Vector2 lake=c+new Vector2(-16,-2.5f);var water=Visual("Mocambo / lago e margem",outWater,lake,-9120,outRoot);water.transform.localScale=new Vector3(11.8f/outWater.bounds.size.x,7.2f/outWater.bounds.size.y,1);
            var lakeBody=new GameObject("Colisão visível / lago do Mocambo");lakeBody.transform.SetParent(outRoot,true);lakeBody.transform.position=lake;lakeBody.layer=8;var polygon=lakeBody.AddComponent<PolygonCollider2D>();var points=new Vector2[20];for(int i=0;i<points.Length;i++){float a=i*Mathf.PI*2/points.Length;points[i]=new Vector2(Mathf.Cos(a)*5.7f,Mathf.Sin(a)*3.4f);}polygon.points=points;
            OutMap(new Rect(lake-new Vector2(5.9f,3.6f),new Vector2(11.8f,7.2f)),new Color(.13f,.37f,.40f),true);
            if(LocationHDReady)LocationHDVisual("Mocambo / quiosque HD apoiado no chão",1,c+new Vector2(-13,5.9f),5.4f,outRoot);
            else{var kiosk=OutCrop(campaignMaps["mocambo"],new Rect(260,155,180,184),"Quiosque do Mocambo / detalhe",5.4f);OutPicture("Mocambo / quiosque original",kiosk,c+new Vector2(-13,8),5.4f,WorldSort.At(c.y+5));}
            OutSolid("quiosque do parque",new Rect(c+new Vector2(-14.9f,5.9f),new Vector2(3.8f,2.2f)));
            Vector2 court=c+new Vector2(8,2);var surface=Visual("Mocambo / quadra autorada de basquete",outCourt,court,-9100,outRoot);surface.transform.localScale=new Vector3(18/outCourt.bounds.size.x,17/outCourt.bounds.size.y,1);
            Color chalk=new Color(.98f,.93f,.80f);int lineOrder=-9090;float left=court.x-8.4f,right=court.x+8.4f,bottom=court.y-7.9f,top=court.y+7.9f;
            OutMap(new Rect(court-new Vector2(9,8.5f),new Vector2(18,17)),new Color(.66f,.31f,.18f));
            OutLine("Quadra / lateral",new Vector2(left,bottom),new Vector2(left,top),.085f,chalk,lineOrder);OutLine("Quadra / lateral",new Vector2(right,bottom),new Vector2(right,top),.085f,chalk,lineOrder);
            OutLine("Quadra / fundo",new Vector2(left,bottom),new Vector2(right,bottom),.085f,chalk,lineOrder);OutLine("Quadra / fundo",new Vector2(left,top),new Vector2(right,top),.085f,chalk,lineOrder);OutLine("Quadra / meio",new Vector2(left,court.y),new Vector2(right,court.y),.075f,chalk,lineOrder);
            OutArc("Quadra / círculo central",court,1.65f,0,360,chalk,lineOrder);
            foreach(float sign in new[]{-1f,1f})
            {
                float endY=court.y+sign*7.9f,freeY=court.y+sign*4.7f;
                OutBox("Quadra / garrafão",new Vector2(court.x,(endY+freeY)*.5f),new Vector2(3.6f,3.2f),new Color(.28f,.42f,.32f),lineOrder-1);
                OutLine("Quadra / garrafão",new Vector2(court.x-1.8f,endY),new Vector2(court.x-1.8f,freeY),.065f,chalk,lineOrder);OutLine("Quadra / garrafão",new Vector2(court.x+1.8f,endY),new Vector2(court.x+1.8f,freeY),.065f,chalk,lineOrder);OutLine("Quadra / lance livre",new Vector2(court.x-1.8f,freeY),new Vector2(court.x+1.8f,freeY),.065f,chalk,lineOrder);
                OutArc("Quadra / arco",new Vector2(court.x,endY),5,sign>0?180:0,sign>0?360:180,chalk,lineOrder);
                Vector2 hoop=new Vector2(court.x,court.y+sign*8.2f);
                if(LocationHDReady)LocationHDVisual("Mocambo / cesta completa HD com base",2,hoop-Vector2.up*.26f,1.4f,outRoot);
                else{OutBox("Cesta / suporte",hoop,new Vector2(.18f,.8f),new Color(.23f,.28f,.28f),WorldSort.At(hoop.y));OutBox("Cesta / tabela",hoop+Vector2.up*.65f,new Vector2(1.25f,.65f),new Color(.86f,.88f,.79f),WorldSort.At(hoop.y)+1);OutArc("Cesta / aro",hoop+Vector2.up*.28f,.24f,0,360,new Color(.97f,.38f,.08f),WorldSort.At(hoop.y)+2);}
                OutSolid("suporte da cesta",new Rect(hoop-new Vector2(.18f,.26f),new Vector2(.36f,.52f)));
            }
            for(int i=0;i<11;i++){float x=-22+i*4.4f;OutPlant(c+new Vector2(x,13.4f),2.7f);if(Mathf.Abs(x-7.68f)>4)OutPlant(c+new Vector2(x,-13.2f),2.6f);}
            for(int i=0;i<5;i++){OutPlant(c+new Vector2(-22,-9+i*4.2f),3.1f);OutPlant(c+new Vector2(22,-9+i*4.2f),2.8f);}
            foreach(var p in new[]{new Vector2(-8,-8),new Vector2(18,-7),new Vector2(-10,11)})OutBench(c+p);
            foreach(var p in new[]{new Vector2(-3,-9),new Vector2(17,-8),new Vector2(-4,11),new Vector2(19,11)})OutLamp(c+p);
            OutLabel("Parque / orientação",c+new Vector2(-12,-9),"LAGO  •  QUIOSQUE",6);
            if(LocationHDReady)
            {
                Vector2 gate=world.Find("24_Mocambo").entry;var portal=LocationHDVisual("Mocambo / portal de entrada HD",5,gate-Vector2.up*.5f,9,outRoot);
                portal.transform.localScale=new Vector3(portal.transform.localScale.x,portal.transform.localScale.y*.72f,1);
                LocationHDText(portal,"PARQUE DO MOCAMBO",new Vector2(0,.80f),7.2f,.65f,outRoot);
                foreach(float sign in new[]{-1f,1f})OutSolid("pilar do portal Mocambo",new Rect(gate+new Vector2(sign*3.65f-.2f,-.5f),new Vector2(.4f,.65f)));
                var parkSign=GameObject.Find("Placa / PARQUE DO MOCAMBO");
                if(parkSign)
                {
                    var oldCollider=parkSign.GetComponent<BoxCollider2D>();if(oldCollider)oldCollider.enabled=false;
                    Vector2 board=parkSign.transform.position;
                    foreach(float sign in new[]{-1f,1f})
                    {
                        Vector2 support=board+new Vector2(sign*2.65f,-.85f);OutBox("Placa do parque / suporte até o chão",support,new Vector2(.14f,1.4f),new Color(.22f,.27f,.23f),WorldSort.At(board.y-1.55f));
                        OutSolid("suporte da placa Mocambo",new Rect(board+new Vector2(sign*2.65f-.15f,-1.55f),new Vector2(.3f,.4f)));
                    }
                }
            }
            SaveOutdoorMap(world,"Mocambo",new Color(.24f,.37f,.20f));
        }
        static void RebuildScrapyard(WorldHub world)
        {
            BeginOutdoor(world,"14_FerroVelho","Ferro-velho municipal",1);Vector2 c=outArea.center;
            OutGround("Ferro-velho / pátio de brita",c,new Vector2(47,30),4,-9180,.06f);OutGround("Ferro-velho / corredor de acesso",c,new Vector2(8,31),1,-9160);OutGround("Ferro-velho / corredor de serviço",c+Vector2.up*1.6f,new Vector2(46,4.2f),1,-9159);
            var scrap=campaignMaps["scrap"];var pileA=OutCrop(scrap,new Rect(205,125,455,245),"Sucata / bloco de peças A",9.8f);var pileB=OutCrop(scrap,new Rect(885,450,490,290),"Sucata / bloco de peças B",10.6f);
            var piles=new[]{(new Vector2(-15,7),new Vector2(8.8f,4.9f),pileA),(new Vector2(12.7f,6.9f),new Vector2(8.6f,4.7f),pileA),(new Vector2(-13,-4.5f),new Vector2(10.2f,4.9f),pileB),(new Vector2(13,-4.5f),new Vector2(10.2f,4.9f),pileB)};
            foreach(var pile in piles)
            {
                var center=c+pile.Item1;var rect=new Rect(center-pile.Item2*.5f,pile.Item2);
                if(!OutClear(rect,1.25f))continue;
                if(LocationHDReady)
                {
                    var sr=LocationHDVisual("Ferro-velho / pilha HD apoiada no chão",3,new Vector2(center.x,rect.yMin),pile.Item2.x,outRoot);
                    rect.height=Mathf.Min(rect.height,sr.sprite.bounds.size.y*Mathf.Abs(sr.transform.lossyScale.y)*.82f);
                }
                else{var sr=OutPicture("Ferro-velho / pilha de carros e metal",pile.Item3,center,pile.Item2.x,WorldSort.At(rect.yMin));sr.color=new Color(1.35f,1.32f,1.23f);}
                OutSolid("pilha de sucata",rect);OutMap(rect,new Color(.24f,.25f,.23f));
            }
            if(LocationHDReady)LocationHDVisual("Ferro-velho / guarita HD com base",4,c+new Vector2(18,12.4f),5.3f,outRoot);
            else{var shack=OutCrop(scrap,new Rect(1180,32,210,130),"Guarita / detalhe original",5.3f);OutPicture("Ferro-velho / guarita",shack,c+new Vector2(18,13.5f),5.3f,WorldSort.At(c.y+12));}
            OutSolid("guarita",new Rect(c+new Vector2(16.2f,12.4f),new Vector2(3.4f,1.6f)));
            OutFence("Ferro-velho / cerca norte",c+new Vector2(-23,15),c+new Vector2(23,15));OutFence("Ferro-velho / cerca oeste",c+new Vector2(-23,-15),c+new Vector2(-23,15));OutFence("Ferro-velho / cerca leste",c+new Vector2(23,-15),c+new Vector2(23,15));OutFence("Ferro-velho / portão esquerdo",c+new Vector2(-23,-15),c+new Vector2(-4,-15));OutFence("Ferro-velho / portão direito",c+new Vector2(4,-15),c+new Vector2(23,-15));
            foreach(var p in new[]{new Vector2(-20,-12),new Vector2(20,-12),new Vector2(-20,12),new Vector2(4,12)})OutLamp(c+p);
            OutLabel("Identificação do ferro-velho",c+new Vector2(-11,-13),"FERRO-VELHO\nPÁTIO DE TRIAGEM",8);
            OutLabel("Triagem / placas",c+new Vector2(15,10),"GUARITA  •  REGISTROS",5);
            SaveOutdoorMap(world,"FerroVelho",new Color(.41f,.35f,.25f));
        }
        static void RebuildHighway(WorldHub world)
        {
            BeginOutdoor(world,"13_Estrada","Estrada de serviço",0);Vector2 c=outArea.center;
            OutGround("Rodovia / acostamentos de terra",c,new Vector2(99,20),4,-9170,.045f);OutGround("Rodovia / pista contínua",c,new Vector2(99,14.6f),5,-9130,.012f);
            Color paint=new Color(.91f,.85f,.63f);
            foreach(float y in new[]{-6.8f,6.8f})OutLine("Rodovia / faixa lateral",c+new Vector2(-48,y),c+new Vector2(48,y),.12f,paint,-9120);
            for(float x=-46;x<47;x+=4)OutLine("Rodovia / faixa central",c+new Vector2(x,0),c+new Vector2(x+1.8f,0),.10f,new Color(.93f,.69f,.22f),-9120);
            foreach(float sign in new[]{-1f,1f})
            {
                for(float section=-46;section<46;section+=10)OutFence("Rodovia / defensa",c+new Vector2(section,sign*8.2f),c+new Vector2(Mathf.Min(46,section+10),sign*8.2f));
                for(float x=-44;x<46;x+=7){OutPlant(c+new Vector2(x,sign*(11.5f+(Mathf.RoundToInt(x)%3)*.3f)),2.4f+(Mathf.Abs(x)%3)*.2f);}
            }
            for(int i=0;i<4;i++)
            {
                Vector2 pos=c+new Vector2(-36+i*23,10.2f);OutBox("Estrada / marco quilométrico",pos,new Vector2(.6f,.8f),new Color(.19f,.36f,.25f),WorldSort.At(pos.y));OutLabel("Estrada / quilometragem",pos+Vector2.up*.2f,"KM "+(i+1),1.2f);
            }
            OutLamp(c+new Vector2(-35,-10));OutLamp(c+new Vector2(13,10));OutLamp(c+new Vector2(42,-10));
            OutBox("Estrada / placa ferro-velho",c+new Vector2(32,11),new Vector2(9,1.5f),new Color(.09f,.26f,.20f),WorldSort.At(c.y+10));OutLabel("Estrada / destino",c+new Vector2(32,11),"FERRO-VELHO  →",8);
            OutGround("Rodovia / recuo de manutenção",c+new Vector2(-17,-12),new Vector2(12,5),2,-9140,.05f);
            if(cleanCars!=null&&cleanCars.Length>4){var parked=Visual("Rodovia / carro estacionado no recuo",cleanCars[4][1],c+new Vector2(-17,-12),WorldSort.At(c.y-13),outRoot);parked.transform.localScale=Vector3.one*(3.2f/Mathf.Max(parked.sprite.bounds.size.x,parked.sprite.bounds.size.y));OutSolid("carro estacionado",new Rect(c+new Vector2(-18.5f,-12.5f),new Vector2(3,.9f)));}
            SaveOutdoorMap(world,"Estrada",new Color(.28f,.36f,.22f));
        }
    }
}
