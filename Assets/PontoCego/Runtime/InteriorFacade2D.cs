using UnityEngine;
namespace PontoCego
{
    /// <summary>One physical building, revealed on approach. No scene load, position reset or collider toggle.</summary>
    public sealed class InteriorFacade2D:MonoBehaviour
    {
        public Rect footprint;public SpriteRenderer[] interior=System.Array.Empty<SpriteRenderer>(),exterior=System.Array.Empty<SpriteRenderer>();
        Color[] insideColors,outsideColors;PlayerMotor player;float open;
        void Awake(){insideColors=Colors(interior);outsideColors=Colors(exterior);}
        static Color[] Colors(SpriteRenderer[] list){var colors=new Color[list.Length];for(int i=0;i<list.Length;i++)colors[i]=list[i]?list[i].color:Color.white;return colors;}
        void Start(){player=FindAnyObjectByType<PlayerMotor>();UpdateVisuals();}
        void LateUpdate(){UpdateVisuals();}
        void UpdateVisuals()
        {
            if(!player)return;Vector2 point=player.transform.position;var approach=footprint;approach.xMin-=2.5f;approach.yMin-=2.5f;approach.xMax+=2.5f;approach.yMax+=2.5f;
            open=Mathf.MoveTowards(open,approach.Contains(point)?1:0,Time.deltaTime*3);
            Paint(interior,insideColors,open);Paint(exterior,outsideColors,1-open);
        }
        static void Paint(SpriteRenderer[] list,Color[] colors,float alpha){for(int i=0;i<list.Length;i++)if(list[i]){var color=colors[i];color.a*=alpha;list[i].color=color;}}
        void OnDisable(){if(insideColors!=null)Paint(interior,insideColors,1);if(outsideColors!=null)Paint(exterior,outsideColors,1);}
    }
}
