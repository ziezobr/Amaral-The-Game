using UnityEngine;
namespace PontoCego
{
    public sealed class MedkitPickup:MonoBehaviour
    {
        public string pickupId;
        void Update()
        {
            var player=FindAnyObjectByType<CombatPlayer>();if(!player||player.Medkits>=8)return;
            if(Vector2.Distance(player.transform.position,transform.position)<.8f)
            {if(GameSession.RewardOnce(pickupId,0))player.AddMedkit();Destroy(gameObject);}
        }
    }
}
