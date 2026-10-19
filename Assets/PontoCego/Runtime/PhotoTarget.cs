using UnityEngine;
namespace PontoCego
{
    public sealed class PhotoTarget:MonoBehaviour
    {
        public int requiredStep;
        public bool revealsCrime;
        public string completedMessage;
        bool photographed;
        public bool Available=>!photographed&&FindAnyObjectByType<LevelProgress>().Step==requiredStep;
        public void Photograph()
        {
            var level=FindAnyObjectByType<LevelProgress>();if(photographed||level.Step!=requiredStep)return;
            photographed=true;level.Advance();var player=FindAnyObjectByType<Interaction>();
            if(!string.IsNullOrEmpty(completedMessage))GameSession.AddEvidence(level.title+" / foto registrada\n"+completedMessage);
            player.Say(!string.IsNullOrEmpty(completedMessage)?completedMessage:revealsCrime?"CLICK! Paulo: Quem está aí?! Peguem o fotógrafo!":"Amaral: Luz e foco conferidos. Vou testar a lente perto daquele corredor.");
        }
    }
}
