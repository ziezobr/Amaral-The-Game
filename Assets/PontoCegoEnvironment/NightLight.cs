using UnityEngine;
namespace PontoCego.WorldEnvironment
{
    /// <summary>Nearby lamps and emissive windows switch gradually at twilight.</summary>
    public sealed class NightLight:MonoBehaviour
    {
        public Light lamp;public Renderer glow;public float intensity=2;public Color color=new Color(1,.64f,.24f);MaterialPropertyBlock block;
        void Awake(){block=new MaterialPropertyBlock();}
        void Update(){var clock=DayNightCycle.Instance;if(!clock)return;float night=clock.Night;if(lamp){bool near=!Camera.main||(Camera.main.transform.position-transform.position).sqrMagnitude<2500;lamp.enabled=night>.03f&&near;lamp.intensity=night*intensity;lamp.color=color;}if(glow){glow.GetPropertyBlock(block);block.SetColor("_EmissionColor",color*night*1.8f);glow.SetPropertyBlock(block);}}
    }
}
