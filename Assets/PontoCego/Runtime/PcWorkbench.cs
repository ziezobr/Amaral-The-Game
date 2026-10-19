using UnityEngine;
namespace PontoCego
{
 public sealed class PcWorkbench:MonoBehaviour,IInteractable
 {
  public string Prompt=>GameSession.Current.pcAssembled?"PC montado / artefatos liberados":"E / Montar PC com Henzo ("+PcCollectible.TypesCollected+"/6)";
  public void Interact(){var chat=FindAnyObjectByType<Interaction>();if(GameSession.Current.pcAssembled){chat?.Say("Artefatos: G joga placa-mae; Cristyan usa RAM; Ribeiro ganhou cadencia.");return;}if(PcCollectible.TypesCollected<6){FindAnyObjectByType<StoryDialogue>()?.Begin("Henzo",null,new[]{"Faltam peças: "+PcCollectible.TypesCollected+"/6. Procure RAM, processador, placa-mãe, fonte, SSD e placa de vídeo nas missões."},null);return;}GameSession.Current.pcAssembled=true;GameSession.Current.motherboardBombs=5;GameSession.Save();foreach(var ally in GetComponentsInChildren<Transform>(true))if(ally!=transform)ally.gameObject.SetActive(true);FindAnyObjectByType<StoryDialogue>()?.Begin("Henzo",null,new[]{"Todos reunidos! PC montado. Placa-mãe explosiva [G], RAM para Cristyan e GPU no 38 do Ribeiro. Paulo liberado!"},null);}
 }
}
