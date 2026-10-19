using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace PontoCego.Editor
{
 public static class DemoVisualFinish
 {
  public static void Build()
  {
   foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(UrbanSceneBuilder.Root+"/Sprites.asset"))
   {
    if(!(obj is Texture2D tex)||!(tex.name.StartsWith("Higor / ")||tex.name.StartsWith("ribeiro / ")||tex.name.StartsWith("cristyan / ")||tex.name.StartsWith("henzo / ")||tex.name.StartsWith("martins / ")||tex.name.StartsWith("kevyn / ")||tex.name.StartsWith("paulo / ")||tex.name.StartsWith("Carro ")||tex.name.StartsWith("Cidad")))continue;
    var pix=tex.GetPixels();int w=tex.width,h=tex.height;var seen=new bool[pix.Length];var q=new Queue<int>();
    void Seed(int i){if(seen[i])return;var c=pix[i];if(c.a>.01f&&(c.r>.16f||c.g>.19f||c.b>.22f||c.b<c.r+.01f||c.g<c.r+.006f))return;seen[i]=true;q.Enqueue(i);}
    for(int x=0;x<w;x++){Seed(x);Seed((h-1)*w+x);}for(int y=0;y<h;y++){Seed(y*w);Seed(y*w+w-1);}
    while(q.Count>0){int i=q.Dequeue(),x=i%w,y=i/w;pix[i]=Color.clear;if(x>0)Seed(i-1);if(x<w-1)Seed(i+1);if(y>0)Seed(i-w);if(y<h-1)Seed(i+w);}
    if(tex.name.StartsWith("Higor / "))for(int y=0;y<h;y++)for(int x=0;x<3;x++)pix[y*w+x]=Color.clear;
    tex.SetPixels(pix);tex.Apply();EditorUtility.SetDirty(tex);
   }
   AssetDatabase.SaveAssets();BuildProject.BuildWindows();
  }
 }
}
