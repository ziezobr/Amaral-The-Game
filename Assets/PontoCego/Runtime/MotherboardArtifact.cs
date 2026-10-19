using UnityEngine;
namespace PontoCego
{
 public sealed class MotherboardArtifact:MonoBehaviour
 {
  public Sprite icon;PlayerInputReader input;PlayerAim aim;
  void Start(){input=GetComponent<PlayerInputReader>();aim=GetComponent<PlayerAim>();}
  void Update(){if(!input||input.Paused||input.GameplayLocked||!GameSession.Current.pcAssembled||GameSession.Current.motherboardBombs<=0)return;if(input.Actions["Artifact"].WasPressedThisFrame())Throw();}
  public void Throw(){if(!GameSession.Current.pcAssembled||GameSession.Current.motherboardBombs<=0)return;GameSession.Current.motherboardBombs--;GameSession.Save();var go=new GameObject("Placa-mae Positivo / artefato");go.transform.position=transform.position;var bomb=go.AddComponent<MotherboardBomb>();bomb.target=(Vector2)transform.position+aim.Direction*4;var sr=go.AddComponent<SpriteRenderer>();sr.sprite=icon;sr.sortingOrder=30000;if(icon)go.transform.localScale=Vector3.one*(.7f/icon.bounds.size.y);}
 }
}
