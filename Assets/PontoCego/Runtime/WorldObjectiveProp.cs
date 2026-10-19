using UnityEngine;
namespace PontoCego
{
    /// <summary>Evidence and its support share one ground anchor; interaction coordinates never move.</summary>
    public sealed class WorldObjectiveProp:MonoBehaviour
    {
        public SpriteRenderer visual;public int requiredStep=-1;
        public SpriteRenderer[] supports;public int[] supportOrders;public int visualOrderOffset=4;
        GameHud hud;PlayerMotor player;
        void Start(){hud=FindAnyObjectByType<GameHud>();player=FindAnyObjectByType<PlayerMotor>();}
        void OnGUI()
        {
            if(WorldHub.Instance)return;
            if(!hud||!player)return;
            var level=hud.level;
            if(!level||player.GetComponent<PlayerInputReader>().GameplayLocked||level.CurrentTarget!=transform||Vector2.Distance(player.transform.position,transform.position)>9)return;
            string text=GetComponent<PhotoTarget>()?"FOTOGRAFAR • "+level.Objective:"E • "+level.Objective;CollectibleCover.Hint(transform.position,text);
        }
        void LateUpdate()
        {
            if(!visual)return;
            int floor=WorldSort.At(transform.position.y);visual.sortingOrder=floor+visualOrderOffset;
            if(supports!=null)for(int i=0;i<supports.Length;i++)if(supports[i])supports[i].sortingOrder=floor+(supportOrders!=null&&i<supportOrders.Length?supportOrders[i]:0);
            if(!hud)hud=FindAnyObjectByType<GameHud>();
            var level=hud?hud.level:null;bool selected=level&&level.Step==requiredStep;
            visual.color=selected?new Color(1,1,.84f):Color.white;
        }
    }
}
