using System.Collections.Generic;
using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static void BuildStreetConnections(WorldHub world)
        {
            var sidewalk=Procedural("Calçada / concreto",16,(x,y)=>new Color(.46f+(x%7==0||y%7==0?-.06f:0),.46f,.43f));
            var metal=Procedural("Mobiliário / metal",4,(x,y)=>new Color(.19f,.23f,.25f));
            var wood=Procedural("Mobiliário / madeira",16,(x,y)=>new Color(.43f+(y%5==0?-.055f:.015f),.27f,.14f));
            var concrete=Procedural("Mobiliário / base de concreto",4,(x,y)=>new Color(.52f,.51f,.46f));
            var shade=Procedural("Mobiliário / sombra de contato",64,(x,y)=>
            {float d=new Vector2((x-31.5f)/31.5f,(y-31.5f)/31.5f).magnitude;return new Color(.03f,.04f,.025f,Mathf.Clamp01(1-d)*.30f);});
            var glow=Procedural("Luz da rua",64,(x,y)=>{float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;return new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2));});
            var used=new HashSet<Vector2Int>();
            for(int i=0;i<world.roadStarts.Length;i++)
            {
                Vector2 a=world.roadStarts[i],b=world.roadEnds[i],dir=(b-a).normalized,side=new Vector2(-dir.y,dir.x);float length=Vector2.Distance(a,b);
                if(!canonicalRoadNetworkReady)foreach(float sign in new[]{-1f,1f})
                {var sr=Visual("Calçada contínua",sidewalk,(a+b)*.5f+side*4.25f*sign,-9001);bool horizontal=Mathf.Abs(dir.x)>.5f;sr.transform.localScale=new Vector3((horizontal?length+1:1.4f)/sidewalk.bounds.size.x,(horizontal?1.4f:length+1)/sidewalk.bounds.size.y,1);}
                for(float d=8;d<length-5;d+=16)
                {
                    Vector2 at=a+dir*d+side*5.3f;
                    var key=new Vector2Int(Mathf.RoundToInt(at.x/5),Mathf.RoundToInt(at.y/5));
                    bool horizontal=Mathf.Abs(dir.x)>.5f;
                    Vector2 baySize=horizontal?new Vector2(4.8f,1.8f):new Vector2(1.8f,4.8f);
                    var bay=new Rect(at+dir+side*.2f-baySize*.5f,baySize);
                    if(!used.Add(key)||!StreetFurnitureBayClear(world,bay))continue;
                    var pad=Visual("Calçada / baía de descanso",sidewalk,bay.center,-9002);
                    pad.transform.localScale=new Vector3(bay.width/sidewalk.bounds.size.x,bay.height/sidewalk.bounds.size.y,1);
                    // Each object has its own foot anchor and collision footprint; no shared floating bar.
                    var poleRoot=new GameObject("Mobiliário da rua / poste com base");poleRoot.transform.position=at;poleRoot.layer=8;
                    poleRoot.AddComponent<BoxCollider2D>().size=new Vector2(.16f,.25f);
                    FurniturePart("Sombra / poste",shade,poleRoot.transform,new Vector2(0,-.025f),new Vector2(.48f,.17f),-3);
                    FurniturePart("Base do poste",concrete,poleRoot.transform,Vector2.up*.035f,new Vector2(.28f,.14f),0);
                    FurniturePart("Poste / haste",metal,poleRoot.transform,Vector2.up*1.12f,new Vector2(.10f,2.12f),1);
                    FurniturePart("Poste / braço",metal,poleRoot.transform,new Vector2(.18f,2.13f),new Vector2(.48f,.09f),2);
                    FurniturePart("Poste / luminária",metal,poleRoot.transform,new Vector2(.38f,2.06f),new Vector2(.39f,.23f),3);
                    var bulb=FurniturePart("Poste / lâmpada",concrete,poleRoot.transform,new Vector2(.38f,2.03f),new Vector2(.25f,.12f),4);
                    var light=FurniturePart("Halo noturno",glow,poleRoot.transform,Vector2.zero,Vector2.one*5,5);
                    var lamp=poleRoot.AddComponent<WorldLamp2D>();lamp.glow=light;lamp.bulb=bulb;

                    var benchRoot=new GameObject("Mobiliário da rua / banco com pés");benchRoot.transform.position=at+dir*2;benchRoot.layer=8;
                    var benchBody=benchRoot.AddComponent<BoxCollider2D>();benchBody.size=new Vector2(1.52f,.35f);benchBody.offset=Vector2.up*.08f;
                    FurniturePart("Sombra / banco",shade,benchRoot.transform,new Vector2(0,.015f),new Vector2(1.65f,.35f),-3);
                    foreach(float x in new[]{-.59f,.59f})
                    {
                        FurniturePart("Banco / pé no chão",metal,benchRoot.transform,new Vector2(x,.20f),new Vector2(.11f,.43f),0);
                        FurniturePart("Banco / suporte do encosto",metal,benchRoot.transform,new Vector2(x,.49f),new Vector2(.075f,.56f),1);
                    }
                    FurniturePart("Banco / encosto de madeira",wood,benchRoot.transform,new Vector2(0,.68f),new Vector2(1.55f,.25f),2);
                    FurniturePart("Banco / assento de madeira",wood,benchRoot.transform,new Vector2(0,.34f),new Vector2(1.62f,.23f),3);
                    FurniturePart("Banco / borda frontal",wood,benchRoot.transform,new Vector2(0,.24f),new Vector2(1.62f,.075f),4);
                }
            }
        }
        static SpriteRenderer FurniturePart(string label,Sprite sprite,Transform parent,Vector2 offset,Vector2 size,int order)
        {
            var sr=Visual(label,sprite,(Vector2)parent.position+offset,WorldSort.At(parent.position.y)+order,parent);
            sr.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);return sr;
        }
        static bool StreetFurnitureBayClear(WorldHub world,Rect bay)
        {
            if(!world.bounds.Contains(bay.min)||!world.bounds.Contains(bay.max))return false;
            for(int i=0;i<world.roadStarts.Length;i++)
            {
                var a=world.roadStarts[i];var b=world.roadEnds[i];
                var asphalt=Rect.MinMaxRect(Mathf.Min(a.x,b.x)-3.6f,Mathf.Min(a.y,b.y)-3.6f,Mathf.Max(a.x,b.x)+3.6f,Mathf.Max(a.y,b.y)+3.6f);
                if(bay.Overlaps(asphalt))return false;
            }
            var padded=bay;padded.min-=Vector2.one*.5f;padded.max+=Vector2.one*.5f;
            foreach(var mission in world.missions)
            {
                if(padded.Contains(mission.entry))return false;
                if(mission.level&&mission.level.objectiveTargets!=null)foreach(var target in mission.level.objectiveTargets)if(target&&padded.Contains(target.position))return false;
            }
            return !Physics2D.OverlapBox(bay.center,bay.size*.92f,0,LayerMask.GetMask("World"));
        }
    }
}
