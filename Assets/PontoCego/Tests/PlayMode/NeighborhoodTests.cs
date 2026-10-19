using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class NeighborhoodTests
    {
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;GameSession.ExploreNeighborhood();yield return null;yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){GameSession.Current.openNeighborhood=false;GameSession.Current.flashCharges=12;GameSession.Current.allyAmmo=48;yield return null;}
        [UnityTest]public IEnumerator MainMissionReturnsToNeighborhoodWithoutSkippingProgress()
        {
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();
            world.Navigate("04_Emboscada");player.Body.position=world.Find("04_Emboscada").entry;player.transform.position=player.Body.position;
            Assert.That(world.TryBegin("04_Emboscada"),Is.True);yield return null;
            GameSession.Travel("05_ResgateMartins");yield return null;Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo(WorldHub.SceneName));Assert.That(GameSession.Current.mainMission,Is.EqualTo("05_ResgateMartins"));
            world.Navigate("24_Mocambo");player.Body.position=world.Find("24_Mocambo").entry;player.transform.position=player.Body.position;
            Assert.That(world.TryBegin("24_Mocambo"),Is.True);yield return null;GameSession.Travel("07_Distribuidora");yield return null;
            Assert.That(WorldHub.Instance,Is.SameAs(world));Assert.That(GameSession.Current.mainMission,Is.EqualTo("05_ResgateMartins"));
        }
        [UnityTest]public IEnumerator DeliveryRewardsOnlyOnceAndResourcesRequirePurchase()
        {
            var gates=Object.FindObjectsByType<HubGate>();HubGate start=null,finish=null;
            foreach(var gate in gates){if(gate.destination==HubDestination.CourierStart)start=gate;if(gate.destination==HubDestination.CourierFinish)finish=gate;}
            int cash=GameSession.Current.currency;start.Interact();finish.Interact();finish.Interact();Assert.That(GameSession.Current.currency,Is.EqualTo(cash+120));
            var inventory=Object.FindAnyObjectByType<AmmoInventory>();GameSession.Current.flashCharges=1;Assert.That(inventory.SpendFlash(),Is.True);Assert.That(inventory.SpendFlash(),Is.False);
            var shop=Object.FindAnyObjectByType<EquipmentShop>();shop.Buy(5);Assert.That(GameSession.Current.flashCharges,Is.EqualTo(8));
            GameSession.Current.hasSidearm=true;GameSession.Current.magazine=0;GameSession.Current.ammoReserve=3;inventory.Reload();yield return new WaitForSeconds(1.4f);
            Assert.That(GameSession.Current.magazine,Is.EqualTo(3));Assert.That(GameSession.Current.ammoReserve,Is.Zero);
        }
        [UnityTest]public IEnumerator CarsDriveAndAllowSafeExitAmongPedestrians()
        {
            Assert.That(Object.FindObjectsByType<StreetWalker>().Length,Is.GreaterThanOrEqualTo(20));Assert.That(Object.FindObjectsByType<DriveableCar>().Length,Is.EqualTo(3));
            var player=Object.FindAnyObjectByType<PlayerMotor>();var car=Object.FindAnyObjectByType<DriveableCar>();Vector2 before=car.transform.position;car.Enter();
            player.GetComponent<PlayerInputReader>().Mobile(Vector2.right,Vector2.right,false,false,false,false);
            for(int i=0;i<15;i++)yield return new WaitForFixedUpdate();
            Assert.That(Vector2.Distance(before,car.transform.position),Is.GreaterThan(.05f));Assert.That(player.Body.simulated,Is.False);
            Assert.That(Physics2D.OverlapCircle(car.GetComponentInChildren<VehicleDoor>().transform.position,.1f,LayerMask.GetMask("World")),Is.Null,"Door must remain outside the collision after changing direction");
            car.Exit();Assert.That(DriveableCar.Current,Is.Null);Assert.That(player.Body.simulated,Is.True);Assert.That(player.enabled,Is.True);
            Assert.That(Physics2D.OverlapCircle(player.transform.position,.23f,LayerMask.GetMask("World")),Is.Null);
        }
        [UnityTest]public IEnumerator UberListsInstalledPhasesAndPreservesEquipmentDuringTravel()
        {
            var driver=Object.FindAnyObjectByType<UberDriver>();Assert.That(driver,Is.Not.Null);
            foreach(string destination in UberDriver.Destinations)Assert.That(Application.CanStreamedLevelBeLoaded(destination),Is.True,destination);
            int money=GameSession.Current.currency;GameSession.Current.ammoReserve=17;driver.Interact();
            Assert.That(Object.FindAnyObjectByType<PlayerInputReader>().GameplayLocked,Is.True);
            driver.Close();Assert.That(Object.FindAnyObjectByType<PlayerInputReader>().GameplayLocked,Is.False);
            driver.Ride("24_Mocambo");yield return null;yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo(WorldHub.SceneName));Assert.That(WorldHub.Instance.Active,Is.Null);Assert.That(WorldHub.Instance.Destination,Is.EqualTo("24_Mocambo"));Assert.That(GameSession.Current.currency,Is.EqualTo(money));Assert.That(GameSession.Current.ammoReserve,Is.EqualTo(17));
            Assert.That(Object.FindAnyObjectByType<UberDriver>(),Is.Not.Null);
        }
    }
}
