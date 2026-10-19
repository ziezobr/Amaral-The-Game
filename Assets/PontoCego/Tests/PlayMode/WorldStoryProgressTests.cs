using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class WorldStoryProgressTests
    {
        // Tests quest state and transitions, not combat difficulty or promised playtime.
        [UnityTest,Timeout(180000)]public IEnumerator StoryObjectivesCanReachEndingInsideOneWorld()
        {
            float fixedStep=Time.fixedDeltaTime,originalScale=Time.timeScale;
            try
            {
            // Accelerate scripted waits without multiplying the physics workload by six.
            Time.fixedDeltaTime=fixedStep*6;
            GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;var world=WorldHub.Instance;var motor=Object.FindAnyObjectByType<PlayerMotor>();var input=motor.GetComponent<PlayerInputReader>();motor.GetComponent<Health>().IncreaseMaximum(100000);GameSession.Current.pcAssembled=true;
            Time.timeScale=6;
            for(int operation=0;operation<260;operation++)
            {
                input.SetPaused(false);Time.timeScale=6;var dialogue=motor.GetComponent<StoryDialogue>();dialogue.DismissForValidation();var ending=Object.FindAnyObjectByType<CampaignEnding>();if(ending&&ending.Active){Time.timeScale=1;Assert.That(world.Active.id,Is.EqualTo("23_Amanhecer"));yield break;}
                if(world.Active==null){string id=GameSession.Current.mainMission;var entry=world.Find(id).entry;motor.Body.position=entry+Vector2.down*4;motor.transform.position=motor.Body.position;yield return null;motor.Body.position=entry;motor.transform.position=entry;Physics2D.SyncTransforms();Assert.IsTrue(world.TryBegin(id),"Cannot activate "+id);yield return null;dialogue.DismissForValidation();}
                var active=world.Active;var level=active.level;int before=level.Step;Debug.Log("PONTO_STORY_QA / "+active.id+" / "+before+" / "+Time.realtimeSinceStartup);var target=level.CurrentTarget;Assert.IsNotNull(target,active.id+" / "+before);var mission=active.actors.GetComponentInChildren<CombatMission>();
                if(mission&&before<mission.rules.Length&&mission.rules[before].kind==MissionRuleKind.Fight){yield return null;foreach(var enemy in mission.rules[before].enemies)if(enemy&&enemy.Alive)enemy.GetComponent<Health>().Damage(100000,CombatTeam.Player);}
                else if(target.TryGetComponent<PhotoTarget>(out var photo))photo.Photograph();
                else if(target.TryGetComponent<StoryInteractable>(out var story)){story.Interact();dialogue.DismissForValidation();}
                else if(target.TryGetComponent<EquipmentShop>(out var shop)){shop.Interact();dialogue.DismissForValidation();if(shop.Active)shop.Close();}
                else if(target.TryGetComponent<EvidencePuzzle>(out var puzzle)){puzzle.Interact();puzzle.Submit(puzzle.correctIndex);}
                else{motor.Body.position=target.position;motor.transform.position=target.position;Physics2D.SyncTransforms();if(mission&&mission.rules[before].kind==MissionRuleKind.Reach)yield return new WaitForSeconds(.15f);else yield return new WaitForSeconds(5.8f);}
                yield return null;yield return new WaitForFixedUpdate();yield return null;
                Assert.IsTrue(world.Active!=active||level.Step!=before||(ending&&ending.Active),"Objective stalled: "+active.id+" step "+before+" "+level.Objective);
            }
            Time.timeScale=1;Assert.Fail("Story exceeded expected objective count.");
            }
            finally {Time.fixedDeltaTime=fixedStep;Time.timeScale=originalScale;}
        }
    }
}
