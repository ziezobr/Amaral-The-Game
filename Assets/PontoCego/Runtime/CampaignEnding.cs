using UnityEngine;
namespace PontoCego
{
    public sealed class CampaignEnding:MonoBehaviour
    {
        public bool Active{get;private set;}
        public string closingText="Adryan está a salvo. As fotos de Amaral expuseram o esquema da Riberal. O grupo finalmente encontra um café e um pão de queijo em paz.";
        public void Begin(){Active=true;FindAnyObjectByType<PlayerInputReader>().SetGameplayLocked(true);GameSession.Save();}
    }
}
