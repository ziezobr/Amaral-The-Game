using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PontoCego.Editor
{
    public static class Fluid023Builder
    {
        const string Bank="Assets/PontoCego/Urban/Fluid023.asset";
        public static void Apply()
        {
            AssetDatabase.Refresh();var source=Polish021Builder.Read("Assets/PontoCego/UrbanArt/References/amaral-fluid-walk-v1.png");
            var bank=AssetDatabase.LoadAssetAtPath<UrbanSpriteBank>(Bank);if(!bank){bank=ScriptableObject.CreateInstance<UrbanSpriteBank>();AssetDatabase.CreateAsset(bank,Bank);}
            var existing=AssetDatabase.LoadAllAssetsAtPath(Bank).OfType<Sprite>().ToDictionary(s=>s.name);var sprites=new Sprite[40];
            for(int col=0;col<8;col++)
            {
                int x0=Mathf.RoundToInt(col*source.width/8f),x1=Mathf.RoundToInt((col+1)*source.width/8f),w=x1-x0,h=source.height;
                var pixels=source.GetPixels(x0,0,w,h);var seen=new bool[pixels.Length];var queue=new Queue<int>();var bodies=new List<List<int>>();
                for(int seed=0;seed<pixels.Length;seed++)
                {
                    if(seen[seed]||pixels[seed].a<.55f)continue;var body=new List<int>();seen[seed]=true;queue.Enqueue(seed);
                    while(queue.Count>0){int p=queue.Dequeue();body.Add(p);int x=p%w,y=p/w;void Add(int n){if(!seen[n]&&pixels[n].a>=.55f){seen[n]=true;queue.Enqueue(n);}}if(x>0)Add(p-1);if(x<w-1)Add(p+1);if(y>0)Add(p-w);if(y<h-1)Add(p+w);}
                    if(body.Count>1800)bodies.Add(body);
                }
                var ordered=bodies.OrderByDescending(b=>b.Count).Take(5).OrderByDescending(b=>b.Max(p=>p/w)).ToArray();
                if(ordered.Length!=5)throw new Exception("Expected five complete walk poses in column "+col+", found "+ordered.Length);
                for(int f=0;f<5;f++)
                {
                    string name="Amaral fluido / "+col+" / "+f;if(existing.TryGetValue(name,out var known)){sprites[col*5+f]=known;continue;}
                    var body=ordered[f];int left=body.Min(p=>p%w),right=body.Max(p=>p%w),bottom=body.Min(p=>p/w),top=body.Max(p=>p/w);
                    if(left<2||right>w-3||top-bottom<130)throw new Exception("Clipped or partial pose: "+name);
                    int tw=right-left+9,th=top-bottom+9;var texture=new Texture2D(tw,th,TextureFormat.RGBA32,false){name=name+" pixels",filterMode=FilterMode.Bilinear};var output=new Color[tw*th];
                    foreach(int p in body)output[(p/w-bottom+4)*tw+p%w-left+4]=pixels[p];
                    texture.SetPixels(output);texture.Apply();AssetDatabase.AddObjectToAsset(texture,bank);
                    var sprite=Sprite.Create(texture,new Rect(0,0,tw,th),new Vector2(.5f,4f/th), (top-bottom+1)/2.05f,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,bank);sprites[col*5+f]=sprite;
                }
            }
            bank.amaralWalk=sprites;EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();
            int actors=0;
            foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
            {
                var scene=EditorSceneManager.OpenScene(entry.path);
                foreach(var root in scene.GetRootGameObjects())foreach(var player in root.GetComponentsInChildren<PlayerMotor>(true))
                {
                    var a=player.GetComponent<ReferenceActorAnimation>();if(!a)continue;var poses=new Sprite[40];for(int d=0;d<8;d++)for(int f=0;f<5;f++)poses[d*5+f]=sprites[(d==3?5:d)*5+f];
                    a.walk=poses;a.run=poses;a.mirroredWalkDirections=new bool[8];a.mirroredWalkDirections[3]=true;actors++;
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            Debug.Log("FLUID023_APPLIED / five poses x eight directions / players="+actors);
        }
    }
}
