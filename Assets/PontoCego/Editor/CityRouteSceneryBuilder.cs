using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        // Reserve the entire illustration, not just its collider: roofs must not cover roads.
        static Sprite[] cityBuildingArt;
        static void BuildCityRouteScenery(WorldHub world)
        {
            var atlas=Import(Art+"/City/city-buildings.png");var buildings=new Sprite[6];
            // Native sprite meshes isolate each facade without modifying the supplied atlas pixels.
            int[][] outlines={
                new[]{22,343,33,243,73,181,95,166,92,81,284,83,284,48,326,25,374,40,384,89,425,91,429,129,454,129,480,418,62,420},
                new[]{550,359,568,220,585,72,812,67,812,38,853,21,897,38,898,68,928,70,942,137,975,147,986,359,966,424,590,432,555,399},
                new[]{1056,422,1064,328,1077,314,1078,159,1069,168,1086,124,1099,91,1317,92,1317,27,1439,22,1446,78,1462,86,1471,150,1496,157,1503,211,1497,314,1510,423},
                new[]{31,928,44,680,56,567,126,566,129,551,159,543,191,561,190,567,442,567,453,611,470,622,487,835,470,839,468,877,462,937},
                new[]{542,954,559,751,567,696,557,651,577,549,775,548,775,481,896,482,897,547,974,549,988,657,986,711,1006,939,1011,952},
                new[]{1095,955,1093,845,1082,844,1088,668,1102,650,1108,578,1144,553,1145,494,1292,487,1292,480,1320,468,1343,481,1346,487,1386,486,1387,535,1467,551,1470,661,1484,667,1488,944,1494,953}
            };
            for(int i=0;i<6;i++)
            {
                var outline=outlines[i];float xMin=atlas.width,yMin=atlas.height,xMax=0,yMax=0;
                for(int v=0;v<outline.Length;v+=2){float x=outline[v]*atlas.width/1536f,y=outline[v+1]*atlas.height/1024f;xMin=Mathf.Min(xMin,x);xMax=Mathf.Max(xMax,x);yMin=Mathf.Min(yMin,y);yMax=Mathf.Max(yMax,y);}
                var rect=new Rect(xMin,atlas.height-yMax,xMax-xMin,yMax-yMin);float ppu=rect.width/7.5f;
                var sprite=Sprite.Create(atlas,rect,new Vector2(.5f,0),ppu,0,SpriteMeshType.FullRect);
                int n=outline.Length/2;var vertices=new Vector2[n+1];var triangles=new ushort[n*3];vertices[n]=new Vector2(0,rect.height*.45f/ppu);
                for(int v=0;v<n;v++){vertices[v]=new Vector2((outline[v*2]*atlas.width/1536f-xMin-rect.width*.5f)/ppu,(yMax-outline[v*2+1]*atlas.height/1024f)/ppu);triangles[v*3]=(ushort)n;triangles[v*3+1]=(ushort)v;triangles[v*3+2]=(ushort)((v+1)%n);}
                sprite.OverrideGeometry(vertices,triangles);sprite.name="Quarteirão ilustrado / "+i;AssetDatabase.AddObjectToAsset(sprite,bank);buildings[i]=sprite;
            }
            cityBuildingArt=buildings;
            var used=new List<Rect>();var plots=Procedural("Piso dos lotes",16,(x,y)=>new Color(.48f,.45f,.38f));
            int count=0;
            // Deterministic street-side placement keeps saved coordinates stable between builds.
            for(int road=0;road<world.roadStarts.Length;road++)
            {
                Vector2 a=world.roadStarts[road],b=world.roadEnds[road],direction=(b-a).normalized,side=new Vector2(-direction.y,direction.x);
                float length=Vector2.Distance(a,b);
                foreach(float setback in new[]{10.5f,20f})
                for(float distance=3;distance<length;distance+=9)
                foreach(float sign in new[]{-1f,1f})
                {
                    Vector2 center=a+direction*distance+side*setback*sign;
                    var reserved=new Rect(center-new Vector2(4.3f,4.5f),new Vector2(8.6f,9));
                    if(!world.bounds.Contains(reserved.min)||!world.bounds.Contains(reserved.max))continue;
                    bool blocked=reserved.Overlaps(Mateus.HideoutBounds);foreach(var site in landscapeSites)if(reserved.Overlaps(new Rect(site-Vector2.one*7.5f,Vector2.one*15)))blocked=true;
                    foreach(var district in world.districtRects)if(reserved.Overlaps(district)){blocked=true;break;}
                    foreach(var prior in used)if(reserved.Overlaps(prior)){blocked=true;break;}
                    for(int j=0;j<world.roadStarts.Length&&!blocked;j++)
                    {
                        Vector2 from=world.roadStarts[j],to=world.roadEnds[j];
                        var corridor=Rect.MinMaxRect(Mathf.Min(from.x,to.x)-5.1f,Mathf.Min(from.y,to.y)-5.1f,Mathf.Max(from.x,to.x)+5.1f,Mathf.Max(from.y,to.y)+5.1f);
                        if(reserved.Overlaps(corridor))blocked=true;
                    }
                    if(blocked)continue;used.Add(reserved);
                    Vector2 feet=center-Vector2.up*3.5f;
                    var root=new GameObject("Cidade / quarteirão da rota / "+count);root.transform.position=feet;root.layer=8;
                    var floor=Visual("Pátio e acesso",plots,center,-9002,root.transform);floor.transform.localScale=new Vector3(8.4f/plots.bounds.size.x,8.6f/plots.bounds.size.y,1);
                    var sprite=buildings[count%6];Visual("Fachada e telhado",sprite,feet,WorldSort.At(feet.y),root.transform);
                    var solid=root.AddComponent<BoxCollider2D>();solid.size=new Vector2(6.2f,3.1f);solid.offset=Vector2.up*2;
                    if(setback>15){var path=Visual("Calçada contínua",plots,a+direction*distance+side*12.5f*sign+direction*4.0f,-9001,root.transform);path.transform.localScale=Mathf.Abs(side.x)>.5f?new Vector3(15/plots.bounds.size.x,1/plots.bounds.size.y,1):new Vector3(1/plots.bounds.size.x,15/plots.bounds.size.y,1);}
                    count++;
                }
            }
            world.streetBlocks=used.ToArray();
            AddRouteResidents(world);
            Debug.Log("PONTO_CITY_ROUTE_BUILDINGS / "+count);
            if(count<6)throw new System.InvalidOperationException("Insufficient roadside scenery: "+count);
        }
        static void AddRouteResidents(WorldHub world)
        {
            var used=new HashSet<Vector2Int>();int count=0;
            for(int i=0;i<world.roadStarts.Length&&count<24;i++)
            {
                Vector2 a=world.roadStarts[i],b=world.roadEnds[i],direction=(b-a).normalized,side=new Vector2(-direction.y,direction.x);
                if(Vector2.Distance(a,b)<15)continue;
                foreach(float sign in new[]{-1f,1f})
                {
                    Vector2 first=a+direction*5+side*4.25f*sign,last=b-direction*5+side*4.25f*sign,mid=(first+last)*.5f;
                    var key=new Vector2Int(Mathf.RoundToInt(mid.x/5),Mathf.RoundToInt(mid.y/5));if(!used.Add(key))continue;
                    bool blocked=false;foreach(var district in world.districtRects)if(district.Contains(mid))blocked=true;if(blocked)continue;
                    var root=new GameObject("Morador / calçada da rota / "+count);root.layer=13;root.transform.position=mid;
                    var body=root.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
                    root.AddComponent<CircleCollider2D>().radius=.24f;
                    var frames=cleanPeople[count%cleanPeople.Length];var walker=root.AddComponent<StreetWalker>();walker.frames=frames;walker.visual=Visual("Cidadão",frames[0],mid,WorldSort.At(mid.y),root.transform);walker.route=new[]{first,last};walker.speed=1.05f+(count%4)*.12f;count++;
                }
            }
        }
    }
}
