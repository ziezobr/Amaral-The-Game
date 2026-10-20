using UnityEngine;
namespace PontoCego
{
    /// <summary>One raster surface per radar. No rotated GUI matrices, nested clip groups or atlas fragments.</summary>
    public sealed class CityRadar2D:MonoBehaviour
    {
        Texture2D compact,full;float compactNext,fullNext;Color32[] compactPixels,fullPixels;SceneryFootprint2D[] footprints;EquipmentShop shop;
        public const int CompactWidth=320,CompactHeight=224;
        public static Vector2 Project(Vector2 point,Rect world,Vector2 pixels)=>new Vector2((point.x-world.xMin)/world.width*pixels.x,(point.y-world.yMin)/world.height*pixels.y);
        public static Rect LocalView(Rect bounds,Vector2 center,float aspect)
        {
            var size=new Vector2(58,58/aspect);center.x=Mathf.Clamp(center.x,bounds.xMin+size.x*.5f,bounds.xMax-size.x*.5f);center.y=Mathf.Clamp(center.y,bounds.yMin+size.y*.5f,bounds.yMax-size.y*.5f);return new Rect(center-size*.5f,size);
        }
        public static bool ClipSegment(ref Vector2 a,ref Vector2 b,Rect bounds)
        {
            Vector2 d=b-a;float first=0,last=1;
            bool Edge(float p,float q){if(Mathf.Abs(p)<.00001f)return q>=0;float t=q/p;if(p<0){if(t>last)return false;first=Mathf.Max(first,t);}else{if(t<first)return false;last=Mathf.Min(last,t);}return true;}
            if(!Edge(-d.x,a.x-bounds.xMin)||!Edge(d.x,bounds.xMax-a.x)||!Edge(-d.y,a.y-bounds.yMin)||!Edge(d.y,bounds.yMax-a.y))return false;
            b=a+d*last;a+=d*first;return true;
        }
        public void Draw(Rect area,bool local,WorldHub world,PlayerMotor player,Vector2[] route)
        {
            if(!world||Event.current.type!=EventType.Repaint)return;
            if(footprints==null){footprints=Object.FindObjectsByType<SceneryFootprint2D>();shop=Object.FindAnyObjectByType<EquipmentShop>();}
            int width=local?CompactWidth:640,height=local?CompactHeight:480;
            Texture2D tex=local?compact:full;Color32[] pixels=local?compactPixels:fullPixels;
            if(!tex){tex=new Texture2D(width,height,TextureFormat.RGBA32,false){name=local?"Radar contínuo 1080p":"Mapa da cidade",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};pixels=new Color32[width*height];if(local){compact=tex;compactPixels=pixels;}else{full=tex;fullPixels=pixels;}}
            float next=local?compactNext:fullNext;
            if(Time.unscaledTime>=next)
            {
                if(local)compactNext=Time.unscaledTime+.1f;else fullNext=Time.unscaledTime+.25f;
                Rect view=local&&player?LocalView(world.bounds,player.transform.position,area.width/area.height):world.bounds;
                Render(pixels,width,height,view,world,player,route);tex.SetPixels32(pixels);tex.Apply(false,false);
            }
            var old=GUI.color;GUI.color=Color.white;
            CleanGui.Round(new Rect(area.x-5,area.y-5,area.width+10,area.height+10),new Color(.015f,.02f,.03f,.98f));
            GUI.DrawTexture(area,tex,ScaleMode.StretchToFill,false);
            GUI.Label(new Rect(area.xMax-24,area.y+3,20,22),"N",new GUIStyle(UiTheme.Label(12,Color.white,true)){alignment=TextAnchor.MiddleCenter});
            GUI.color=old;
        }
        void Render(Color32[] pixels,int w,int h,Rect view,WorldHub world,PlayerMotor player,Vector2[] route)
        {
            Color32 land=new Color(.19f,.24f,.21f),block=new Color(.35f,.39f,.38f),curb=new Color(.07f,.09f,.1f),road=new Color(.72f,.75f,.72f);
            for(int i=0;i<pixels.Length;i++)pixels[i]=land;
            Vector2 P(Vector2 p)=>Project(p,view,new Vector2(w,h));
            void Pixel(int x,int y,Color32 c){if(x>=0&&x<w&&y>=0&&y<h)pixels[y*w+x]=c;}
            void Box(Rect r,Color32 c){var a=P(r.min);var b=P(r.max);for(int y=Mathf.Clamp(Mathf.FloorToInt(a.y),0,h);y<Mathf.Clamp(Mathf.CeilToInt(b.y),0,h);y++)for(int x=Mathf.Clamp(Mathf.FloorToInt(a.x),0,w);x<Mathf.Clamp(Mathf.CeilToInt(b.x),0,w);x++)Pixel(x,y,c);}
            void Disc(Vector2 p,int radius,Color32 c){int cx=Mathf.RoundToInt(p.x),cy=Mathf.RoundToInt(p.y);for(int y=-radius;y<=radius;y++)for(int x=-radius;x<=radius;x++)if(x*x+y*y<=radius*radius)Pixel(cx+x,cy+y,c);}
            void Line(Vector2 from,Vector2 to,int radius,Color32 c){if(!ClipSegment(ref from,ref to,new Rect(-radius,-radius,w+radius*2,h+radius*2)))return;Vector2 d=to-from;if(Mathf.Abs(d.x)<.1f||Mathf.Abs(d.y)<.1f){int x0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(from.x,to.x))-radius,0,w),x1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(from.x,to.x))+radius,0,w),y0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(from.y,to.y))-radius,0,h),y1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(from.y,to.y))+radius,0,h);for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)Pixel(x,y,c);return;}int count=Mathf.Clamp(Mathf.CeilToInt(d.magnitude),1,1000);for(int i=0;i<=count;i++)Disc(Vector2.Lerp(from,to,i/(float)count),radius,c);}
            if(world.streetBlocks!=null)foreach(var r in world.streetBlocks)Box(r,block);
            if(world.districtRects!=null)foreach(var r in world.districtRects)Box(r,new Color(.31f,.37f,.32f));
            foreach(var contract in footprints)if(contract)Box(contract.groundBounds,block);
            int roadSize=Mathf.Max(2,Mathf.RoundToInt(3.2f*w/view.width));
            for(int i=0;i<world.roadStarts.Length;i++)Line(P(world.roadStarts[i]),P(world.roadEnds[i]),roadSize+2,curb);
            for(int i=0;i<world.roadStarts.Length;i++)Line(P(world.roadStarts[i]),P(world.roadEnds[i]),roadSize,road);
            if(route!=null)for(int i=1;i<route.Length;i++){Line(P(route[i-1]),P(route[i]),3,curb);Line(P(route[i-1]),P(route[i]),1,UiTheme.Route);}
            if(route!=null&&route.Length>1){Vector2 goal=P(route[route.Length-1]);Disc(goal,6,curb);Disc(goal,4,UiTheme.Destination);}
            if(MapWaypoint.Has){Vector2 mark=P(MapWaypoint.Position);Disc(mark,7,curb);Disc(mark,5,new Color(1,.3f,.75f));}
            if(shop){Vector2 at=P(shop.transform.position);Disc(at,5,curb);Disc(at,3,new Color(.35f,.85f,.5f));}
            if(player)
            {
                Vector2 at=P(player.transform.position),facing=player.Model.Facing.normalized,side=new Vector2(-facing.y,facing.x);
                Disc(at,7,curb);Line(at-facing*3,at+facing*7,2,Color.white);Line(at+facing*7,at-facing*2+side*4,1,Color.white);Line(at+facing*7,at-facing*2-side*4,1,Color.white);
            }
        }
        void OnDestroy(){if(compact)Destroy(compact);if(full)Destroy(full);}
    }
}
