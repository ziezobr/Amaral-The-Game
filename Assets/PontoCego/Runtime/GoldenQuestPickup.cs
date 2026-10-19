using UnityEngine;
namespace PontoCego
{
    public sealed class GoldenQuestPickup:MonoBehaviour,IInteractable
    {
        public int chain,step;
        public string Prompt=>step==3?"E • "+(chain==0?"Glock dourada":"Navalha dourada"):"E • Recolher encomenda de Mateus";
        void OnGUI(){var p=FindAnyObjectByType<PlayerMotor>();if(p&&Vector2.Distance(p.transform.position,transform.position)<2)CollectibleCover.Hint(transform.position,Prompt);}
        public void Interact()
        {
            if(!GoldenWeaponQuest.Started(chain))return;
            if(step==3){if(GoldenWeaponQuest.Step(chain)<3||!GameSession.Current.collected.Contains("gold:unlock:"+chain))return;string id=chain==0?"gold:glock":"gold:razor";if(GameSession.Current.collected.Contains(id))return;GameSession.Current.collected.Add(id);FindAnyObjectByType<Interaction>()?.Say(chain==0?"Ribeiro: Glock dourada! Munição infinita.":"Cristyan: Navalha dourada! Dano dobrado.");}
            else{if(GoldenWeaponQuest.Step(chain)!=step)return;GameSession.Current.collected.Add("gold:"+chain+":"+step);FindAnyObjectByType<Interaction>()?.Say(step==2?"Três serviços concluídos. Volte ao Mateus.":GoldenWeaponQuest.Jobs[chain][step+1]);}
            GameSession.Save();Destroy(gameObject);
        }
    }
}
