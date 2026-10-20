using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class Mateus024Tests
    {
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;var input=Object.FindAnyObjectByType<PlayerInputReader>();input.SetPaused(false);input.SetGameplayLocked(false);}
        void Place(PlayerMotor player,Vector2 at){player.Body.position=at;player.transform.position=at;player.Body.linearVelocity=Vector2.zero;player.GetComponent<PlayerInputReader>().Mobile(Vector2.zero,Vector2.up,false,false,false,false);Physics2D.SyncTransforms();}
        [UnityTest,Timeout(180000)]public IEnumerator BothMateusChainsCanBeCompletedWithTheirActualActions()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();var quest=Object.FindAnyObjectByType<GoldenWeaponQuest>();Assert.That(quest,Is.Not.Null);
            GameSession.Current.collected.RemoveAll(x=>x.StartsWith("gold:"));
            foreach(var npc in Object.FindObjectsByType<StreetWalker>())npc.gameObject.SetActive(false);
            for(int chain=0;chain<2;chain++)
            {
                GameSession.Current.collected.Add("gold:start:"+chain);yield return null;yield return null;
                for(int step=0;step<3;step++)
                {
                    var task=Object.FindObjectsByType<GoldenQuestPickup>().Single(t=>t.chain==chain&&t.step==step);
                    Assert.That(QuestPlacement2D.Clear(task.transform.position),Is.True,"Blocked task "+chain+"/"+step);
                    Place(player,(Vector2)task.transform.position+Vector2.down*.9f);yield return null;
                    if(task.Kind==GoldenTaskKind.Photograph)
                    {task.Interact();Assert.That(GoldenWeaponQuest.Step(chain),Is.EqualTo(step),"E must not complete a photo");yield return null;Assert.That(task.TryPhotograph(player.GetComponent<PlayerAim>()),Is.True);}
                    else
                    {
                        if(task.Kind==GoldenTaskKind.Recover)
                        {
                            yield return null;Assert.That(task.EncounterReady,Is.True,"Missing encounter");Assert.That(task.RemainingGuards,Is.EqualTo(2));task.Interact();Assert.That(GoldenWeaponQuest.Step(chain),Is.EqualTo(step));
                            foreach(var enemy in task.GetComponentsInChildren<EnemyAgent>()){enemy.enabled=false;enemy.GetComponent<Health>().Damage(100000,CombatTeam.Player,true);}
                        }
                        task.Interact();yield return new WaitForSeconds(task.Kind==GoldenTaskKind.Repair?3.2f:1.8f);
                    }
                    Assert.That(GoldenWeaponQuest.Step(chain),Is.EqualTo(step+1));yield return null;yield return null;
                }
                quest.Talk();yield return null;yield return null;var reward=Object.FindObjectsByType<GoldenQuestPickup>().Single(t=>t.chain==chain&&t.step==3);
                Place(player,(Vector2)reward.transform.position+Vector2.down*.8f);yield return null;reward.Interact();yield return null;
                Assert.That(chain==0?GoldenWeaponQuest.Glock:GoldenWeaponQuest.Razor,Is.True);
            }
        }
        [UnityTest,Timeout(180000)]public IEnumerator AllErrandsHaveWalkableCityRoutes()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();Physics2D.SyncTransforms();
            for(int c=0;c<2;c++)for(int s=0;s<3;s++)
            {var goal=GoldenWeaponQuest.StopPosition(c,s);Assert.That(QuestPlacement2D.Clear(goal),Is.True);var route=TaxiCollisionRoute.Plan(Mateus.ApproachPosition,goal,player.Body,.29f,.8f);Assert.That(route,Is.Not.Null,"No city route "+c+"/"+s);Assert.That(route.Length,Is.GreaterThan(0));Assert.That(Vector2.Distance(route[route.Length-1],goal),Is.LessThan(.8f));yield return null;}
        }
        [UnityTest]public IEnumerator QuestCannotBeCollectedRemotelyAndGateDoesNotInterruptErrands()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();GameSession.Current.collected.Add("gold:start:0");yield return null;yield return null;
            var task=Object.FindObjectsByType<GoldenQuestPickup>().First(t=>t.chain==0&&t.step==0);Place(player,Mateus.ApproachPosition);task.Interact();yield return new WaitForSeconds(1.8f);Assert.That(GoldenWeaponQuest.Step(0),Is.Zero);
            var gps=Object.FindAnyObjectByType<CityNavigation>();gps.FollowMateus=true;var mission=WorldHub.Instance.Find("01_Riberal");Place(player,mission.entry);Assert.That(WorldHub.Instance.TryBegin(mission.id),Is.False);
        }
    }
}
