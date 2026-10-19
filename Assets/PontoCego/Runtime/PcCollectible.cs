using UnityEngine;
namespace PontoCego
{
    public sealed class PcCollectible:MonoBehaviour,IInteractable
    {
        public string id;public int kind;public static readonly string[] Names={"Memória RAM","Processador","Placa-mãe Positivo","Fonte","SSD","Placa de vídeo"};
        public string Prompt=>"E • Recolher "+Names[kind];
        void Start(){if(GameSession.Current.collected.Contains(id))gameObject.SetActive(false);}
        void OnGUI(){var p=FindAnyObjectByType<PlayerMotor>();if(p&&!p.GetComponent<PlayerInputReader>().GameplayLocked&&Vector2.Distance(p.transform.position,transform.position)<2)CollectibleCover.Hint(transform.position,Prompt);}
        public void Interact(){if(GameSession.Current.collected.Contains(id))return;GameSession.Current.collected.Add(id);GameSession.Current.collected.Add("pc:kind:"+kind);GameSession.Save();FindAnyObjectByType<Interaction>()?.Say(Names[kind]+" guardada. Leve as peças ao Henzo.");gameObject.SetActive(false);}
        public static int TypesCollected{get{int count=0;for(int i=0;i<6;i++)if(GameSession.Current.collected.Contains("pc:kind:"+i))count++;return count;}}
    }
}
