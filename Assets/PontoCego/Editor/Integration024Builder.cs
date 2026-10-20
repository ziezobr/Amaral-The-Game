using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PontoCego.Editor
{
    /// <summary>Incremental exterior approach pass. Preserves the authored buildings, sprites and campaign.</summary>
    public static class Integration024Builder
    {
        public static void Apply()
        {
            var scene=EditorSceneManager.OpenScene("Assets/PontoCego/Urban/Scenes/26_PatosWorld.unity");var world=Object.FindAnyObjectByType<WorldHub>();
            var old=GameObject.Find("Acessos contínuos / 024");if(old)Object.DestroyImmediate(old);
            var pavement=Object.FindObjectsByType<SpriteRenderer>().FirstOrDefault(s=>s.sprite&&s.name.StartsWith("Calçada / rede canônica"));
            if(!pavement)throw new System.InvalidOperationException("Missing city pavement reference");
            var root=new GameObject("Acessos contínuos / 024");int count=0;Physics2D.SyncTransforms();
            foreach(var entry in world.missions.Where(m=>!m.road).Select(m=>m.entry).Distinct())
            {
                Vector2 nearest=entry;float best=float.MaxValue;
                for(int i=0;i<world.roadStarts.Length;i++)
                {var a=world.roadStarts[i];var d=world.roadEnds[i]-a;var q=a+d*Mathf.Clamp01(Vector2.Dot(entry-a,d)/Mathf.Max(.001f,d.sqrMagnitude));float distance=(q-entry).sqrMagnitude;if(distance<best){best=distance;nearest=q;}}
                var direction=(nearest-entry).sqrMagnitude>.1f?(nearest-entry).normalized:Vector2.down;float length=Mathf.Max(2.5f,Mathf.Sqrt(best)-3.8f);
                if(length>18)continue;
                for(float distance=.35f;distance<length;distance+=.5f)
                {
                    var at=entry+direction*distance;
                    if(Physics2D.OverlapBox(at,new Vector2(1.6f,.5f),Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90,LayerMask.GetMask("World")))continue;
                    var tile=new GameObject("Acesso pavimentado / "+entry);tile.transform.SetParent(root.transform);tile.transform.position=at;tile.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
                    var sr=tile.AddComponent<SpriteRenderer>();sr.sprite=pavement.sprite;sr.color=new Color(.85f,.85f,.82f);sr.sortingOrder=-8998;
                    tile.transform.localScale=new Vector3(1.6f/sr.sprite.bounds.size.x,.54f/sr.sprite.bounds.size.y,1);count++;
                }
            }
            if(count==0)throw new System.InvalidOperationException("No approach geometry authored");EditorSceneManager.SaveScene(scene);Debug.Log("INTEGRATION024_APPROACH_TILES / "+count);
        }
    }
}
