using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static readonly Vector2[] landscapeSites={new Vector2(-60,75),new Vector2(-33,75),new Vector2(118,74),new Vector2(166,75),new Vector2(183,-10),new Vector2(54,-75)};
        static Sprite[] landscapeGround;
        static Material GroundMaterial(Sprite sprite,Vector2 repeat,float feather)
        {
            var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/PontoCego/Resources/GroundBlend.shader");
            if(!shader)throw new System.InvalidOperationException("Ground blend shader missing");
            var mat=new Material(shader){name="Transição de solo / "+sprite.name};var r=sprite.rect;var tex=sprite.texture;
            mat.SetVector("_UVRect",new Vector4(r.x/tex.width,r.y/tex.height,r.width/tex.width,r.height/tex.height));mat.SetVector("_Repeat",new Vector4(Mathf.Max(1,repeat.x),Mathf.Max(1,repeat.y),0,0));mat.SetFloat("_Feather",feather);mat.SetFloat("_WorldSpace",tex.name=="terrain-atlas-v1"?1:0);AssetDatabase.AddObjectToAsset(mat,bank);return mat;
        }
        static Sprite LandscapeCell(Texture2D atlas,int index,string name,float worldSize)
        {
            int w=atlas.width/3,h=atlas.height/2;var sprite=Sprite.Create(atlas,new Rect(index%3*w,(1-index/3)*h,w,h),new Vector2(.5f,.5f),w/worldSize,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,bank);return sprite;
        }
        static void DressGround(SpriteRenderer sr,Vector2 size,int kind,float feather)
        {
            sr.sprite=landscapeGround[kind];sr.drawMode=SpriteDrawMode.Simple;sr.transform.localScale=new Vector3(size.x/sr.sprite.bounds.size.x,size.y/sr.sprite.bounds.size.y,1);sr.sharedMaterial=GroundMaterial(sr.sprite,size/12,feather);
        }
        static void GroundPatch(string name,Vector2 center,Vector2 size,int kind,int order,float feather=.18f)
        {var sr=Visual(name,landscapeGround[kind],center,order);DressGround(sr,size,kind,feather);}
        static void BuildLandscapeIntegration(WorldHub world)
        {
            var atlas=Import(Art+"/Landscape/terrain-atlas-v1.png");landscapeGround=new Sprite[6];for(int i=0;i<6;i++)landscapeGround[i]=LandscapeCell(atlas,i,"Material de solo / "+i,16);
            PrepareLocationHD(Import(Art+"/Landscape/environment-v2.png"));
            // Feather sprite borders into real terrain; no extra collision or geometry is hidden here.
            foreach(var sr in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())
            {
                if(sr.name.StartsWith("Terreno contínuo")){DressGround(sr,world.bounds.size,0,.001f);sr.color=new Color(.77f,.83f,.77f);}
                else if((sr.name=="Rua de ligação"||sr.name=="Asfalto / rede canônica"))DressGround(sr,sr.bounds.size,5,.012f);
                else if(sr.name=="Calçada contínua"||sr.name.StartsWith("Calçada / rede canônica /")||sr.name=="Pátio e acesso")DressGround(sr,sr.bounds.size,2,.025f);
                else if(sr.name.StartsWith("Cenário")&&sr.sortingOrder<0&&sr.sprite)sr.sharedMaterial=GroundMaterial(sr.sprite,Vector2.one,.012f);
            }
            foreach(var district in world.districtRects)
            {
                GroundPatch("Fronteira / terra e vegetação",district.center,district.size+Vector2.one*4,3,-9500,.12f);
            }
            var poiAtlas=Import(Art+"/Landscape/exploration-atlas-v1.png");var sprites=new List<Sprite>();var patches=new List<Rect>();
            var sites=landscapeSites;
            string[] titles={"Ruínas do antigo depósito","Passarela do córrego","Abrigo da linha antiga","Horta dos moradores","Oficina de manutenção","Escadaria do mirante"};
            string[] notes={"Um caminho gasto atravessa os muros. Há pegadas recentes entre os tijolos.","A ponte liga as trilhas antigas. As marcas de pneus terminam perto da margem.","Horários apagados e um banco gasto contam como o bairro cresceu ao redor desta rota.","A comunidade mantém este espaço vivo. As trilhas levam às ruas vizinhas.","Etiquetas de manutenção e cabos abandonados sugerem uma antiga rota de serviços.","Da subida é possível reconhecer os caminhos entre os bairros e o parque."};
            for(int i=0;i<sites.Length;i++)
            {
                var sprite=LandscapeCell(poiAtlas,i,titles[i],14);var patch=Visual("Paisagem / "+titles[i],sprite,sites[i],-8996);patch.sharedMaterial=GroundMaterial(sprite,Vector2.one,.075f);
                sprites.Add(sprite);patches.Add(new Rect(sites[i]-Vector2.one*7,Vector2.one*14));
                Vector2 front=sites[i]+Vector2.down*4.9f;var root=new GameObject("Ponto de interesse / "+titles[i]);root.layer=10;root.transform.position=front;root.AddComponent<CircleCollider2D>().isTrigger=true;var landmark=root.AddComponent<EnvironmentalLandmark>();landmark.id="city:"+i;landmark.title=titles[i];landmark.description=notes[i];
                // Visible scene objects have matching local solids; central trails and discovery anchors remain open.
                void Solid(Vector2 offset,Vector2 size){var go=new GameObject("Colisão / "+titles[i]);go.layer=8;go.transform.position=sites[i]+offset;go.AddComponent<BoxCollider2D>().size=size;}
                if(i==0){Solid(new Vector2(-4,1),new Vector2(.7f,6));Solid(new Vector2(4,2),new Vector2(.7f,4));}
                if(i==1){Solid(new Vector2(-4,.7f),new Vector2(5,2.7f));Solid(new Vector2(4,.7f),new Vector2(5,2.7f));}
                if(i==2){Solid(new Vector2(0,2),new Vector2(7,.5f));Solid(new Vector2(0,.3f),new Vector2(4.5f,.7f));}
                if(i==3){Solid(new Vector2(-3,0),new Vector2(3.2f,7));Solid(new Vector2(3,0),new Vector2(3.2f,7));}
                if(i==4){Solid(new Vector2(0,2),new Vector2(7,3));Solid(new Vector2(-3,-1.2f),new Vector2(1.5f,2.4f));Solid(new Vector2(3.5f,-1.2f),new Vector2(2,2.4f));}
            }
            world.landmarkMaps=sprites.ToArray();world.landmarkPatches=patches.ToArray();
            // Ground detail has no collider. It never obstructs mission gates, vehicles or collectibles.
            for(int i=0;i<world.roadStarts.Length;i++)
            {
                Vector2 a=world.roadStarts[i],b=world.roadEnds[i],dir=(b-a).normalized,side=new Vector2(-dir.y,dir.x);
                for(float d=8;d<Vector2.Distance(a,b)-5;d+=18)
                {
                    Vector2 at=a+dir*d+side*(7.8f+(i%3)*.6f);bool inside=false;foreach(var r in world.districtRects)if(r.Contains(at))inside=true;foreach(var r in world.streetBlocks)if(r.Contains(at))inside=true;foreach(var r in patches)if(r.Contains(at))inside=true;if(inside)continue;
                    GroundPatch("Paisagem / raízes e cascalho",at,new Vector2(6,4),i%2==0?3:4,-9400,.3f);
                    var plant=Visual("Vegetação / moita da trilha",questIcons[5],at,WorldSort.At(at.y));plant.transform.localScale=Vector3.one*(.8f+(i%3)*.25f);plant.gameObject.AddComponent<WorldVegetation2D>();
                }
            }
            RebuildCityHubAndInteriors(world);BuildRebuiltOutdoorDistricts(world,landscapeGround);RefreshAgencyFacade(world);RebuildPedestrianRoutes(world);
            foreach(var sr in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())if(sr.name=="Fachada e telhado")
            {var cast=sr.gameObject.AddComponent<WorldPropShadow2D>();cast.source=sr;cast.height=3.2f;}
        }
    }
}
