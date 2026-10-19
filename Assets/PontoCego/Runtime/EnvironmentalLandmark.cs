using UnityEngine;
namespace PontoCego
{
    /// <summary>Optional environmental evidence rewards exploration without gating campaign missions.</summary>
    public sealed class EnvironmentalLandmark:MonoBehaviour,IInteractable
    {
        public string id,title,description;
        public string Prompt=>"E • Examinar "+title;
        public bool Discovered=>GameSession.Current.collected.Contains("landmark:"+id);
        public void Interact()
        {
            bool first=GameSession.RewardOnce("landmark:"+id,25);
            if(first)GameSession.AddEvidence(title+": "+description);
            FindAnyObjectByType<Interaction>()?.Say(title+" — "+description+(first?" • +R$25 por documentar a descoberta.":""));
        }
        void OnGUI()
        {
            var player=FindAnyObjectByType<PlayerMotor>();if(!player)return;var input=player.GetComponent<PlayerInputReader>();
            if(!input.Paused&&!input.GameplayLocked&&Vector2.Distance(player.transform.position,transform.position)<2)CollectibleCover.Hint(transform.position,Prompt);
        }
    }
}
