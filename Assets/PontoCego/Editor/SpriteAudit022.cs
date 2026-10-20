using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PontoCego.Editor
{
    public static class SpriteAudit022
    {
        public static void Export()
        {
            EditorSceneManager.OpenScene("Assets/PontoCego/Urban/Scenes/26_PatosWorld.unity");
            string folder="Validation/SpriteAudit022";Directory.CreateDirectory(folder);
            foreach(var a in UnityEngine.Object.FindObjectsByType<ReferenceActorAnimation>(FindObjectsInactive.Include).GroupBy(a=>a.idle[0].name.Split('/')[0]).Select(g=>g.First()))
                Page(folder,a.idle[0].name.Split('/')[0].Trim(),a.idle.Concat(a.walk).Concat(a.run??Array.Empty<Sprite>()).Concat(a.idleCycle??Array.Empty<Sprite>()).ToArray(),8);
            var player=UnityEngine.Object.FindAnyObjectByType<DirectionalCombatSprites>();Page(folder,"Ataques_Amaral",player.poses,8);
            Page(folder,"Pedestres",UnityEngine.Object.FindObjectsByType<StreetWalker>(FindObjectsInactive.Include).SelectMany(w=>w.frames).Distinct().ToArray(),8);
            Page(folder,"Carros",UnityEngine.Object.FindObjectsByType<CityTraffic>(FindObjectsInactive.Include).SelectMany(w=>w.views).Distinct().ToArray(),8);
            Page(folder,"Higor",UnityEngine.Object.FindObjectsByType<HigorMartialArts>(FindObjectsInactive.Include).First().frames,7);
            foreach(var a in UnityEngine.Object.FindObjectsByType<AllyAgent>(FindObjectsInactive.Include).GroupBy(a=>a.name.Split('/')[0]).Select(g=>g.First()))Page(folder,"Combate_"+a.name.Split('/')[0].Trim(),a.poses,4);
            Debug.Log("SPRITE_AUDIT_EXPORTED");
        }
        static void Page(string folder,string name,Sprite[] sprites,int cols)
        {
            const int cw=144,ch=180;int rows=Mathf.CeilToInt(sprites.Length/(float)cols);var t=new Texture2D(cols*cw,rows*ch,TextureFormat.RGBA32,false);var data=Enumerable.Repeat(new Color(.18f,.20f,.22f,1),t.width*t.height).ToArray();
            for(int i=0;i<sprites.Length;i++)
            {
                var s=sprites[i];if(!s)continue;var r=s.rect;float scale=Mathf.Min((cw-12)/r.width,(ch-14)/r.height);int w=Mathf.RoundToInt(r.width*scale),h=Mathf.RoundToInt(r.height*scale);int x0=(i%cols)*cw+(cw-w)/2,y0=(rows-1-i/cols)*ch+5;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++){var c=s.texture.GetPixel(Mathf.Clamp((int)(r.x+x/scale),0,s.texture.width-1),Mathf.Clamp((int)(r.y+y/scale),0,s.texture.height-1));int at=(y0+y)*t.width+x0+x;data[at]=Color.Lerp(data[at],c,c.a);}
            }
            t.SetPixels(data);t.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
        }
    }
}
