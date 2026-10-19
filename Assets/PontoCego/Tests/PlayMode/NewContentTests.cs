using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class NewContentTests
    {
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;TaxiService.Instance?.Cancel();yield return null;}
        [UnityTest]public IEnumerator FleetAndCitizensUseSuppliedReferences()
        {Assert.That(Object.FindObjectsByType<CityTraffic>().Length,Is.GreaterThanOrEqualTo(16));foreach(var car in Object.FindObjectsByType<CityTraffic>()){Assert.That(car.views.Length,Is.EqualTo(4));Assert.That(car.route.Length,Is.GreaterThanOrEqualTo(4));Assert.That(car.GetComponent<BoxCollider2D>(),Is.Not.Null);}foreach(var npc in Object.FindObjectsByType<StreetWalker>())Assert.That(npc.frames[0].name,Does.StartWith("Cidad"));yield return null;}
        [UnityTest]public IEnumerator AllMissionsContainPcPartsAndWorkbenchRequiresSixTypes()
        {
            foreach(var mission in WorldHub.Instance.missions)Assert.That(mission.actors.GetComponentInChildren<PcCollectible>(true),Is.Not.Null,mission.id);
            var table=Object.FindAnyObjectByType<PcWorkbench>();table.Interact();Object.FindAnyObjectByType<StoryDialogue>().DismissForValidation();Assert.That(GameSession.Current.pcAssembled,Is.False);
            for(int i=0;i<6;i++){var go=new GameObject("test part");var piece=go.AddComponent<PcCollectible>();piece.kind=i;piece.id="test:pc:"+i;piece.Interact();piece.Interact();Object.Destroy(go);}
            Assert.That(PcCollectible.TypesCollected,Is.EqualTo(6));table.Interact();Object.FindAnyObjectByType<StoryDialogue>().DismissForValidation();Assert.That(GameSession.Current.pcAssembled,Is.True);Assert.That(GameSession.Current.motherboardBombs,Is.EqualTo(5));table.Interact();Assert.That(GameSession.Current.motherboardBombs,Is.EqualTo(5));yield return null;
        }
        [UnityTest]public IEnumerator PauloGateRequiresAssemblyAndBossesHaveCorrectPhaseCounts()
        {
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();world.Navigate("20_Paulo");player.Body.position=world.Find("20_Paulo").entry;player.transform.position=player.Body.position;GameSession.Current.pcAssembled=false;
            Assert.That(world.TryBegin("20_Paulo"),Is.False);GameSession.Current.pcAssembled=true;Assert.That(world.TryBegin("20_Paulo"),Is.True);yield return null;player.GetComponent<StoryDialogue>().DismissForValidation();
            world.Active.level.RestoreStep(world.Active.level.objectives.Length-2);yield return null;var boss=world.Active.actors.GetComponentInChildren<BossAbilities>(true);Assert.That(boss.PhaseCount,Is.EqualTo(5));for(int i=1;i<=5;i++){boss.GetComponent<Health>().Restore(boss.GetComponent<Health>().maximum*(1-(i-.5f)/5));Assert.That(boss.Phase,Is.EqualTo(i));}
        }
        [UnityTest,Timeout(30000)]public IEnumerator TaxiHonorsTenSecondsAndActuallyArrives()
        {
            foreach(var traffic in Object.FindObjectsByType<CityTraffic>())traffic.gameObject.SetActive(false);
            var taxi=TaxiService.Instance;Assert.That(taxi.arrivalSeconds,Is.EqualTo(10));Assert.That(taxi.RequestPickup(),Is.True,taxi.Notice);Assert.That(taxi.Remaining,Is.GreaterThan(9));
            Time.timeScale=4;yield return new WaitForSeconds(7);Assert.That(taxi.Arrived,Is.False);
            float until=Time.time+25;while(!taxi.Arrived&&Time.time<until)yield return null;Assert.That(taxi.Arrived,Is.True,taxi.Notice);Assert.That(taxi.Driver,Is.Not.Null);Assert.That(taxi.Remaining,Is.EqualTo(0));taxi.Cancel();Time.timeScale=1;
        }
        [UnityTest]public IEnumerator HudReflectsRealDamageAndSprintDoesNotConsumeHealth()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();var hp=player.GetComponent<Health>();var input=player.GetComponent<PlayerInputReader>();var hud=Object.FindAnyObjectByType<GameHud>();hp.Damage(20,CombatTeam.Enemy);Assert.That(hud.LifeFraction,Is.EqualTo(hp.Current/hp.maximum).Within(.001));float life=hp.Current;
            input.Mobile(Vector2.right,Vector2.down,true,false,false,false);yield return new WaitForSeconds(.5f);input.Mobile(Vector2.zero,Vector2.down,false,false,false,false);Assert.That(hp.Current,Is.EqualTo(life));Assert.That(hud.StaminaFraction,Is.LessThan(1));Assert.That(hud.CameraCount,Is.EqualTo(GameSession.Current.flashCharges));
        }
    }
}
