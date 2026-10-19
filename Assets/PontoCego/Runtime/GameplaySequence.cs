using UnityEngine;
namespace PontoCego
{
    public enum SequenceKind{Kidnapping,MartinsRescue,RescueAdryan}
    /// <summary>Short in-engine staging. World actors move in the actual phase; no still panels or comic UI.</summary>
    public sealed class GameplaySequence:MonoBehaviour
    {
        public SequenceKind kind;public int triggerStep;
        public Transform triggerPoint,vehicle,actor,vehicleExit,actorGoal;
        public SpriteRenderer vehicleVisual;public float speed=6;
        public Sprite smokeSprite;
        bool begun,finished;float elapsed;PlayerInputReader input;LevelProgress level;
        void Start(){input=FindAnyObjectByType<PlayerInputReader>();level=FindAnyObjectByType<LevelProgress>();}
        void Update()
        {
            if(finished||!input||input.Paused)return;
            if(!begun&&level.Step>triggerStep)
            {
                finished=true;if(kind==SequenceKind.Kidnapping){if(actor)actor.gameObject.SetActive(false);if(vehicle)vehicle.gameObject.SetActive(false);}
                if(kind==SequenceKind.RescueAdryan&&actor){actor.position=input.transform.position+Vector3.right;actor.gameObject.layer=13;actor.gameObject.AddComponent<CivilianFollower>();}return;
            }
            if(!begun)
            {
                if(level.Step!=triggerStep||Vector2.Distance(input.transform.position,triggerPoint.position)>4)return;
                begun=true;input.SetGameplayLocked(true);elapsed=0;
                if(kind==SequenceKind.MartinsRescue&&smokeSprite){var cloud=new GameObject("Cortina de fumaça / resgate");cloud.transform.position=triggerPoint.position;var fx=cloud.AddComponent<SmokeCloud>();fx.puff=smokeSprite;fx.duration=7;}
                FindAnyObjectByType<Interaction>().Say(kind==SequenceKind.Kidnapping?"Amaral: ADRYAN! Cenoura... solta meu irmão!":kind==SequenceKind.MartinsRescue?"Martins: Abaixa! Eu cubro vocês. Sigam para a oficina!":"Amaral: Adryan! Eu vim te buscar. Vamos sair daqui!");
            }
            elapsed+=Time.deltaTime;
            if(kind==SequenceKind.Kidnapping)
            {
                if(elapsed<2&&actor&&actorGoal)actor.position=Vector3.MoveTowards(actor.position,actorGoal.position,speed*.45f*Time.deltaTime);
                if(elapsed>=2){if(actor)actor.gameObject.SetActive(false);if(vehicle&&vehicleExit)vehicle.position=Vector3.MoveTowards(vehicle.position,vehicleExit.position,speed*Time.deltaTime);}
            }
            else if(vehicle&&vehicleExit)vehicle.position=Vector3.MoveTowards(vehicle.position,vehicleExit.position,speed*Time.deltaTime);
            if(kind==SequenceKind.RescueAdryan&&actor)actor.position=Vector3.MoveTowards(actor.position,input.transform.position+Vector3.right,2*Time.deltaTime);
            if(elapsed>5.5f)
            {
                finished=true;input.SetGameplayLocked(false);level.Advance();
                if(kind==SequenceKind.RescueAdryan&&actor){actor.gameObject.layer=13;actor.gameObject.AddComponent<CivilianFollower>();}
            }
        }
    }
}
