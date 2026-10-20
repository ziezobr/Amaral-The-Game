using System;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace PontoCego.Editor
{
    public static class Features025Builder
    {
        public static void Apply()
        {
            const string path="Assets/PontoCego/Urban/Equipment025.asset";AssetDatabase.Refresh();var source=Polish021Builder.Read("Assets/PontoCego/UrbanArt/References/amaral-held-pink-v1.png");
            var bank=AssetDatabase.LoadAssetAtPath<UrbanSpriteBank>(path);if(!bank){bank=ScriptableObject.CreateInstance<UrbanSpriteBank>();AssetDatabase.CreateAsset(bank,path);}var existing=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);var sprites=new Sprite[24];
            int w=source.width,h=source.height;var pixels=source.GetPixels();var seen=new bool[pixels.Length];var queue=new Queue<int>();var bodies=new List<List<int>>();
            for(int seed=0;seed<pixels.Length;seed++)
            {if(seen[seed]||pixels[seed].a<.55f)continue;var body=new List<int>();seen[seed]=true;queue.Enqueue(seed);while(queue.Count>0){int at=queue.Dequeue();body.Add(at);int x=at%w,y=at/w;void Add(int n){if(!seen[n]&&pixels[n].a>=.55f){seen[n]=true;queue.Enqueue(n);}}if(x>0)Add(at-1);if(x<w-1)Add(at+1);if(y>0)Add(at-w);if(y<h-1)Add(at+w);}if(body.Count>1500)bodies.Add(body);}
            var all=bodies.OrderByDescending(b=>b.Count).Take(24).OrderByDescending(b=>b.Max(i=>i/w)).ToArray();if(all.Length!=24)throw new Exception("Expected 24 full pink-camera bodies");
            var rows=Enumerable.Range(0,3).Select(row=>all.Skip(row*8).Take(8).OrderBy(b=>b.Min(i=>i%w)).ToArray()).ToArray();
            for(int col=0;col<8;col++)
            {
                var ordered=Enumerable.Range(0,3).Select(row=>rows[row][col]).ToArray();
                for(int frame=0;frame<3;frame++)
                {
                    string name="Amaral / câmera rosa nas mãos / "+col+" / "+frame;if(existing.TryGetValue(name,out var known)){sprites[col*3+frame]=known;continue;}
                    var body=ordered[frame];int left=body.Min(i=>i%w),right=body.Max(i=>i%w),bottom=body.Min(i=>i/w),top=body.Max(i=>i/w);if(left<2||right>w-3||top-bottom<150)throw new Exception("Clipped pink pose: "+name);
                    int tw=right-left+9,th=top-bottom+9;var tex=new Texture2D(tw,th,TextureFormat.RGBA32,false){name=name+" pixels",filterMode=FilterMode.Bilinear};var output=new Color[tw*th];foreach(int at in body)output[(at/w-bottom+4)*tw+at%w-left+4]=pixels[at];tex.SetPixels(output);tex.Apply();AssetDatabase.AddObjectToAsset(tex,bank);
                    var sprite=Sprite.Create(tex,new Rect(0,0,tw,th),new Vector2(.5f,4f/th),(top-bottom+1)/2.05f,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,bank);sprites[col*3+frame]=sprite;
                }
            }
            bank.amaralWalk=sprites;EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();int count=0;
            foreach(var scenePath in EditorBuildSettings.scenes.Where(s=>s.enabled))
            {
                var scene=EditorSceneManager.OpenScene(scenePath.path);var mateus=UnityEngine.Object.FindAnyObjectByType<Mateus>();
                foreach(var root in scene.GetRootGameObjects())foreach(var player in root.GetComponentsInChildren<PlayerMotor>(true))
                {var equipment=player.GetComponent<PlayerEquipment2D>();if(!equipment)equipment=player.gameObject.AddComponent<PlayerEquipment2D>();equipment.pinkIdle=Enumerable.Range(0,8).Select(d=>sprites[d*3]).ToArray();equipment.pinkWalk=sprites;if(mateus)equipment.pinkIcon=mateus.cameraIcon;if(!player.GetComponent<PlayerEquipmentVisual2D>())player.gameObject.AddComponent<PlayerEquipmentVisual2D>();count++;}
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            Debug.Log("FEATURES025_APPLIED / "+count+" players / 24 pink camera poses");
        }
    }
}
