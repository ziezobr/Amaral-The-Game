using UnityEngine;
namespace PontoCego
{
    public sealed class LevelProgress:MonoBehaviour
    {
        public string title,chapter="PRÓLOGO";
        [TextArea]public string[] objectives;
        public Transform[] objectiveTargets;
        public Texture2D mapTexture,amaralPortrait;
        public Vector2 mapCenter;
        public Vector2 mapSize=new Vector2(36,24);
        public bool showIntroduction;
        public string[] introductionLines;
        [SerializeField]int step;
        public int Step=>step;
        public string Objective=>objectives.Length==0?"Explore o cenário.":objectives[Mathf.Min(step,objectives.Length-1)];
        public Transform CurrentTarget=>objectiveTargets!=null&&step<objectiveTargets.Length?objectiveTargets[step]:null;
        public void Advance(){step=Mathf.Min(step+1,objectives.Length-1);if(GameSession.CanPersist)GameSession.Checkpoint(this);}
        public void RestoreStep(int value){step=Mathf.Clamp(value,0,objectives.Length-1);}
        void Start()
        {
            if(!WorldHub.Instance&&GameSession.CanPersist&&GameSession.Current.checkpointScene==UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)RestoreStep(GameSession.Current.checkpointStep);
            if(showIntroduction&&step==0)FindAnyObjectByType<StoryDialogue>().Begin("Amaral",amaralPortrait,introductionLines!=null&&introductionLines.Length>0?introductionLines:new[]{
                "Patos de Minas. Mais um dia na Riberal. Tenho 24 anos e parece que já passei uma vida correndo atrás de prazo.",
                "O Adryan depende de mim. O cachê do evento desta noite vai ajudar em casa. Primeiro, bater o ponto e aguentar o Paulo."});
        }
    }
}
