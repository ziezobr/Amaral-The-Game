using UnityEngine;
namespace PontoCego3D
{
 public sealed class StreetSignFacing:MonoBehaviour
 {
  MeshRenderer display;void Awake(){display=GetComponent<MeshRenderer>();}
  void LateUpdate(){if(Camera.main&&display)display.enabled=Vector3.Dot(transform.forward,Camera.main.transform.position-transform.position)<0;}
 }
}
