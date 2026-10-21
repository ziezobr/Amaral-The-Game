using NUnit.Framework;
using UnityEngine;
namespace PontoCego.Tests
{
    public sealed class Animation026Tests
    {
        [Test]public void AnimationHasTwentyFourPosesForEachOfEightDirections()
        {Assert.AreEqual(24,DirectionalPose24.FramesPerDirection);Assert.AreEqual(192,DirectionalPose24.FramesPerDirection*8);}
        [Test]public void ClosedRoadHasOppositeRightHandLanesAndRoundedCorners()
        {
            var a=new[]{new Vector2(0,0),new Vector2(20,0),new Vector2(20,20),new Vector2(0,20)};
            var b=new[]{a[1],a[2],a[3],a[0]};var routes=TrafficLaneRoutes.Build(a,b);
            Assert.AreEqual(2,routes.Count);foreach(var route in routes){Assert.GreaterOrEqual(route.Length,20);foreach(var p in route){float distance=float.MaxValue;for(int edge=0;edge<4;edge++){var ab=b[edge]-a[edge];var closest=a[edge]+ab*Mathf.Clamp01(Vector2.Dot(p-a[edge],ab)/ab.sqrMagnitude);distance=Mathf.Min(distance,Vector2.Distance(p,closest));}Assert.Greater(distance,.9f);Assert.Less(distance,2f);}}
        }
        [Test]public void DeadEndDoesNotProduceUTurnThroughOpposingTraffic()
        {Assert.IsEmpty(TrafficLaneRoutes.Build(new[]{Vector2.zero},new[]{Vector2.right*20}));}
        [Test]public void CrossingAndTJunctionsAreConnectedBeforeLaneGeneration()
        {
            var a=new[]{new Vector2(0,0),new Vector2(20,0),new Vector2(20,20),new Vector2(0,20),new Vector2(10,0),new Vector2(10,10)};
            var b=new[]{a[1],a[2],a[3],a[0],new Vector2(10,20),new Vector2(15,10)};Assert.AreEqual(3,TrafficLaneRoutes.Build(a,b).Count);
        }
    }
}
