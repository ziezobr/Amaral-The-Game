using UnityEngine;
namespace PontoCego
{
    public sealed class StreetDoorCutaway2D:MonoBehaviour
    {
        public SpriteRenderer facade;public Rect room;PlayerMotor player;float alpha=1;
        void Start(){player=FindAnyObjectByType<PlayerMotor>();}
        void LateUpdate(){if(!player||!facade)return;alpha=Mathf.MoveTowards(alpha,room.Contains(player.transform.position)?.22f:1,Time.deltaTime*5);var c=facade.color;c.a=alpha;facade.color=c;}
    }
}
