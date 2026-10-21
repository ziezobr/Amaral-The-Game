using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PontoCego.Editor
{
    public static class Animation026Builder
    {
        public static void FinalizeDemo(){Apply();Coherent026Builder.Apply();SpriteAudit022.Export();}
        public static void Apply()
        {
            int cars=0,actors=0;
            foreach(var item in EditorBuildSettings.scenes.Where(s=>s.enabled))
            {
                var scene=EditorSceneManager.OpenScene(item.path);var world=UnityEngine.Object.FindAnyObjectByType<WorldHub>();
                foreach(var root in scene.GetRootGameObjects())
                {
                    foreach(var animation in root.GetComponentsInChildren<ReferenceActorAnimation>(true))
                    {if(!animation.GetComponent<DirectionalPose24>())animation.gameObject.AddComponent<DirectionalPose24>();actors++;}
                    foreach(var walker in root.GetComponentsInChildren<StreetWalker>(true))
                    {if(!walker.GetComponent<DirectionalPose24>())walker.gameObject.AddComponent<DirectionalPose24>();}
                }
                if(world)
                {
                    var lanes=TrafficLaneRoutes.Build(world.roadStarts,world.roadEnds);if(lanes.Count==0)throw new Exception("No closed road lanes found");
                    var traffic=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CityTraffic>(true)).ToArray();
                    var placed=new System.Collections.Generic.List<Vector2>();
                    for(int i=0;i<traffic.Length;i++)
                    {
                        Vector2[] route=null;
                        for(int attempt=0;attempt<lanes.Count*60;attempt++)
                        {
                            var lane=lanes[(i+attempt/60)%lanes.Count];float length=0;for(int j=0;j<lane.Length;j++)length+=Vector2.Distance(lane[j],lane[(j+1)%lane.Length]);
                            var candidate=TrafficLaneRoutes.StartAtDistance(lane,length*Mathf.Repeat(i*.618034f+attempt*.071f,1));
                            if(placed.All(p=>Vector2.Distance(p,candidate[0])>=9)){route=candidate;break;}
                        }
                        if(route==null)throw new Exception("No safe traffic spawn for car "+i);
                        traffic[i].route=route;placed.Add(route[0]);
                        traffic[i].transform.position=traffic[i].route[0];cars++;
                    }
                    Debug.Log("ANIMATION026_ROAD_LOOPS / "+lanes.Count);
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();Debug.Log("ANIMATION026_APPLIED / actors="+actors+" cars="+cars);
        }
    }
}
