using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego.Editor
{
    public static class SpriteRepair022
    {
        public static void Apply(Scene scene)
        {
            var people=Polish021Builder.Read("Assets/PontoCego/UrbanArt/UI/pedestrians-clean.png");
            var cars=Polish021Builder.Read("Assets/PontoCego/UrbanArt/UI/vehicles-clean.png");
            var higor=Polish021Builder.Read("Assets/PontoCego/UrbanArt/References/higor.png");
            int[][] edges={new[]{31,174,193,459,480,625,642,886,916,1054,1072,1321,1354,1488,1514,1754},new[]{27,178,192,459,481,623,641,890,912,1059,1077,1326,1351,1489,1513,1754},new[]{28,181,197,455,481,623,641,887,911,1059,1074,1323,1347,1487,1510,1753},new[]{30,187,200,461,480,629,644,887,912,1057,1067,1327,1349,1480,1499,1754}};
            int[] tops={43,254,458,660},heights={198,195,199,198};
            var cache=new Dictionary<Sprite,Sprite>();
            Sprite Replace(Sprite old)
            {
                if(cache.TryGetValue(old,out var known))return known;
                var ped=Regex.Match(old.name,@"^(?:Cidadão|022 Cidadão) (\d+) / (\d+)");
                var car=Regex.Match(old.name,@"^(?:Carro limpo|022 Carro) (\d+).* / (\d+)");
                var driver=Regex.Match(old.name,@"^(?:Higor / movimento|022 Higor) (\d+)");
                Sprite result=old;
                if(ped.Success)
                {
                    int model=int.Parse(ped.Groups[1].Value),frame=int.Parse(ped.Groups[2].Value),col=(model%4)*4+frame,row=model/4;
                    int x=Mathf.RoundToInt(col*people.width/16f),right=Mathf.RoundToInt((col+1)*people.width/16f);
                    result=Polish021Builder.Cell(people,new RectInt(x,row==0?40:415,right-x,row==0?330:335),"022 Cidadão "+model+" / "+frame,300/1.85f);
                }
                else if(car.Success)
                {
                    int model=int.Parse(car.Groups[1].Value)%8,view=int.Parse(car.Groups[2].Value),row=model/2,col=model%2*4+view,x=edges[row][col*2];
                    result=Polish021Builder.Cell(cars,new RectInt(x-3,tops[row],edges[row][col*2+1]-x+6,heights[row]),"022 Carro "+model+" / "+view,(view%2==0?heights[row]:edges[row][col*2+1]-x)/3.4f);
                }
                else if(driver.Success)
                {
                    int frame=int.Parse(driver.Groups[1].Value);
                    result=Polish021Builder.Cell(higor,new RectInt(77+frame*54,188,54,86),"022 Higor "+frame,82/2.05f);
                }
                cache[old]=result;return result;
            }
            foreach(var root in scene.GetRootGameObjects())foreach(var walker in root.GetComponentsInChildren<StreetWalker>(true))walker.visual.transform.localPosition=Vector3.zero;
            foreach(var root in scene.GetRootGameObjects())foreach(var component in root.GetComponentsInChildren<Component>(true))
            {
                if(!component)continue;var serialized=new SerializedObject(component);var prop=serialized.GetIterator();bool changed=false;
                while(prop.Next(true))if(prop.propertyType==SerializedPropertyType.ObjectReference&&prop.objectReferenceValue is Sprite old)
                {var replacement=Replace(old);if(replacement!=old){prop.objectReferenceValue=replacement;changed=true;}}
                if(changed)serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            Debug.Log("SPRITE022_AUXILIARY_REFS / "+cache.Count);
        }
    }
}
