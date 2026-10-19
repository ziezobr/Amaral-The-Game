using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static void RebuildCityHubAndInteriors(WorldHub world)
        {
            var blocks=new List<Rect>(world.streetBlocks);var hubRect=world.districtRects[0];
            var original=GameObject.Find("Bairro / cenário próprio");if(original)UnityEngine.Object.DestroyImmediate(original);
            foreach(var solid in UnityEngine.Object.FindObjectsByType<BoxCollider2D>())if(solid.gameObject.name.StartsWith("Colisão /")&&hubRect.Contains(solid.transform.position)&&!solid.GetComponentInParent<Rigidbody2D>())UnityEngine.Object.DestroyImmediate(solid.gameObject);
            int index=0;
            foreach(float u in new[]{.15f,.50f,.85f})foreach(float v in new[]{.11f,.45f,.82f})
            {
                Vector2 center=new Vector2((u-.5f)*64,(.5f-v)*128/3);var size=new Vector2((u<.2f?.25f:u<.6f?.23f:.25f)*64,(v<.2f?.17f:v<.6f?.18f:.24f)*128/3);
                float x0=u<.2f?-30.5f:u<.6f?-5.66f:16.1f,x1=u<.2f?-16.1f:u<.6f?5.66f:30.5f;
                float y0=v<.2f?14.58f:v<.6f?-.25f:-20.4f,y1=v<.2f?20.4f:v<.6f?4.18f:-10.75f;
                var rect=Rect.MinMaxRect(x0,y0,x1,y1);center=rect.center;size=rect.size;blocks.Add(rect);var plot=Visual("Cidade / pátio central",landscapeGround[2],center,-8997);DressGround(plot,size,2,.018f);
                var solid=new GameObject("Colisão / quarteirão central reconstruído");solid.layer=8;solid.transform.position=center;solid.AddComponent<BoxCollider2D>().size=size;
                Fence(rect,"Muro baixo / quarteirão central");
                for(int house=0;house<2;house++)
                {
                    var art=cityBuildingArt[(index+house)%6];Vector2 feet=new Vector2(center.x+(house==0?-1:1)*size.x*.24f,rect.yMin+.18f);
                    var sr=Visual("Cidade / fachada do centro",art,feet,WorldSort.At(feet.y));float scale=Mathf.Min((size.x*.47f)/art.bounds.size.x,(size.y-.3f)/art.bounds.size.y);sr.transform.localScale=Vector3.one*scale;
                    var cast=sr.gameObject.AddComponent<WorldPropShadow2D>();cast.source=sr;cast.height=2.5f;
                }
                index++;
            }
            foreach(var pair in new[]{("office","AGÊNCIA RIBERAL",0),("gala","SALÃO DE EVENTOS",1),("home","CASA DE AMARAL",0),("workshop","OFICINA / HENZO",3),("warehouse","GALPÕES DA RODOVIA",4)})
            {
                int zone=-1;for(int i=1;i<world.districtMaps.Length;i++){string key=world.districtMaps[i].name.ToLowerInvariant();if(key.Contains(pair.Item1)||pair.Item1=="office"&&key.Contains("riberal")){zone=i;break;}}
                if(zone<0)continue;var rect=world.districtRects[zone];var indoors=new List<SpriteRenderer>();
                foreach(var sr in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())if((sr.name.StartsWith("Cenário")||sr.name.StartsWith("Oclusão /"))&&rect.Contains(sr.transform.position))indoors.Add(sr);
                var root=new GameObject("Cidade / edifício de missão / "+pair.Item2);var outside=new List<SpriteRenderer>();
                var plot=Visual("Pátio / "+pair.Item2,landscapeGround[2],rect.center,-4997,root.transform);DressGround(plot,rect.size,2,.01f);outside.Add(plot);
                var sprite=cityBuildingArt[pair.Item3];var feet=new Vector2(rect.center.x,rect.yMin+.4f);var facade=Visual("Fachada / "+pair.Item2,sprite,feet,WorldSort.At(feet.y),root.transform);float scale=Mathf.Min((rect.width-2)/sprite.bounds.size.x,(rect.height-1)/sprite.bounds.size.y);facade.transform.localScale=Vector3.one*scale;outside.Add(facade);
                var state=root.AddComponent<InteriorFacade2D>();state.footprint=rect;state.interior=indoors.ToArray();state.exterior=outside.ToArray();
                // The perimeter advertises the real existing collision walls even while the cutaway is closed.
                var mission=System.Array.Find(world.missions,m=>m.center==rect.center);Fence(rect,"Muro / "+pair.Item2,mission!=null?(float?)mission.entry.x:null);if(mission!=null)StreetName(mission.entry+Vector2.right*4.7f,pair.Item2,5);
            }
            world.streetBlocks=blocks.ToArray();world.rebuiltOutdoorLayout=true;
            var shop=UnityEngine.Object.FindAnyObjectByType<EquipmentShop>();if(shop){shop.transform.position+=Vector3.up*1.2f;foreach(var table in UnityEngine.Object.FindObjectsByType<PcWorkbench>())table.transform.position+=Vector3.up*1.2f;}

            StreetName(new Vector2(-5.5f,-23),"AVENIDA CENTRAL",8);StreetName(new Vector2(-79,24),"RUA DA RIBERAL",7);StreetName(new Vector2(96,23),"VIA DOS GALPÕES",7);StreetName(new Vector2(-23,-78),"CAMINHO DO MOCAMBO",8);StreetName(new Vector2(-80,95),"ALAMEDA DO CÓRREGO",8);StreetName(new Vector2(191,-78),"ESTRADA DO RESGATE",8);
        }
        static void RebuildPedestrianRoutes(WorldHub world)
        {
            Physics2D.SyncTransforms();var routes=new List<Vector2[]>();int mask=LayerMask.GetMask("World");
            for(int i=0;i<world.roadStarts.Length;i++)
            {
                Vector2 a=world.roadStarts[i],b=world.roadEnds[i],dir=(b-a).normalized,side=new Vector2(-dir.y,dir.x);float length=Vector2.Distance(a,b);if(length<11)continue;
                foreach(float sign in new[]{-1f,1f})
                {
                    Vector2 first=a+dir*4.5f+side*4.25f*sign,last=b-dir*4.5f+side*4.25f*sign;bool blocked=false;
                    foreach(var hit in Physics2D.CircleCastAll(first,.3f,(last-first).normalized,Vector2.Distance(first,last),mask))if(hit.collider&&!hit.collider.isTrigger&&(!hit.rigidbody||hit.rigidbody.bodyType==RigidbodyType2D.Static))blocked=true;
                    if(!blocked)routes.Add(new[]{first,last});
                }
            }
            if(routes.Count==0)throw new System.InvalidOperationException("No pedestrian routes remain after city reconstruction");
            int index=0;foreach(var walker in UnityEngine.Object.FindObjectsByType<StreetWalker>()){var route=routes[index*17%routes.Count];walker.route=route;walker.transform.position=Vector2.Lerp(route[0],route[1],.13f+(index%6)*.13f);index++;}
            Debug.Log("PONTO_PEDESTRIAN_ROUTES / "+routes.Count+" / residents "+index);
        }
        static void Fence(Rect rect,string name,float? doorX=null)
        {
            var line=Procedural("Mureta clara",8,(x,y)=>new Color(.57f+(x%4==0?-.08f:0),.53f,.44f));
            // Mission entrances stay physically and visually open at the same authored coordinates.
            var edges=new List<Rect>{new Rect(rect.xMin,rect.yMin,.15f,rect.height),new Rect(rect.xMax-.15f,rect.yMin,.15f,rect.height),new Rect(rect.xMin,rect.yMax-.15f,rect.width,.15f)};
            if(doorX.HasValue){edges.Add(new Rect(rect.xMin,rect.yMin,doorX.Value-2.25f-rect.xMin,.15f));edges.Add(new Rect(doorX.Value+2.25f,rect.yMin,rect.xMax-doorX.Value-2.25f,.15f));}else edges.Add(new Rect(rect.xMin,rect.yMin,rect.width,.15f));
            foreach(var edge in edges)
            {var sr=Visual(name,line,edge.center,-4994);sr.transform.localScale=new Vector3(edge.width/line.bounds.size.x,edge.height/line.bounds.size.y,1);}
        }
        static void StreetName(Vector2 at,string text,float width)
        {
            // Put a physical sign beside the street, with feet at the same ground anchor as its sorting.
            var world=UnityEngine.Object.FindAnyObjectByType<WorldHub>();
            if(world)for(int pass=0;pass<3;pass++)for(int i=0;i<world.roadStarts.Length;i++){Vector2 a=world.roadStarts[i],b=world.roadEnds[i],delta=b-a;float along=Mathf.Clamp01(Vector2.Dot(at-a,delta)/Mathf.Max(.001f,delta.sqrMagnitude));Vector2 nearest=a+delta*along,away=at-nearest;if(away.magnitude<5.5f){if(away.sqrMagnitude<.001f)away=new Vector2(-delta.y,delta.x).normalized;at=nearest+away.normalized*5.7f;}}
            width=Mathf.Min(width,3.3f);
            var root=new GameObject("Placa da rua / "+text);root.transform.position=at;
            var metal=Procedural("Suporte de placa",8,(x,y)=>new Color(.19f,.22f,.20f));
            foreach(float sign in new[]{-1f,1f}){var post=Visual("Poste da placa / pés no chão",metal,at+new Vector2(sign*width*.36f,.7f),WorldSort.At(at.y),root.transform);post.transform.localScale=new Vector3(.10f/metal.bounds.size.x,1.4f/metal.bounds.size.y,1);var baseArt=Visual("Base da placa",metal,at+Vector2.right*sign*width*.36f,WorldSort.At(at.y)-1,root.transform);baseArt.transform.localScale=new Vector3(.34f/metal.bounds.size.x,.18f/metal.bounds.size.y,1);}
            at+=Vector2.up*1.4f;
            var board=Procedural("Placa urbana verde",16,(x,y)=>new Color(.06f,.23f,.18f));var sr=Visual("Placa urbana",board,at,WorldSort.At(at.y),root.transform);sr.transform.localScale=new Vector3(width/board.bounds.size.x,.7f/board.bounds.size.y,1);
            var label=new GameObject("Nome do local");label.transform.SetParent(root.transform,false);label.transform.localPosition=Vector3.up*1.4f;var t=label.AddComponent<TextMesh>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;t.text=text;t.fontSize=64;t.characterSize=.08f;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.94f,.96f,.85f);t.GetComponent<MeshRenderer>().sortingOrder=WorldSort.At(at.y)+1;var fit=label.AddComponent<WorldTextLabel>();fit.width=width-.3f;fit.height=.48f;fit.visibleDistance=10;
        }
    }
}
