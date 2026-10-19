using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    public sealed class HubPhotoJob:MonoBehaviour
    {
        void Update()
        {
            var s=GameSession.Current;if(!s.photoJobActive||s.collected.Contains("bairro:foto"))return;
            var p=FindAnyObjectByType<PlayerAim>();if(!p||p.FlashIntensity<=0||Vector2.Distance(p.transform.position,transform.position)>4.5f)return;
            Vector2 d=transform.position-p.transform.position;if(Vector2.Dot(p.Direction,d.normalized)<.8f)return;
            var hit=Physics2D.Raycast(p.transform.position,d.normalized,d.magnitude,LayerMask.GetMask("World"));if(hit.collider&&!hit.collider.transform.IsChildOf(transform))return;
            GameSession.RewardOnce("bairro:foto",100);s.photoJobActive=false;GameSession.Save();FindAnyObjectByType<Interaction>().Say("Foto da praça entregue: R$ 100. Trabalho concluído!");
        }
    }
}
