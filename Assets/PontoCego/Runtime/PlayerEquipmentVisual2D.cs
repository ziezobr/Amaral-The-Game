using UnityEngine;
namespace PontoCego
{
    [DefaultExecutionOrder(90)]public sealed class PlayerEquipmentVisual2D:MonoBehaviour
    {PlayerEquipment2D equipment;void Awake(){equipment=GetComponent<PlayerEquipment2D>();}void LateUpdate(){if(equipment)equipment.RefreshVisual();}}
}
