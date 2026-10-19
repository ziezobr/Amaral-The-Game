using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
namespace PontoCego.Tests
{
    public sealed class ReleaseRepairTests
    {
        [UnitySetUp] public IEnumerator Setup(){GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;}
        [UnityTest]public IEnumerator PersistentPartsAreAccessibleAndSixTypesExist()
        {var parts=Object.FindObjectsByType<PcCollectible>();Assert.That(parts.Length,Is.GreaterThanOrEqualTo(6));foreach(var part in parts){Assert.IsFalse(Physics2D.OverlapCircle(part.transform.position,.3f,LayerMask.GetMask("World")),part.name);part.Interact();}Assert.That(PcCollectible.TypesCollected,Is.EqualTo(6));yield return null;}
        [UnityTest]public IEnumerator GoldenRewardsRequireThreeErrandsAndReturn()
        {
            var q=Object.FindAnyObjectByType<GoldenWeaponQuest>();Assert.That(q,Is.Not.Null);q.Talk();yield return null;Assert.IsFalse(GoldenWeaponQuest.Glock);
            for(int step=0;step<3;step++){yield return null;var item=Object.FindAnyObjectByType<GoldenQuestPickup>();Assert.That(item.step,Is.EqualTo(step));item.Interact();yield return null;}
            Assert.IsFalse(GoldenWeaponQuest.Glock);q.Talk();yield return null;var reward=Object.FindAnyObjectByType<GoldenQuestPickup>();Assert.That(reward.step,Is.EqualTo(3));reward.Interact();Assert.IsTrue(GoldenWeaponQuest.Glock);Assert.IsFalse(GoldenWeaponQuest.Razor);
        }
        [UnityTest]public IEnumerator InteractionInputActuallyCollectsHiddenPart()
        {
            var p=Object.FindAnyObjectByType<PlayerMotor>();p.GetComponent<StoryDialogue>().DismissForValidation();var input=p.GetComponent<PlayerInputReader>();input.SetPaused(false);input.SetGameplayLocked(false);
            var part=Object.FindAnyObjectByType<PcCollectible>();p.Body.position=part.transform.position;p.transform.position=p.Body.position;Physics2D.SyncTransforms();yield return null;input.Mobile(Vector2.zero,Vector2.down,false,false,true,false);yield return null;yield return null;Assert.IsTrue(GameSession.Current.collected.Contains(part.id),p.GetComponent<Interaction>().Prompt);
        }
        [UnityTest,Timeout(45000)]public IEnumerator TaxiBoardsDrivesWholeRouteAndDisembarks()
        {
            foreach(var car in Object.FindObjectsByType<CityTraffic>())car.gameObject.SetActive(false);
            var p=Object.FindAnyObjectByType<PlayerMotor>();p.GetComponent<StoryDialogue>().DismissForValidation();p.GetComponent<PlayerInputReader>().SetPaused(false);
            var taxi=TaxiService.Instance;taxi.SelectDestination("01_Riberal");Assert.IsTrue(taxi.RequestPickup(),taxi.Notice);Time.timeScale=8;float deadline=Time.time+30;while(!taxi.Arrived&&Time.time<deadline)yield return null;Assert.IsTrue(taxi.Arrived,taxi.Notice);
            p.Body.position=taxi.Pickup+Vector2.right*2;p.transform.position=p.Body.position;Physics2D.SyncTransforms();Assert.IsTrue(taxi.BeginRide(),taxi.Notice);deadline=Time.time+100;while(taxi.TripActive&&Time.time<deadline)yield return null;
            Assert.IsFalse(taxi.TripActive,"Ride stalled: "+taxi.RideState+" distance "+taxi.TripDistance+" "+taxi.Notice+" "+taxi.Diagnostics+taxi.Contacts);Assert.That(taxi.TripDistance,Is.GreaterThan(40));Assert.That(Vector2.Distance(p.transform.position,WorldHub.Instance.Find("01_Riberal").entry),Is.LessThan(4));Assert.IsTrue(p.enabled);Assert.IsFalse(p.GetComponent<PlayerInputReader>().GameplayLocked);Time.timeScale=1;
        }
        [UnityTest]public IEnumerator TaxiUsesGrayPeugeotAndCleanCivilianFrames()
        {Assert.That(TaxiService.Instance.sprites[0].name,Does.StartWith("Carro limpo 0"));foreach(var npc in Object.FindObjectsByType<StreetWalker>()){Assert.That(npc.visual.sprite.bounds.size.y,Is.LessThanOrEqualTo(1.9f));Assert.That(npc.frames.Length,Is.EqualTo(4));}yield return null;}
    }
}
