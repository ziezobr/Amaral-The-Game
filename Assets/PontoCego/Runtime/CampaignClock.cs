using UnityEngine;
namespace PontoCego
{
    public sealed class CampaignClock:MonoBehaviour
    {
        PlayerInputReader input;
        void Start(){input=FindAnyObjectByType<PlayerInputReader>();}
        void Update(){if(input&&!input.Paused&&!input.GameplayLocked)GameSession.Current.playedSeconds+=Time.deltaTime;}
        void OnApplicationQuit(){GameSession.Save();}
    }
}
