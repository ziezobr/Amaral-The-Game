using UnityEngine;
namespace PontoCego
{
    /// <summary>Position is authoritative. Menus can mark a route but cannot remotely activate a mission.</summary>
    public sealed class WorldMissionTrigger:MonoBehaviour,IInteractable
    {
        void OnGUI(){if(!GamePreferences.Objectives)return;var player=Object.FindAnyObjectByType<PlayerMotor>();var mission=WorldHub.Instance?WorldHub.Instance.Find(missionId):null;if(player&&mission!=null&&Vector2.Distance(player.transform.position,transform.position)<10)CollectibleCover.Hint(transform.position,"ENTRADA • "+mission.level.title);}

        public string missionId;public float radius=1.1f;public string Prompt=>"Iniciar missão • "+missionId;
        void OnTriggerStay2D(Collider2D other)
        {
            if(other.GetComponent<PlayerMotor>()||(DriveableCar.Current&&other.GetComponent<DriveableCar>()))WorldHub.Instance?.RequestBegin(missionId);
        }
        public void Interact(){WorldHub.Instance?.TryBegin(missionId);}
    }
}
