using System.Collections.Generic;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Closed right-hand lanes derived from the actual road graph, including junction corners.</summary>
    public static class TrafficLaneRoutes
    {
        public static Vector2[] StartAtDistance(Vector2[] lane,float distance)
        {
            float length=0;for(int i=0;i<lane.Length;i++)length+=Vector2.Distance(lane[i],lane[(i+1)%lane.Length]);distance=Mathf.Repeat(distance,length);
            for(int i=0;i<lane.Length;i++)
            {var a=lane[i];var b=lane[(i+1)%lane.Length];float segment=Vector2.Distance(a,b);if(distance<=segment){var result=new Vector2[lane.Length+1];result[0]=Vector2.Lerp(a,b,segment>.001f?distance/segment:0);for(int j=0;j<lane.Length;j++)result[j+1]=lane[(i+j+1)%lane.Length];return result;}distance-=segment;}
            return lane;
        }
        public static List<Vector2[]> Build(Vector2[] starts,Vector2[] ends)
        {
            var result=new List<Vector2[]>();if(starts==null||ends==null)return result;
            var nodes=new List<Vector2>();var links=new List<List<int>>();
            int Node(Vector2 p){for(int i=0;i<nodes.Count;i++)if((nodes[i]-p).sqrMagnitude<.01f)return i;nodes.Add(p);links.Add(new List<int>());return nodes.Count-1;}
            float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
            int count=Mathf.Min(starts.Length,ends.Length);
            for(int i=0;i<count;i++)
            {
                Vector2 origin=starts[i],delta=ends[i]-origin;if(delta.sqrMagnitude<.001f)continue;
                var cuts=new List<float>{0,1};
                for(int j=0;j<count;j++)
                {
                    if(j==i)continue;Vector2 other=ends[j]-starts[j],offset=starts[j]-origin;float denominator=Cross(delta,other);
                    if(Mathf.Abs(denominator)>.0001f)
                    {float t=Cross(offset,other)/denominator,u=Cross(offset,delta)/denominator;if(t>=0&&t<=1&&u>=0&&u<=1)cuts.Add(t);}
                    else if(Mathf.Abs(Cross(offset,delta))<.01f)
                    {float a=Vector2.Dot(offset,delta)/delta.sqrMagnitude,b=Vector2.Dot(ends[j]-origin,delta)/delta.sqrMagnitude;if(a>0&&a<1)cuts.Add(a);if(b>0&&b<1)cuts.Add(b);}
                }
                cuts.Sort();for(int k=1;k<cuts.Count;k++)
                {if(cuts[k]-cuts[k-1]<.0001f)continue;int a=Node(origin+delta*cuts[k-1]),b=Node(origin+delta*cuts[k]);if(a==b)continue;if(!links[a].Contains(b))links[a].Add(b);if(!links[b].Contains(a))links[b].Add(a);}
            }
            var visited=new HashSet<long>();
            for(int a=0;a<nodes.Count;a++)foreach(int b in links[a])
            {
                long key=(long)a*nodes.Count+b;if(visited.Contains(key))continue;
                int previous=a,current=b;var cycle=new List<int>{a};bool closed=false;
                for(int step=0;step<nodes.Count*4;step++)
                {
                    visited.Add((long)previous*nodes.Count+current);cycle.Add(current);
                    int chosen=-1;float turn=361;Vector2 forward=(nodes[current]-nodes[previous]).normalized;
                    foreach(int candidate in links[current]){if(candidate==previous)continue;float angle=Vector2.SignedAngle(forward,nodes[candidate]-nodes[current]);if(angle<turn){turn=angle;chosen=candidate;}}
                    // Traverse a dangling branch only while extracting the face; strip it before building a lane.
                    if(chosen<0)chosen=previous;
                    if(current==a&&chosen==b){closed=true;break;}
                    previous=current;current=chosen;
                }
                if(!closed||cycle.Count<5)continue;cycle.RemoveAt(cycle.Count-1);
                bool simplified=true;while(simplified&&cycle.Count>2)
                {
                    simplified=false;for(int i=0;i<cycle.Count;i++)if(cycle[(i+cycle.Count-1)%cycle.Count]==cycle[(i+1)%cycle.Count])
                    {cycle.RemoveAt(i);cycle.RemoveAt(i%cycle.Count);simplified=true;break;}
                }
                if(cycle.Count<4)continue;
                if(new HashSet<int>(cycle).Count!=cycle.Count)continue;
                var corners=new List<Vector2>();
                for(int i=0;i<cycle.Count;i++)
                {Vector2 p=nodes[cycle[i]],incoming=(p-nodes[cycle[(i+cycle.Count-1)%cycle.Count]]).normalized,outgoing=(nodes[cycle[(i+1)%cycle.Count]]-p).normalized;var r1=new Vector2(incoming.y,-incoming.x);var r2=new Vector2(outgoing.y,-outgoing.x);corners.Add(p+(r1+r2)*(1.5f/Mathf.Max(.5f,1+Vector2.Dot(r1,r2))));}
                var lane=new List<Vector2>();
                for(int i=0;i<corners.Count;i++)
                {var p=corners[i];var before=corners[(i+corners.Count-1)%corners.Count];var after=corners[(i+1)%corners.Count];float radius=Mathf.Min(1.1f,Mathf.Min(Vector2.Distance(p,before),Vector2.Distance(p,after))*.2f);var enter=p+(before-p).normalized*radius;var exit=p+(after-p).normalized*radius;for(int sample=0;sample<5;sample++){float t=sample/4f;lane.Add((1-t)*(1-t)*enter+2*(1-t)*t*p+t*t*exit);}}
                result.Add(lane.ToArray());
            }
            return result;
        }
    }
}
