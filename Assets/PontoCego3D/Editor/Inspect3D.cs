using UnityEngine;using UnityEditor;using System.Linq;
namespace PontoCego3D.Editor
{
 public static class Inspect3D
 {
  public static void Run()
  {
   foreach(var name in new[]{"Male","Female","Animations"})
   {
    string path="Assets/PontoCego3D/Art/Characters/"+name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
    importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.isReadable=true;importer.importAnimation=true;importer.SaveAndReimport();
    var go=AssetDatabase.LoadAssetAtPath<GameObject>(path);var avatar=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
    Debug.Log("MODEL "+name+" Avatar "+(avatar?avatar.isValid:false));
    foreach(var mesh in go.GetComponentsInChildren<SkinnedMeshRenderer>())Debug.Log("MESH "+mesh.name+" "+mesh.sharedMesh.bounds+" bone "+string.Join(",",mesh.bones.Select(b=>b.name)));
    foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())Debug.Log("CLIP "+clip.name+" length "+clip.length);
   }
   foreach(var file in new[]{"building-a","building-b"})
   {
    var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PontoCego3D/Art/City/"+file+".fbx"));foreach(var r in go.GetComponentsInChildren<Renderer>())Debug.Log("CITY "+file+" "+r.bounds);Object.DestroyImmediate(go);
   }
  }
 }
}
