using System;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace PontoCego.Editor
{
    public static class Coherent026Builder
    {
        static Sprite[] Import(string name,int columns=8,int rowCount=3,bool vehicle=false)
        {
            const string folder="Assets/PontoCego/Urban/Animation026";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/PontoCego/Urban","Animation026");string bankPath=folder+"/"+name+".asset";var bank=AssetDatabase.LoadAssetAtPath<UrbanSpriteBank>(bankPath);if(!bank){bank=ScriptableObject.CreateInstance<UrbanSpriteBank>();AssetDatabase.CreateAsset(bank,bankPath);}
            var known=AssetDatabase.LoadAllAssetsAtPath(bankPath).OfType<Sprite>().Where(s=>s.name.StartsWith(name+" / coerente026 /")).OrderBy(s=>s.name).ToArray();if(known.Length==columns*rowCount)return known;
            string path="Assets/PontoCego/UrbanArt/References/"+name+"-coherent-v3.png";if(!System.IO.File.Exists(path))return null;
            var source=Polish021Builder.Read(path);int w=source.width,h=source.height;var pixels=source.GetPixels();var seen=new bool[pixels.Length];var queue=new Queue<int>();var bodies=new List<List<int>>();
            for(int seed=0;seed<pixels.Length;seed++)
            {if(seen[seed]||pixels[seed].a<.55f)continue;var body=new List<int>();seen[seed]=true;queue.Enqueue(seed);while(queue.Count>0){int at=queue.Dequeue();body.Add(at);int x=at%w,y=at/w;void Add(int n){if(!seen[n]&&pixels[n].a>=.55f){seen[n]=true;queue.Enqueue(n);}}if(x>0)Add(at-1);if(x<w-1)Add(at+1);if(y>0)Add(at-w);if(y<h-1)Add(at+w);}if(body.Count>1000)bodies.Add(body);}
            var all=bodies.OrderByDescending(b=>b.Count).Take(columns*rowCount).OrderByDescending(b=>b.Max(i=>i/w)).ToArray();if(all.Length!=columns*rowCount)throw new Exception(name+": expected 24 complete bodies, got "+all.Length);
            var rows=Enumerable.Range(0,rowCount).Select(row=>all.Skip(row*columns).Take(columns).OrderBy(b=>b.Min(i=>i%w)).ToArray()).ToArray();var sprites=new Sprite[columns*rowCount];
            for(int direction=0;direction<columns;direction++)for(int frame=0;frame<rowCount;frame++)
            {
                string label=name+" / coerente026 / "+(vehicle?frame:direction)+" / "+(vehicle?direction:frame);var body=rows[frame][direction];int left=body.Min(i=>i%w),right=body.Max(i=>i%w),bottom=body.Min(i=>i/w),top=body.Max(i=>i/w);
                if(top-bottom<100||left<1||right>=w-1)throw new Exception("Invalid body bounds: "+label);
                int tw=right-left+9,th=top-bottom+9;var tex=new Texture2D(tw,th,TextureFormat.RGBA32,false){name=label+" pixels",filterMode=FilterMode.Bilinear};var output=new Color[tw*th];foreach(int at in body)output[(at/w-bottom+4)*tw+at%w-left+4]=pixels[at];tex.SetPixels(output);tex.Apply();AssetDatabase.AddObjectToAsset(tex,bank);
                var sprite=Sprite.Create(tex,new Rect(0,0,tw,th),new Vector2(.5f,vehicle?.5f:4f/th),vehicle?Mathf.Max(tw,th)/3.25f:(top-bottom+1)/(name=="adryan"?1.3f:2.05f),0,SpriteMeshType.FullRect);sprite.name=label;AssetDatabase.AddObjectToAsset(sprite,bank);sprites[vehicle?frame*columns+direction:direction*rowCount+frame]=sprite;
            }
            bank.amaralWalk=sprites;EditorUtility.SetDirty(bank);AssetDatabase.SaveAssets();return sprites;
        }
        public static void Apply()
        {
            AssetDatabase.Refresh();
            var sets=new Dictionary<string,Sprite[]>();foreach(string name in new[]{"amaral","ribeiro","cristyan","henzo","martins","kevyn","paulo","higor","mateus","adryan","amaral-fists","amaral-pink","pivo"}){var sprites=Import(name);if(sprites!=null)sets[name]=sprites;}
            for(int i=0;i<8;i++){var poses=Import("citizen"+i);if(poses!=null)sets["citizen"+i]=poses;}for(int i=0;i<6;i++){var poses=Import("enemy"+i);if(poses!=null)sets["enemy"+i]=poses;}var vehicles=Import("vehicles",4,8,true);AssetDatabase.SaveAssets();int count=0;
            foreach(var item in EditorBuildSettings.scenes.Where(s=>s.enabled))
            {
                var scene=EditorSceneManager.OpenScene(item.path);
                void Animate(GameObject owner,SpriteRenderer visual,Sprite[] sprites)
                {
                    var actor=owner.GetComponent<ReferenceActorAnimation>();if(!actor)actor=owner.AddComponent<ReferenceActorAnimation>();actor.visual=visual;actor.idle=Enumerable.Range(0,8).Select(d=>sprites[d*3+1]).ToArray();actor.idleCycle=actor.idle.SelectMany(s=>Enumerable.Repeat(s,3)).ToArray();actor.walk=sprites;actor.run=sprites;actor.mirroredWalkDirections=new bool[8];if(visual)visual.sprite=actor.idle[0];if(!owner.GetComponent<DirectionalPose24>())owner.AddComponent<DirectionalPose24>();
                }
                foreach(var root in scene.GetRootGameObjects())foreach(var walker in root.GetComponentsInChildren<StreetWalker>(true))
                {var match=System.Text.RegularExpressions.Regex.Match(walker.frames[0].name,@"(?:Cidadão |^citizen)(\d+)");int model=match.Success?int.Parse(match.Groups[1].Value):0;if(sets.TryGetValue("citizen"+model,out var sprites))Animate(walker.gameObject,walker.visual,sprites);}
                foreach(var root in scene.GetRootGameObjects())foreach(var enemy in root.GetComponentsInChildren<EnemyVariety>(true))
                {if(sets.TryGetValue("enemy"+(int)enemy.archetype,out var sprites)){var agent=enemy.GetComponent<EnemyAgent>();Animate(enemy.gameObject,agent.visual,sprites);agent.poses=new[]{sprites[1],sprites[0],sprites[2],sprites[19]};}}
                foreach(var root in scene.GetRootGameObjects())foreach(var boss in root.GetComponentsInChildren<EnemyAgent>(true))
                {if(boss.role==EnemyRole.Pivo&&sets.TryGetValue("pivo",out var sprites))Animate(boss.gameObject,boss.visual,sprites);}
                foreach(var root in scene.GetRootGameObjects())foreach(var mateus in root.GetComponentsInChildren<Mateus>(true))
                {if(sets.TryGetValue("mateus",out var sprites)){mateus.idle=sprites[1];mateus.side=sprites[19];mateus.back=sprites[13];mateus.visual.sprite=mateus.idle;}}
                foreach(var root in scene.GetRootGameObjects())foreach(var equipment in root.GetComponentsInChildren<PlayerEquipment2D>(true))
                {equipment.blackHeldInBody=true;if(sets.TryGetValue("amaral-pink",out var pink)){equipment.pinkIdle=Enumerable.Range(0,8).Select(d=>pink[d*3+1]).ToArray();equipment.pinkWalk=pink;}if(sets.TryGetValue("amaral-fists",out var fists)){equipment.fistIdle=Enumerable.Range(0,8).Select(d=>fists[d*3+1]).ToArray();equipment.fistWalk=fists;}}
                if(vehicles!=null)foreach(var root in scene.GetRootGameObjects())foreach(var component in root.GetComponentsInChildren<Component>(true))
                {
                    if(!component)continue;var serialized=new SerializedObject(component);var property=serialized.GetIterator();bool changed=false;
                    while(property.Next(true))if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue is Sprite old)
                    {
                        var citizen=System.Text.RegularExpressions.Regex.Match(old.name,@"^(?:Cidadão|022 Cidadão) (\d+) / (\d+)");
                        if(citizen.Success&&sets.TryGetValue("citizen"+int.Parse(citizen.Groups[1].Value),out var poses)){int pose=int.Parse(citizen.Groups[2].Value);property.objectReferenceValue=poses[pose==3?13:pose==1?0:pose==2?2:1];changed=true;continue;}
                        var match=System.Text.RegularExpressions.Regex.Match(old.name,@"^(?:Carro limpo|022 Carro) (\d+).* / (\d+)");if(!match.Success)match=System.Text.RegularExpressions.Regex.Match(old.name,@"^vehicles / coerente026 / (\d+) / (\d+)");if(match.Success){int model=int.Parse(match.Groups[1].Value)%8,view=int.Parse(match.Groups[2].Value);property.objectReferenceValue=vehicles[model*4+view];changed=true;}
                    }
                    if(changed)serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                foreach(var root in scene.GetRootGameObjects())foreach(var actor in root.GetComponentsInChildren<ReferenceActorAnimation>(true))
                {
                    if(actor.idle==null||actor.idle.Length==0||!actor.idle[0])continue;string name=actor.idle[0].name.ToLowerInvariant().Split('/')[0].Trim();
                    if(name.StartsWith("adryan"))name="adryan";if(!sets.TryGetValue(name,out var sprites))continue;actor.idle=Enumerable.Range(0,8).Select(d=>sprites[d*3+1]).ToArray();actor.idleCycle=actor.idle.SelectMany(s=>Enumerable.Repeat(s,3)).ToArray();actor.walk=sprites;actor.run=sprites;actor.mirroredWalkDirections=new bool[8];if(actor.visual)actor.visual.sprite=actor.idle[0];count++;
                }
                foreach(var root in scene.GetRootGameObjects())foreach(var higor in root.GetComponentsInChildren<HigorMartialArts>(true))
                {if(!sets.TryGetValue("higor",out var sprites))continue;Animate(higor.gameObject,higor.visual,sprites);higor.frames=new[]{sprites[1],sprites[0],sprites[1],sprites[2],sprites[18],sprites[19],sprites[20]};}
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            Debug.Log("COHERENT026_APPLIED / "+sets.Count+" atlases / "+count+" actors");
        }
    }
}
