using UnityEngine;
namespace PontoCego
{
 public sealed class MotherboardBomb:MonoBehaviour
 {
  public Vector2 target;float clock;
  void Update(){var input=FindAnyObjectByType<PlayerInputReader>();if(input&&(input.Paused||input.GameplayLocked))return;clock+=Time.deltaTime;transform.position=Vector2.MoveTowards(transform.position,target,8*Time.deltaTime);transform.Rotate(0,0,240*Time.deltaTime);if(clock<.8f)return;foreach(var e in FindObjectsByType<EnemyAgent>()){Vector2 d=e.transform.position-transform.position;if(d.magnitude<2.5f&&!Physics2D.Raycast(transform.position,d.normalized,d.magnitude,LayerMask.GetMask("World"))){e.GetComponent<Health>().Damage(150,CombatTeam.Player);e.Stun(1);}}FindAnyObjectByType<OfficeAudio>()?.Impact();Destroy(gameObject);}
 }
}
