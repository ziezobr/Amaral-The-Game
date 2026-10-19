using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class RebuiltCityTests
    {
        [UnityTest]public IEnumerator InteriorRevealsWithoutChangingPlayerCameraOrPhysics()
        {
            Time.timeScale=1;GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();var camera=Camera.main;InteriorFacade2D office=null;
            foreach(var building in Object.FindObjectsByType<InteriorFacade2D>())if(Vector2.Distance(building.footprint.center,new Vector2(-60,0))<1)office=building;
            Assert.That(office,Is.Not.Null);int colliders=Object.FindObjectsByType<Collider2D>().Length;
            player.Body.position=new Vector2(-37,-20);player.transform.position=player.Body.position;yield return new WaitForSeconds(.6f);
            Assert.That(office.exterior[0].color.a,Is.GreaterThan(.9f));
            player.Body.position=world.Find("01_Riberal").entry;player.transform.position=player.Body.position;yield return new WaitForSeconds(.6f);
            Assert.That(office.exterior[0].color.a,Is.LessThan(.1f));Assert.That(WorldHub.Instance,Is.SameAs(world));Assert.That(Camera.main,Is.SameAs(camera));Assert.That(Object.FindAnyObjectByType<PlayerMotor>(),Is.SameAs(player));
            Assert.That(Object.FindObjectsByType<Collider2D>().Length,Is.GreaterThanOrEqualTo(colliders));
        }
        [UnityTest]public IEnumerator MainOutdoorBackdropsAreReplacedAndPedestrianRoutesAvoidStaticWalls()
        {
            GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;
            Assert.That(GameObject.Find("Bairro / cenário próprio"),Is.Null);Assert.That(WorldHub.Instance.rebuiltOutdoorLayout,Is.True);
            Assert.That(Object.FindObjectsByType<InteriorFacade2D>().Length,Is.EqualTo(5));int residents=0;
            foreach(var walker in Object.FindObjectsByType<StreetWalker>())
            {
                residents++;Assert.That(walker.route.Length,Is.EqualTo(2));var delta=walker.route[1]-walker.route[0];
                foreach(var hit in Physics2D.CircleCastAll(walker.route[0],.27f,delta.normalized,delta.magnitude,LayerMask.GetMask("World")))
                    if(hit.collider&&!hit.collider.isTrigger&&(!hit.rigidbody||hit.rigidbody.bodyType==RigidbodyType2D.Static))Assert.Fail(walker.name+" route crosses "+hit.collider.name);
            }
            Assert.That(residents,Is.GreaterThanOrEqualTo(40));
        }
    }
}
