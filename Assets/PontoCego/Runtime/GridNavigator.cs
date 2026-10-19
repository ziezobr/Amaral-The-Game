using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Cached per-phase walk grid with eight-neighbour A*, no corner cutting and no world-level graph.</summary>
    public sealed class GridNavigator:MonoBehaviour
    {
        public float cellSize=.65f,agentRadius=.29f;
        public Vector2 areaCenter,areaSize;
        public void SetArea(Vector2 center,Vector2 size){areaCenter=center;areaSize=size;Rebuild();}
        bool[,] open;int width,height;Vector2 origin;
        void Start(){Rebuild();}
        public void Rebuild()
        {
            var level=FindAnyObjectByType<LevelProgress>();Vector2 size=areaSize.x>0?areaSize:level?level.mapSize:new Vector2(36,24);
            width=Mathf.CeilToInt(size.x/cellSize);height=Mathf.CeilToInt(size.y/cellSize);origin=areaCenter-size*.5f+Vector2.one*cellSize*.5f;
            open=new bool[width,height];int mask=LayerMask.GetMask("World");Physics2D.SyncTransforms();
            for(int x=0;x<width;x++)for(int y=0;y<height;y++)open[x,y]=!Physics2D.OverlapCircle(Point(x,y),agentRadius,mask);
        }
        Vector2 Point(int x,int y)=>origin+new Vector2(x*cellSize,y*cellSize);
        Vector2Int Cell(Vector2 p)=>new Vector2Int(Mathf.Clamp(Mathf.RoundToInt((p.x-origin.x)/cellSize),0,width-1),Mathf.Clamp(Mathf.RoundToInt((p.y-origin.y)/cellSize),0,height-1));
        public List<Vector2> Path(Vector2 from,Vector2 to)
        {
            if(open==null)Rebuild();var start=Cell(from);var end=Cell(to);var result=new List<Vector2>();
            if(!open[end.x,end.y])
            {
                float best=float.MaxValue;for(int x=-2;x<=2;x++)for(int y=-2;y<=2;y++)
                {int nx=end.x+x,ny=end.y+y;if(nx<0||ny<0||nx>=width||ny>=height||!open[nx,ny])continue;float d=(Point(nx,ny)-to).sqrMagnitude;if(d<best){best=d;end=new Vector2Int(nx,ny);}}
            }
            int count=width*height;var g=new float[count];var parent=new int[count];var closed=new bool[count];
            for(int i=0;i<count;i++){g[i]=float.MaxValue;parent[i]=-1;}int s=start.y*width+start.x,e=end.y*width+end.x;g[s]=0;
            var frontier=new List<int>{s};int guard=0;
            while(frontier.Count>0&&guard++<count)
            {
                int at=0;float best=float.MaxValue;for(int i=0;i<frontier.Count;i++)
                {int n=frontier[i];float f=g[n]+Vector2.Distance(new Vector2(n%width,n/width),end);if(f<best){best=f;at=i;}}
                int current=frontier[at];frontier.RemoveAt(at);if(current==e)break;if(closed[current])continue;closed[current]=true;
                int cx=current%width,cy=current/width;
                for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=1;dy++)
                {
                    if(dx==0&&dy==0)continue;int x=cx+dx,y=cy+dy;
                    if(x<0||y<0||x>=width||y>=height||!open[x,y])continue;
                    if(dx!=0&&dy!=0&&(!open[cx+dx,cy]||!open[cx,cy+dy]))continue;
                    int next=y*width+x;if(closed[next])continue;float cost=g[current]+(dx!=0&&dy!=0?1.4142135f:1f);
                    if(cost>=g[next])continue;g[next]=cost;parent[next]=current;frontier.Add(next);
                }
            }
            if(e!=s&&parent[e]<0)return result;
            for(int n=e;n!=s&&n>=0;n=parent[n])result.Add(Point(n%width,n/width));result.Reverse();return result;
        }
        public bool Clear(Vector2 from,Vector2 to,float radius=.22f)
        {
            Vector2 d=to-from;return !Physics2D.CircleCast(from,radius,d.normalized,d.magnitude,LayerMask.GetMask("World"));
        }
    }
}
