using System.Collections.Generic;using UnityEngine;
namespace PontoCego
{
    /// <summary>Street-only graph: overlapping road segments are split at intersections before Dijkstra.</summary>
    public static class TaxiRoadGraph
    {
        public static Vector2[] Route(Vector2 from,Vector2 to)
        {
            var starts=new List<Vector2>();var ends=new List<Vector2>();var world=WorldHub.Instance;
            if(world&&world.roadStarts!=null){starts.AddRange(world.roadStarts);ends.AddRange(world.roadEnds);}
            foreach(float x in new[]{-10.88f,10.88f}){starts.Add(new Vector2(x,-20));ends.Add(new Vector2(x,20));}
            foreach(float y in new[]{9.38f,-5.55f}){starts.Add(new Vector2(-30,y));ends.Add(new Vector2(30,y));}
            var nodes=new List<Vector2>();int Node(Vector2 p){for(int i=0;i<nodes.Count;i++)if(Vector2.Distance(nodes[i],p)<.08f)return i;nodes.Add(p);return nodes.Count-1;}
            Vector2 Project(Vector2 p,int s){Vector2 d=ends[s]-starts[s];return starts[s]+d*Mathf.Clamp01(Vector2.Dot(p-starts[s],d)/d.sqrMagnitude);}
            Vector2 Closest(Vector2 p){Vector2 best=p;float score=float.MaxValue;for(int i=0;i<starts.Count;i++){var at=Project(p,i);if((at-p).sqrMagnitude<score){score=(at-p).sqrMagnitude;best=at;}}return best;}
            int first=Node(Closest(from)),last=Node(Closest(to));for(int i=0;i<starts.Count;i++){Node(starts[i]);Node(ends[i]);}
            for(int i=0;i<starts.Count;i++)for(int j=i+1;j<starts.Count;j++)
            {
                bool hi=Mathf.Abs(starts[i].x-ends[i].x)>.1f,hj=Mathf.Abs(starts[j].x-ends[j].x)>.1f;if(hi==hj)continue;int h=hi?i:j,v=hi?j:i;Vector2 at=new Vector2(starts[v].x,starts[h].y);if(Vector2.Distance(Project(at,h),at)<.08f&&Vector2.Distance(Project(at,v),at)<.08f)Node(at);
            }
            int n=nodes.Count;var edges=new List<int>[n];for(int i=0;i<n;i++)edges[i]=new List<int>();
            for(int s=0;s<starts.Count;s++){var on=new List<int>();for(int i=0;i<n;i++)if(Vector2.Distance(Project(nodes[i],s),nodes[i])<.08f)on.Add(i);int segment=s;on.Sort((a,b)=>(nodes[a]-starts[segment]).sqrMagnitude.CompareTo((nodes[b]-starts[segment]).sqrMagnitude));for(int i=1;i<on.Count;i++){edges[on[i-1]].Add(on[i]);edges[on[i]].Add(on[i-1]);}}
            var distance=new float[n];var parent=new int[n];var used=new bool[n];for(int i=0;i<n;i++){distance[i]=float.MaxValue;parent[i]=-1;}distance[first]=0;
            for(int count=0;count<n;count++){int at=-1;for(int i=0;i<n;i++)if(!used[i]&&(at<0||distance[i]<distance[at]))at=i;if(at<0||distance[at]==float.MaxValue)break;if(at==last)break;used[at]=true;foreach(int other in edges[at]){float d=distance[at]+Vector2.Distance(nodes[at],nodes[other]);if(d<distance[other]){distance[other]=d;parent[other]=at;}}}
            if(distance[last]==float.MaxValue)return null;var result=new List<Vector2>();for(int at=last;at>=0;at=parent[at])result.Add(nodes[at]);result.Reverse();return result.ToArray();
        }
    }
}
