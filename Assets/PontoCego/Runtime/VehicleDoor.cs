using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    public sealed class VehicleDoor:MonoBehaviour,IInteractable
    {
        public DriveableCar car;public string Prompt=>"Entrar no veículo • E";public void Interact(){car.Enter();}
    }
}
