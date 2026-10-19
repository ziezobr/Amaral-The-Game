using UnityEngine;
namespace PontoCego
{
    /// <summary>Non-spatial chase score with pause, looping and entry/exit fades.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class ChaseSoundtrack:MonoBehaviour
    {
        public AudioClip clip;public RoadDriver owner;public bool Paused{get;private set;}public AudioSource Source{get;private set;}PlayerInputReader input;
        void Start(){Source=GetComponent<AudioSource>();Source.playOnAwake=false;Source.spatialBlend=0;Source.loop=true;Source.volume=0;Source.clip=clip;input=FindAnyObjectByType<PlayerInputReader>();if(clip)Source.Play();}
        void Update()
        {
            if(!Source)return;bool stop=!owner||!owner.enabled;
            if(input&&input.Paused&&!stop){if(!Paused){Source.Pause();Paused=true;}return;}
            if(Paused){Source.UnPause();Paused=false;}Source.volume=Mathf.MoveTowards(Source.volume,stop?0:.45f*GamePreferences.Music,Time.unscaledDeltaTime*.6f);if(stop&&Source.volume<=.001f)Destroy(gameObject);
        }
    }
}
