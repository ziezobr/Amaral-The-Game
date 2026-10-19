using NUnit.Framework;
using UnityEngine;
using PontoCego.WorldEnvironment;
namespace PontoCego.Tests
{
    public sealed class WorldLightModelTests
    {
        [Test] public void SunAndMoonDirectionsStayContinuousThroughTwilight()
        {
            var previous=DayNightCycle.DirectionAtHour(0);
            for(int sample=1;sample<=4800;sample++)
            {
                float hour=sample*.005f;var direction=DayNightCycle.DirectionAtHour(hour);
                Assert.That(direction.magnitude,Is.EqualTo(1).Within(.0001f));
                Assert.That(Vector2.Dot(previous,direction),Is.GreaterThan(.996f),"Light direction jumped at hour "+hour);
                Assert.That(direction.y,Is.GreaterThan(0));previous=direction;
            }
        }
        [TestCase(6.115f)][TestCase(17.885f)][TestCase(24f)]
        public void TinyClockChangesDoNotFlipTheProjectedShadow(float hour)
        {
            var before=DayNightCycle.DirectionAtHour(hour-.001f);var after=DayNightCycle.DirectionAtHour(hour+.001f);
            Assert.That(Vector2.Angle(before,after),Is.LessThan(2));
        }
    }
}
