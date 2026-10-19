using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static Sprite[] locationHd;
        static bool LocationHDReady=>locationHd!=null&&locationHd.Length==6&&locationHd[0];

        /// <summary>Six isolated transparent cells, ordered facade/kiosk/hoop/scrap/watchhouse/park gate.</summary>
        static void PrepareLocationHD(Texture2D atlas)
        {
            if(!atlas)throw new ArgumentNullException(nameof(atlas));
            var pixels=atlas.GetPixels32();
            // The finished illustrations deliberately have unequal gutters; these top-origin regions preserve roofs and feet.
            RectInt[] regions={new RectInt(0,0,565,565),new RectInt(566,0,536,565),new RectInt(1103,0,433,565),new RectInt(0,566,566,458),new RectInt(567,566,510,458),new RectInt(1080,566,456,458)};
            string[] names={"Agência Riberal / fachada HD","Mocambo / quiosque HD","Mocambo / cesta HD","Ferro-velho / sucata HD","Ferro-velho / guarita HD","Mocambo / portal HD"};
            locationHd=new Sprite[6];
            for(int cell=0;cell<6;cell++)
            {
                var source=regions[cell];int cellWidth=source.width,cellHeight=source.height;
                int x0=source.x,y0=atlas.height-source.y-source.height,minX=cellWidth,minY=cellHeight,maxX=-1,maxY=-1,transparent=0;
                for(int y=0;y<cellHeight;y++)for(int x=0;x<cellWidth;x++)
                {
                    byte alpha=pixels[(y0+y)*atlas.width+x0+x].a;
                    if(alpha<8){transparent++;continue;}
                    minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);
                }
                if(maxX<minX||maxY<minY)throw new InvalidOperationException("Empty HD location sprite: "+names[cell]);
                if(transparent<cellWidth*cellHeight*.03f)Debug.LogWarning("HD sprite has little transparent gutter: "+names[cell]);
                minX=Mathf.Max(0,minX-2);minY=Mathf.Max(0,minY-2);maxX=Mathf.Min(cellWidth-1,maxX+2);maxY=Mathf.Min(cellHeight-1,maxY+2);
                var rect=new Rect(x0+minX,y0+minY,maxX-minX+1,maxY-minY+1);
                var sprite=Sprite.Create(atlas,rect,new Vector2(.5f,0),rect.width,0,SpriteMeshType.FullRect);sprite.name=names[cell];AssetDatabase.AddObjectToAsset(sprite,bank);locationHd[cell]=sprite;
            }
            Debug.Log("PONTO_LOCATION_HD_SPRITES / 6 transparent foot-anchored cells");
        }
        static SpriteRenderer LocationHDVisual(string name,int cell,Vector2 feet,float width,Transform parent=null)
        {
            if(!LocationHDReady)return null;
            var sprite=locationHd[cell];var sr=Visual(name,sprite,feet,WorldSort.At(feet.y),parent);sr.transform.localScale=Vector3.one*(width/sprite.bounds.size.x);
            // SpriteDefault uses the source alpha. GroundBlend is intentionally reserved for ground tiles.
            return sr;
        }
        static void LocationHDText(SpriteRenderer source,string caption,Vector2 localPoint,float width,float height,Transform parent)
        {
            var root=new GameObject("Inscrição apoiada / "+caption);root.transform.SetParent(parent,true);root.transform.position=source.transform.TransformPoint(localPoint);
            var text=root.AddComponent<TextMesh>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.text=caption;text.anchor=TextAnchor.MiddleCenter;text.fontSize=64;text.characterSize=.1f;text.color=new Color(.97f,.92f,.72f);
            var mesh=text.GetComponent<MeshRenderer>();mesh.sharedMaterial=text.font.material;mesh.sortingOrder=source.sortingOrder+1;
            var fit=root.AddComponent<WorldTextLabel>();fit.width=width;fit.height=height;
            var follow=root.AddComponent<GroundedSpriteLabel>();follow.source=source;follow.caption=text;
        }
        static void RefreshAgencyFacade(WorldHub world)
        {
            if(!LocationHDReady)return;
            var agency=world.Find("01_Riberal");if(agency==null)return;
            foreach(var state in UnityEngine.Object.FindObjectsByType<InteriorFacade2D>())
            {
                if(Vector2.Distance(state.footprint.center,agency.center)>.1f)continue;
                var retained=new List<SpriteRenderer>();
                foreach(var old in state.exterior)
                {
                    if(!old)continue;
                    if(old.name.StartsWith("Fachada /")||old.name.StartsWith("Fachada HD /")){UnityEngine.Object.DestroyImmediate(old.gameObject);continue;}
                    retained.Add(old);
                }
                Vector2 feet=new Vector2(agency.center.x,state.footprint.yMin+.35f);
                float facadeWidth=Mathf.Min(32,22/locationHd[0].bounds.size.y);
                var facade=LocationHDVisual("Fachada HD / AGÊNCIA RIBERAL",0,feet,facadeWidth,state.transform);
                LocationHDText(facade,"AGÊNCIA RIBERAL",new Vector2(0,.375f),facadeWidth*.46f,facadeWidth*.055f,state.transform);
                retained.Add(facade);state.exterior=retained.ToArray();
                // Keep the original cutaway, walls, furniture and every story interaction for this short release.
                Debug.Log("PONTO_RIBERAL_HD_FACADE / original interior and mission coordinates preserved");break;
            }
        }
    }
}
