using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class LandscapeIntegrationTests
    {
        [UnityTest]public IEnumerator SixLandmarksConnectToTheSameWorldAndRewardOnlyOnce()
        {
            Time.timeScale=1;GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();var places=Object.FindObjectsByType<EnvironmentalLandmark>();Assert.That(places.Length,Is.EqualTo(6));
            Assert.That(world.bounds.width,Is.GreaterThan(280));Assert.That(world.bounds.height,Is.GreaterThan(164));
            Assert.That(world.landmarkMaps.Length,Is.EqualTo(6));Assert.That(world.landmarkPatches.Length,Is.EqualTo(6));
            foreach(var place in places)
            {
                Assert.That(world.IsWalkable(place.transform.position),Is.True,place.title+" blocked anchor");
                var route=TaxiCollisionRoute.Plan(player.Body.position,place.transform.position,player.Body,.35f,.8f);Assert.That(route,Is.Not.Null,place.title+" unreachable");
                int money=GameSession.Current.currency;place.Interact();place.Interact();Assert.That(GameSession.Current.currency,Is.EqualTo(money+25));Assert.That(place.Discovered,Is.True);
            }
            Assert.That(WorldHub.Instance,Is.SameAs(world));
        }
    }
}
