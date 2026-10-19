using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    public sealed class HubGate:MonoBehaviour,IInteractable
    {
        public HubDestination destination;public string label;
        public string Prompt=>label;
        public void Interact()
        {
            var s=GameSession.Current;var say=FindAnyObjectByType<Interaction>();
            if(destination==HubDestination.MainMission){GameSession.EnterMission(s.mainMission);return;}
            if(destination==HubDestination.Mocambo){GameSession.EnterMission("24_Mocambo");return;}
            if(destination==HubDestination.CourierStart){if(s.collected.Contains("bairro:entrega")){say.Say("O serviço de entrega já foi concluído.");return;}s.deliveryActive=true;GameSession.Save();say.Say("Entrega: leve a encomenda da oficina à padaria. Recompensa: R$ 120.");return;}
            if(destination==HubDestination.CourierFinish)
            {
                if(!s.deliveryActive){say.Say("A padaria aguarda uma encomenda. Fale com o funcionário da oficina.");return;}
                if(GameSession.RewardOnce("bairro:entrega",120)){s.deliveryActive=false;say.Say("Entrega concluída! R$ 120. O Henzo vende munição e melhorias na oficina.");}
                else say.Say("A encomenda já foi entregue.");
            }
            if(destination==HubDestination.StreetPhoto)
            {
                var aim=FindAnyObjectByType<PlayerAim>();
                if(aim&&aim.FlashIntensity>0){GameSession.RewardOnce("bairro:foto",100);say.Say("Ensaio urbano concluído: R$ 100 pela foto da praça.");}
                else {say.Say("Ensaio urbano: mire na praça e fotografe com o botão esquerdo. Depois pressione E para entregar a foto.");s.photoJobActive=true;}
            }
            if(destination==HubDestination.LostCamera){GameSession.RewardOnce("bairro:camera",150);say.Say("Você recuperou a câmera perdida de um morador. Recompensa: R$ 150.");}
            GameSession.Save();
        }
    }
}
