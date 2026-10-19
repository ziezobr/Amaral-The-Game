using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    public sealed class StoryInteractable:MonoBehaviour,IInteractable
    {
        public string displayName,dialogue,speaker;
        [TextArea]public string[] lines;
        public Texture2D portrait;
        public int requiredStep=-1;
        public bool advancesObjective,endsOpening,completesStage,keepEvidence;
        public string nextScene;
        public string Prompt=>displayName;
        public void Interact()
        {
            var player=FindAnyObjectByType<Interaction>();var level=FindAnyObjectByType<LevelProgress>();
            var talk=player.GetComponent<StoryDialogue>();if(talk&&talk.Active)return;
            if(requiredStep>=0&&level.Step!=requiredStep)
            {player.Say(level.Step>requiredStep?"Já conversamos. "+level.Objective:"Antes: "+level.Objective);return;}
            void Complete()
            {
                if(keepEvidence)GameSession.AddEvidence(level.title+" / "+displayName+"\n"+string.Join("\n",lines));
                if(advancesObjective)level.Advance();
                if(endsOpening){GameSession.Travel("03_Casa");return;}
                if(completesStage){FindAnyObjectByType<CombatMission>().CompleteStage();return;}
                if(!string.IsNullOrEmpty(nextScene)){var mission=FindAnyObjectByType<CombatMission>();if(mission&&mission.nextScene==nextScene)mission.CompleteStage();else GameSession.Travel(nextScene);}
            }
            if(talk)talk.Begin(string.IsNullOrEmpty(speaker)?displayName:speaker,portrait,lines!=null&&lines.Length>0?lines:new[]{dialogue},Complete);
            else{player.Say(dialogue);Complete();}
        }
    }
}
