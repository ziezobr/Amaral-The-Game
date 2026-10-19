using System.Collections.Generic;using UnityEngine;using UnityEngine.SceneManagement;
namespace PontoCego3D
{
    /// <summary>Art treatment isolated to the 3D district. Original materials and the 2D campaign remain intact.</summary>
    public static class ToonStyling
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Register(){SceneManager.sceneLoaded+=Apply;if(SceneManager.GetActiveScene().name=="CentroAberto")Apply(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
        static void Apply(Scene scene,LoadSceneMode mode)
        {
            if(scene.name!="CentroAberto")return;var shader=Resources.Load<Shader>("PontoCegoToon");if(!shader)return;
            var cache=new Dictionary<Material,Material>();
            foreach(var renderer in Resources.FindObjectsOfTypeAll<Renderer>())
            {
                if(!renderer.gameObject.scene.IsValid())continue;
                var materials=renderer.sharedMaterials;bool changed=false;
                for(int i=0;i<materials.Length;i++)
                {
                    var source=materials[i];if(!source||source.shader.name!="Standard")continue;
                    if(!cache.TryGetValue(source,out var material)){material=new Material(shader){name=source.name+" / desenho",color=source.color,mainTexture=source.mainTexture};material.SetFloat("_Grain",renderer is SkinnedMeshRenderer?.015f:.018f);material.mainTextureScale=source.mainTextureScale;material.mainTextureOffset=source.mainTextureOffset;cache.Add(source,material);}materials[i]=material;changed=true;
                }
                if(changed)renderer.sharedMaterials=materials;
            }
            foreach(var actor in Resources.FindObjectsOfTypeAll<CombatActor>())if(actor.gameObject.scene.IsValid()&&!actor.GetComponent<ToonProportions>())actor.gameObject.AddComponent<ToonProportions>();
        }
    }
    public sealed class ToonProportions:MonoBehaviour
    {
        Transform head;Vector3 original;
        void Start(){var animator=GetComponent<CombatActor>().animator;head=animator.GetBoneTransform(HumanBodyBones.Head);if(head)original=head.localScale;GetComponent<CombatActor>().visual.localScale=Vector3.one*1.12f;}
        void LateUpdate(){if(head)head.localScale=original*1.65f;}
    }
}
