using UnityEditor;
using UnityEngine;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static Sprite[][] cleanCars,cleanPeople;static Sprite[] foodIcons,questIcons;
        static Sprite CleanCell(Texture2D t,RectInt rect,string name,float size)
        {var s=ReferenceCell(t,rect,name,1);float factor=size/Mathf.Max(s.bounds.size.x,s.bounds.size.y);var n=Sprite.Create(s.texture,s.rect,new Vector2(.5f,.1f),s.pixelsPerUnit/factor);n.name=name+" dimensionado";AssetDatabase.AddObjectToAsset(n,bank);return n;}
        static void LoadFixedAssets()
        {
            var cars=Import(Art+"/UI/vehicles-clean.png");cleanCars=new Sprite[8][];
            int[][] edges={new[]{31,174,193,459,480,625,642,886,916,1054,1072,1321,1354,1488,1514,1754},new[]{27,178,192,459,481,623,641,890,912,1059,1077,1326,1351,1489,1513,1754},new[]{28,181,197,455,481,623,641,887,911,1059,1074,1323,1347,1487,1510,1753},new[]{30,187,200,461,480,629,644,887,912,1057,1067,1327,1349,1480,1499,1754}};
            int[] top={50,263,466,670},height={185,184,188,184};
            for(int model=0;model<8;model++){cleanCars[model]=new Sprite[4];for(int view=0;view<4;view++){int col=(model%2)*4+view,row=model/2,x=edges[row][col*2];cleanCars[model][view]=CleanCell(cars,new RectInt(x-2,top[row],edges[row][col*2+1]-x+4,height[row]),"Carro limpo "+model+" / "+view,view%2==0?2.9f:3.5f);}}
            var supplied=Import(Art+"/References/cars.png");var peugeotRects=new[]{new RectInt(22,43,115,92),new RectInt(143,43,181,92),new RectInt(332,43,111,92),new RectInt(450,43,192,92)};for(int v=0;v<4;v++)cleanCars[0][v]=CleanCell(supplied,peugeotRects[v],"Carro limpo 0 / Peugeot 206 cinza / "+v,3.4f);
            var people=Import(Art+"/UI/pedestrians-clean.png");cleanPeople=new Sprite[8][];
            for(int model=0;model<8;model++){cleanPeople[model]=new Sprite[4];for(int frame=0;frame<4;frame++){int col=(model%4)*4+frame,row=model/4;int x=Mathf.RoundToInt(col*people.width/16f),next=Mathf.RoundToInt((col+1)*people.width/16f);cleanPeople[model][frame]=CleanCell(people,new RectInt(x,row==0?40:415,next-x,row==0?330:335),"Cidadão "+model+" / "+frame,1.85f);}}
            var food=Import(Art+"/UI/food-clean.png");foodIcons=new Sprite[5];int[] xs={18,452,836,1358,1765},ws={433,380,516,398,391};for(int i=0;i<5;i++)foodIcons[i]=CleanCell(food,new RectInt(xs[i],25,ws[i],685),"Alimento "+i,.7f);
            var quest=Import(Art+"/UI/quest-clean.png");questIcons=new Sprite[6];int[] qx={0,300,660,974,1475,1825},qw={360,355,310,494,350,340};for(int i=0;i<6;i++)questIcons[i]=CleanCell(quest,new RectInt(qx[i],30,qw[i],675),"Equipamento "+i,i==5?1.6f:i==3?2.5f:.85f);
        }
        static Vector2 SafeCollectible(Vector2 candidate)
        {Physics2D.SyncTransforms();for(int i=0;i<160;i++){var p=candidate+new Vector2((i%13-6)*.65f,(i/13-6)*.65f);if(!Physics2D.OverlapCircle(p,.65f,LayerMask.GetMask("World")))return p;}return candidate;}
        static void HideInBush(GameObject root,SpriteRenderer item)
        {var cover=Visual("Moita / esconderijo",questIcons[5],root.transform.position,20000,root.transform);cover.transform.localPosition=Vector3.up*.3f;var hide=root.AddComponent<CollectibleCover>();hide.item=item;hide.cover=cover;}
        static void AddExplorationCollectibles()
        {
            var points=new[]{new Vector2(-34,24),new Vector2(-37,-24),new Vector2(65,29),new Vector2(83,-25),new Vector2(-27,-72),new Vector2(75,-71)};
            for(int i=0;i<points.Length;i++){var go=new GameObject("Peça escondida da cidade / "+PcCollectible.Names[i]);go.layer=10;go.transform.position=SafeCollectible(points[i]);go.AddComponent<CircleCollider2D>().isTrigger=true;var part=go.AddComponent<PcCollectible>();part.kind=i;part.id="pc:city:"+i;var sr=Visual("Peça",pcIcons[i],go.transform.position,20000,go.transform);sr.transform.localScale=Vector3.one*(.65f/Mathf.Max(sr.sprite.bounds.size.x,sr.sprite.bounds.size.y));HideInBush(go,sr);}
        }
    }
}
