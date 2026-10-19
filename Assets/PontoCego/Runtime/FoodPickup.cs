using UnityEngine;
namespace PontoCego
{
    public enum FoodKind{Banana,OrangeJuice,Steak,Apple,Snack}
    public sealed class FoodPickup:MonoBehaviour,IInteractable
    {
        public FoodKind kind;public string uniqueId;bool used;
        public string Prompt=>"E • "+(kind==FoodKind.Banana?"Banana / vida completa":kind==FoodKind.OrangeJuice?"Suco / fôlego infinito por 10 s":kind==FoodKind.Steak?"Picanha / invencível por 5 s":"Lanche / cura temporária");
        public void Interact(){if(used)return;var player=FindAnyObjectByType<PlayerMotor>();if(!player)return;var hp=player.GetComponent<Health>();if(kind==FoodKind.Banana&&hp.Current>=hp.maximum)return;used=true;switch(kind){case FoodKind.Banana:hp.Heal(hp.maximum);break;case FoodKind.OrangeJuice:player.Model.GrantInfiniteStamina(10);break;case FoodKind.Steak:hp.GrantInvincibility(5);break;default:hp.GrantTemporaryHealth(kind==FoodKind.Apple?20:12,20);break;}FindAnyObjectByType<OfficeAudio>()?.Impact();Destroy(gameObject);}
        void OnGUI(){var player=FindAnyObjectByType<PlayerMotor>();if(!player||player.GetComponent<PlayerInputReader>().GameplayLocked||Vector2.Distance(player.transform.position,transform.position)>1.8f)return;CollectibleCover.Hint(transform.position,Prompt);}
    }
}
