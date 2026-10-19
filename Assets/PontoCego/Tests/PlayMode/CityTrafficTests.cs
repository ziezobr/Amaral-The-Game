using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PontoCego.Tests
{
    public sealed class CityTrafficTests
    {
        [UnityTest] public IEnumerator TrafficBrakesForPedestrianAndResumesWhenLaneClears()
        {
            Time.timeScale=1;
            GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;
            var input=Object.FindAnyObjectByType<PlayerInputReader>();input.SetPaused(false);
            var reference=Object.FindAnyObjectByType<CityTraffic>();
            Assert.That(reference,Is.Not.Null);
            // An isolated physical lane avoids interference from authored city traffic or walls.
            Vector2 start=new Vector2(400,400);
            var car=new GameObject("QA / faixa isolada / carro");car.layer=8;car.transform.position=start;
            var body=car.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;
            car.AddComponent<BoxCollider2D>().size=new Vector2(3.25f,1.4f);
            var picture=new GameObject("Carro");picture.transform.SetParent(car.transform,false);
            picture.AddComponent<SpriteRenderer>().sprite=reference.views[0];
            var traffic=car.AddComponent<CityTraffic>();traffic.views=reference.views;traffic.speed=4.2f;
            traffic.route=new[]{start,start+Vector2.right*20,start+new Vector2(20,10),start+Vector2.up*10};
            yield return null;
            for(int i=0;i<25;i++)yield return new WaitForFixedUpdate();
            Assert.That(body.position.x-start.x,Is.GreaterThan(.5f),"The unobstructed car must first drive normally.");

            body.position=start;car.transform.position=start;body.linearVelocity=Vector2.zero;
            var pedestrian=new GameObject("QA / pedestre aguardando travessia");pedestrian.layer=13;pedestrian.transform.position=start+Vector2.right*3;
            var pedestrianBody=pedestrian.AddComponent<Rigidbody2D>();pedestrianBody.bodyType=RigidbodyType2D.Kinematic;
            pedestrian.AddComponent<CircleCollider2D>().radius=.25f;
            // Keep the pedestrian stationary; its presence must still count for vehicle avoidance.
            pedestrian.AddComponent<StreetWalker>().enabled=false;
            Physics2D.SyncTransforms();
            for(int i=0;i<25;i++)yield return new WaitForFixedUpdate();
            Assert.That(body.position.x-start.x,Is.LessThan(.35f),"Traffic must brake before physical contact with the pedestrian.");
            Assert.That(body.linearVelocity.x,Is.LessThan(.25f));
            Assert.That(Vector2.Distance(body.position,pedestrian.transform.position),Is.GreaterThan(2),"A passing assertion must not rely on collision stopping the car.");

            pedestrian.SetActive(false);float stoppedAt=body.position.x;
            for(int i=0;i<25;i++)yield return new WaitForFixedUpdate();
            Assert.That(body.position.x-stoppedAt,Is.GreaterThan(.5f),"Stale entries in the reusable hit buffer must not keep the car stopped.");
            Object.Destroy(car);Object.Destroy(pedestrian);yield return null;
        }
    }
}

