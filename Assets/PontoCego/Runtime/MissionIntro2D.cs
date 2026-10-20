using UnityEngine;using Unity.Cinemachine;
namespace PontoCego
{
    /// <summary>In-world establishing shot. Keeps physics, scene and campaign position intact.</summary>
    public sealed class MissionIntro2D:MonoBehaviour
    {
        public static bool Showing=>Object.FindAnyObjectByType<MissionIntro2D>() is MissionIntro2D intro&&intro.Active;
        public bool Active{get;private set;}float elapsed,zoom;string title,objective;PlayerInputReader input;CinemachineCamera cameraRig;
        public void Begin(CityMission mission)
        {
            if(Active)Finish();input=Object.FindAnyObjectByType<PlayerInputReader>();cameraRig=Object.FindAnyObjectByType<CinemachineCamera>();if(!input)return;
            title=mission.level.title;objective=mission.level.Objective;elapsed=0;Active=true;input.SetGameplayLocked(true);if(cameraRig)zoom=cameraRig.Lens.OrthographicSize;
        }
        void Update()
        {
            if(!Active||input.Paused)return;elapsed+=Time.deltaTime;
            if(cameraRig)cameraRig.Lens.OrthographicSize=Mathf.Lerp(zoom*1.18f,zoom,Mathf.SmoothStep(0,1,elapsed/4));
            if(elapsed>=4.5f||elapsed>.35f&&input.Actions["Interact"].WasPressedThisFrame())Finish();
        }
        public void Finish()
        {
            if(!Active)return;Active=false;if(cameraRig)cameraRig.Lens.OrthographicSize=zoom;
            if(input)input.SetGameplayLocked(input.GetComponent<StoryDialogue>().Active||!input.GetComponent<Health>().Alive);
        }
        void OnGUI()
        {
            if(!Active)return;var matrix=GUI.matrix;var colour=GUI.color;int depth=GUI.depth;GUI.depth=-60;GUI.matrix=UiTheme.SafeMatrix();
            GUI.color=new Color(0,0,0,.94f);GUI.DrawTexture(new Rect(0,0,1280,116),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(0,550,1280,170),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(60,28,1160,65),title,UiTheme.Label(31,UiTheme.Accent,true));GUI.Label(new Rect(60,575,1050,85),objective,UiTheme.Label(20,Color.white));
            if(GUI.Button(new Rect(1070,666,150,32),"E • PULAR",GamePhoneView.ButtonStyle))Finish();GUI.matrix=matrix;GUI.color=colour;GUI.depth=depth;
        }
        void OnDisable(){Finish();}
    }
}
