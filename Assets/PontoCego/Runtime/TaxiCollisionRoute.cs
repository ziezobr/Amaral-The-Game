using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    /// <summary>A* clearance uses the whole vehicle turning radius. Dynamic cars are handled by braking.</summary>
    public static class TaxiCollisionRoute
    {
        static WorldHub cachedWorld;static byte[] cachedFootClear;
        static readonly Vector2Int[] Directions={Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
        static readonly Collider2D[] Hits=new Collider2D[64];
        static readonly Dictionary<int,float[]> RoadWeights=new Dictionary<int,float[]>();
        public static Vector2[] Plan(Vector2 from,Vector2 to,Rigidbody2D own,float clearance=1.98f,float step=.4f)
        {
            var world=WorldHub.Instance;if(!world)return null;var rect=world.bounds;int width=Mathf.CeilToInt(rect.width/step)+1,height=Mathf.CeilToInt(rect.height/step)+1,n=width*height;int mask=LayerMask.GetMask("World");if(cachedWorld!=world){cachedFootClear=null;RoadWeights.Clear();cachedWorld=world;}if(!RoadWeights.TryGetValue(n,out var roadWeights)){roadWeights=new float[n];RoadWeights[n]=roadWeights;}var filter=new ContactFilter2D{useLayerMask=true,layerMask=mask,useTriggers=false};var clear=clearance<.5f&&cachedWorld==world&&cachedFootClear!=null&&cachedFootClear.Length==n?cachedFootClear:new byte[n];if(clearance<.5f){cachedWorld=world;cachedFootClear=clear;}
            Vector2 Point(int i)=>rect.min+new Vector2(i%width,i/width)*step;
            int Id(Vector2 p)=>Mathf.Clamp(Mathf.RoundToInt((p.y-rect.yMin)/step),0,height-1)*width+Mathf.Clamp(Mathf.RoundToInt((p.x-rect.xMin)/step),0,width-1);
            bool Clear(int i){if(clear[i]!=0)return clear[i]==1;int count=Physics2D.OverlapCircle(Point(i),clearance,filter,Hits);for(int k=0;k<count;k++){var c=Hits[k];if(c&&c.attachedRigidbody!=own&&(!c.attachedRigidbody||c.attachedRigidbody.bodyType==RigidbodyType2D.Static)){clear[i]=2;return false;}}clear[i]=1;return true;}
            int Near(Vector2 p){int at=Id(p),best=-1;float score=float.MaxValue;for(int y=-12;y<=12;y++)for(int x=-12;x<=12;x++){int nx=at%width+x,ny=at/width+y;if(nx<0||ny<0||nx>=width||ny>=height)continue;int i=ny*width+nx;float d=(Point(i)-p).sqrMagnitude;if(d<score&&Clear(i)){score=d;best=i;}}return best;}
            int first=Near(from),last=Near(to);if(first<0||last<0)return null;var costs=new float[n];var parent=new int[n];for(int i=0;i<n;i++){costs[i]=float.MaxValue;parent[i]=-1;}var closed=new bool[n];var open=new SortedSet<(float score,int sequence,int id)>();int sequence=0;costs[first]=0;open.Add((0,sequence++,first));
            float Heuristic(int i)=>Mathf.Abs(Point(i).x-Point(last).x)+Mathf.Abs(Point(i).y-Point(last).y);
            float RoadCost(int id){if(roadWeights[id]>0)return roadWeights[id];Vector2 p=Point(id);float distance=400;
                void Segment(Vector2 a,Vector2 b){var delta=b-a;float length=delta.sqrMagnitude;if(length<.0001f)return;var q=a+delta*Mathf.Clamp01(Vector2.Dot(p-a,delta)/length);distance=Mathf.Min(distance,(p-q).sqrMagnitude);}
                for(int i=0;i<world.roadStarts.Length;i++)Segment(world.roadStarts[i],world.roadEnds[i]);
                Segment(new Vector2(-10.88f,-20),new Vector2(-10.88f,20));Segment(new Vector2(10.88f,-20),new Vector2(10.88f,20));Segment(new Vector2(-30,9.38f),new Vector2(30,9.38f));Segment(new Vector2(-30,-5.55f),new Vector2(30,-5.55f));
                return roadWeights[id]=1+Mathf.Clamp(Mathf.Sqrt(distance)-2,0,5)*.4f;}
            while(open.Count>0){var head=open.Min;open.Remove(head);int at=head.id;if(closed[at])continue;if(at==last)break;closed[at]=true;int x=at%width,y=at/width;foreach(var d in Directions){int nx=x+d.x,ny=y+d.y;if(nx<0||ny<0||nx>=width||ny>=height)continue;int other=ny*width+nx;if(closed[other]||!Clear(other))continue;float next=costs[at]+step*RoadCost(other);if(next>=costs[other])continue;costs[other]=next;parent[other]=at;open.Add((next+Heuristic(other),sequence++,other));}}
            if(first!=last&&parent[last]<0)return null;var path=new List<Vector2>();for(int i=last;i>=0;i=parent[i])path.Add(Point(i));path.Reverse();if(path.Count<3)return path.ToArray();var compact=new List<Vector2>{path[0]};for(int i=1;i<path.Count-1;i++){var before=path[i]-path[i-1];var after=path[i+1]-path[i];if(Mathf.Abs(before.x*after.y-before.y*after.x)>.0001f)compact.Add(path[i]);}compact.Add(path[path.Count-1]);return compact.ToArray();
        }
    }
}
