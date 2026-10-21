using System.Collections;using System.Linq;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class Features025PlayTests
    {
        [UnityTest]public IEnumerator NewCitizensKeepEightModelsAndCameraFollowsAimWhileStrafing()
        {
            var walkers=Object.FindObjectsByType<StreetWalker>(FindObjectsInactive.Include);
            Assert.AreEqual(8,walkers.Select(w=>w.GetComponent<ReferenceActorAnimation>().idle[0].name.Split('/')[0]).Distinct().Count());
            foreach(var walker in walkers)foreach(var frame in walker.frames)Assert.That(frame.name,Does.StartWith("citizen"));
            var cars=Object.FindObjectsByType<CityTraffic>();for(int i=0;i<cars.Length;i++)for(int j=i+1;j<cars.Length;j++)Assert.Greater(Vector2.Distance(cars[i].route[0],cars[j].route[0]),8,"Overlapping traffic spawn");
            var player=Object.FindAnyObjectByType<PlayerMotor>();player.Body.position=new Vector2(-20,-30);player.transform.position=player.Body.position;
            player.GetComponent<PlayerEquipment2D>().Select(PlayerEquipment.BlackCamera);player.GetComponent<PlayerInputReader>().Mobile(Vector2.right,Vector2.up,false,false,false,false);
            yield return new WaitForSeconds(.2f);var actor=player.GetComponent<ReferenceActorAnimation>();Assert.Greater(player.Body.linearVelocity.x,.1f);
            Assert.Greater(Vector2.Dot(player.GetComponent<PlayerAim>().Direction,Vector2.up),.9f);Assert.That(actor.walk.Skip(12).Take(3),Does.Contain(actor.visual.sprite));
        }
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;var input=Object.FindAnyObjectByType<PlayerInputReader>();input.SetPaused(false);input.SetGameplayLocked(false);}
        [UnityTest]public IEnumerator WheelSkipsLockedPinkAndFistsUseLeftClick()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();var equipment=player.GetComponent<PlayerEquipment2D>();Assert.That(equipment,Is.Not.Null);GameSession.Current.collected.Remove("weapon:xuxa");equipment.Select(PlayerEquipment.BlackCamera);equipment.Cycle(1);Assert.That(equipment.Selected,Is.EqualTo(PlayerEquipment.Fists));equipment.Cycle(1);Assert.That(equipment.Selected,Is.EqualTo(PlayerEquipment.BlackCamera));
            GameSession.Current.collected.Add("weapon:xuxa");yield return null;equipment.Select(PlayerEquipment.PinkCamera);yield return null;yield return null;Assert.That(player.GetComponent<ReferenceActorAnimation>().visual.sprite.name,Does.StartWith("amaral-pink / coerente026"));Assert.That(player.GetComponent<PlayerAim>().cameraProp.GetComponent<SpriteRenderer>().enabled,Is.False,"Do not draw a duplicate camera");
            equipment.Select(PlayerEquipment.Fists);float previous=player.GetComponent<CombatPlayer>().AttackStarted;player.GetComponent<PlayerInputReader>().Mobile(Vector2.zero,Vector2.down,false,false,false,true);yield return null;yield return null;Assert.That(player.GetComponent<CombatPlayer>().AttackStarted,Is.GreaterThan(previous));
        }
        [UnityTest]public IEnumerator StreetCarsCanBeDrivenAndExited()
        {
            var traffic=Object.FindObjectsByType<CityTraffic>().First();var car=traffic.GetComponent<DriveableCar>();Assert.That(car,Is.Not.Null);Assert.That(car.GetComponentInChildren<VehicleDoor>(),Is.Not.Null);car.Enter();Assert.That(DriveableCar.Current,Is.SameAs(car));Assert.That(traffic.enabled,Is.False);var player=Object.FindAnyObjectByType<PlayerMotor>();player.GetComponent<PlayerInputReader>().Mobile(Vector2.right,Vector2.down,false,false,false,false);yield return new WaitForSeconds(.3f);car.Exit();Assert.That(DriveableCar.Current,Is.Null);Assert.That(player.enabled,Is.True);Assert.That(player.Body.simulated,Is.True);
        }
        [UnityTest]public IEnumerator OncomingTrafficYieldsWithoutTeleporting()
        {
            foreach(var traffic in Object.FindObjectsByType<CityTraffic>())traffic.gameObject.SetActive(false);
            var player=Object.FindAnyObjectByType<PlayerMotor>();player.Body.position=new Vector2(-20,-20);player.transform.position=player.Body.position;
            var sprite=player.GetComponent<ReferenceActorAnimation>().idle[0];var cab=new GameObject("Priority cab test");cab.transform.position=new Vector2(-24,-30);
            var npc=new GameObject("Oncoming traffic test");npc.transform.position=new Vector2(-20,-30);npc.layer=LayerMask.NameToLayer("World");var body=npc.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;npc.AddComponent<BoxCollider2D>().size=new Vector2(3.25f,1.4f);npc.AddComponent<SpriteRenderer>().sprite=sprite;var agent=npc.AddComponent<CityTraffic>();agent.route=new[]{new Vector2(-20,-30),new Vector2(-35,-30)};agent.views=new[]{sprite,sprite,sprite,sprite};yield return null;
            Assert.That(agent.YieldFor(cab.transform,Vector2.right),Is.True);Assert.That(agent.Yielding,Is.True);var before=body.position;
            for(int i=0;i<60;i++){var last=body.position;yield return new WaitForFixedUpdate();Assert.That(Vector2.Distance(last,body.position),Is.LessThan(.2f),"Traffic teleported");}
            Assert.That(Mathf.Abs(body.position.y-before.y),Is.GreaterThan(1));cab.transform.position=new Vector2(-40,-30);yield return new WaitForFixedUpdate();Assert.That(agent.Yielding,Is.False);Object.Destroy(npc);Object.Destroy(cab);
        }
        [UnityTest]public IEnumerator WaypointCanBeQuotedAndMissionHasSkippableIntro()
        {
            MapWaypoint.Set(new Vector2(60,-22));Assert.That(MapWaypoint.Has,Is.True);Assert.That(TaxiService.Instance.SelectWaypoint(),Is.True);Assert.That(TaxiService.Instance.WaypointDestination,Is.True);Assert.That(TaxiService.Instance.Fare,Is.GreaterThanOrEqualTo(5));Assert.That(TaxiService.Instance.DestinationPoint,Is.EqualTo(MapWaypoint.Position));
            var world=WorldHub.Instance;var mission=world.Find("01_Riberal");var player=Object.FindAnyObjectByType<PlayerMotor>();world.Navigate(mission.id);player.Body.position=mission.entry;player.transform.position=player.Body.position;Assert.That(world.TryBegin(mission.id),Is.True);var intro=world.GetComponent<MissionIntro2D>();Assert.That(intro.Active,Is.True);Assert.That(player.GetComponent<PlayerInputReader>().GameplayLocked,Is.True);intro.Finish();Assert.That(intro.Active,Is.False);yield return null;
        }
        [UnityTest]public IEnumerator NewContractPhotoRequiresFlashAndPaysOnlyOnce()
        {
            GameSession.Current.collected.RemoveAll(id=>id.StartsWith("city:"));var contracts=WorldHub.Instance.GetComponent<CityContracts>();contracts.Accept();yield return null;yield return null;
            var player=Object.FindAnyObjectByType<PlayerMotor>();player.GetComponent<PlayerInputReader>().Mobile(Vector2.zero,Vector2.up,false,false,false,false);
            for(int step=0;step<3;step++)
            {var task=Object.FindObjectsByType<CityContractPoint>().Single();player.Body.position=(Vector2)task.transform.position+Vector2.down*1.5f;player.transform.position=player.Body.position;yield return null;task.Interact();Assert.That(CityContracts.Step(0),Is.EqualTo(step));Assert.That(task.Photograph(player.GetComponent<PlayerAim>()),Is.True);yield return null;yield return null;}
            Assert.That(CityContracts.Current,Is.EqualTo(1));int paid=GameSession.Current.currency;CityContracts.Complete(0,2);Assert.That(GameSession.Current.currency,Is.EqualTo(paid));Assert.That(GameSession.Current.collected.Contains("city:done:0"),Is.True);
        }
        [UnityTest]public IEnumerator DeliveryAndRecoveryContractsReachTheirRewards()
        {
            GameSession.Current.collected.RemoveAll(id=>id.StartsWith("city:"));GameSession.Current.collected.Add("city:done:0");var world=WorldHub.Instance;var contracts=world.GetComponent<CityContracts>();var player=Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();input.Mobile(Vector2.zero,Vector2.up,false,false,false,false);
            contracts.Accept();yield return null;yield return null;
            for(int step=0;step<2;step++){var point=Object.FindObjectsByType<CityContractPoint>().Single();player.Body.position=(Vector2)point.transform.position+Vector2.down*1.3f;player.transform.position=player.Body.position;yield return null;point.Interact();yield return null;yield return null;}
            Assert.That(CityContracts.Current,Is.EqualTo(2));contracts.Accept();yield return null;yield return null;var recovery=Object.FindObjectsByType<CityContractPoint>().Single();player.Body.position=(Vector2)recovery.transform.position+Vector2.down*1.3f;player.transform.position=player.Body.position;yield return null;yield return null;
            recovery.Interact();Assert.That(CityContracts.Current,Is.EqualTo(2));var guards=Object.FindObjectsByType<EnemyAgent>().Where(e=>e.rewardId!=null&&e.rewardId.StartsWith("city:guard:")).ToArray();Assert.That(guards.Length,Is.EqualTo(2));foreach(var guard in guards){guard.enabled=false;guard.GetComponent<Health>().Damage(100000,CombatTeam.Player,true);}recovery.Interact();yield return null;Assert.That(CityContracts.Current,Is.EqualTo(3));
        }
        [UnityTest,Timeout(120000)]public IEnumerator UberCompletesPhysicalWaypointTripPastOncomingCar()
        {
            var source=Object.FindObjectsByType<CityTraffic>().First();var views=source.views;foreach(var traffic in Object.FindObjectsByType<CityTraffic>())traffic.gameObject.SetActive(false);
            var player=Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();input.Mobile(Vector2.zero,Vector2.right,false,false,false,false);player.Body.position=new Vector2(-20,-30);player.transform.position=player.Body.position;Physics2D.SyncTransforms();GameSession.Current.currency=10000;
            MapWaypoint.Set(new Vector2(10,-30));var gps=Object.FindAnyObjectByType<CityNavigation>();gps.FollowWaypoint=true;var taxi=TaxiService.Instance;Assert.That(taxi.SelectWaypoint(),Is.True);Assert.That(taxi.RequestPickup(),Is.True);float deadline=Time.time+35;while(!taxi.Arrived&&Time.time<deadline)yield return new WaitForFixedUpdate();Assert.That(taxi.Arrived,Is.True,taxi.Notice);Assert.That(taxi.BeginRide(),Is.True,taxi.Notice);
            deadline=Time.time+10;while(taxi.RideState!=3&&Time.time<deadline)yield return new WaitForFixedUpdate();Assert.That(taxi.RideState,Is.EqualTo(3));var cab=taxi.Driver.transform.parent;var npc=new GameObject("Oncoming car in actual Uber route");npc.layer=LayerMask.NameToLayer("World");npc.transform.position=cab.position+Vector3.right*5;var body=npc.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;npc.AddComponent<BoxCollider2D>().size=new Vector2(3.25f,1.4f);var visual=npc.AddComponent<SpriteRenderer>();visual.sprite=views[0];var agent=npc.AddComponent<CityTraffic>();agent.views=views;agent.route=new[]{(Vector2)npc.transform.position,(Vector2)npc.transform.position+Vector2.left*20};bool yielded=false;
            deadline=Time.time+45;while(taxi.TripActive&&Time.time<deadline){yielded|=agent&&agent.Yielding;yield return new WaitForFixedUpdate();}
            Assert.That(yielded,Is.True,"The actual cab never requested passage");Assert.That(taxi.TripActive,Is.False,taxi.Diagnostics+taxi.Notice);Assert.That(taxi.TripDistance,Is.GreaterThan(20));Assert.That(Vector2.Distance(player.transform.position,MapWaypoint.Position),Is.LessThan(8));Object.Destroy(npc);
        }
    }
}
