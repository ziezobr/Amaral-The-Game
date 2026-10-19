using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class CampaignFlowTests
    {
        // Exercises quest completion and scene transitions. Combat balance/playtime require human playtests.
        [UnityTest,Timeout(180000)]public IEnumerator CampaignCanProgressFromKidnappingToEnding()
        {
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("03_Casa");yield return null;
            int operations=0;
            while(operations++<230)
            {
                var ending=Object.FindAnyObjectByType<CampaignEnding>();if(ending&&ending.Active){Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("23_Amanhecer"));yield break;}
                var player=Object.FindAnyObjectByType<PlayerInputReader>();var hp=player.GetComponent<Health>();hp.IncreaseMaximum(10000);hp.Heal(10000);
                var dialogue=player.GetComponent<StoryDialogue>();while(dialogue.Active)dialogue.Advance();
                var level=Object.FindAnyObjectByType<LevelProgress>();var mission=Object.FindAnyObjectByType<CombatMission>();
                int before=level.Step;string scene=SceneManager.GetActiveScene().name;var target=level.CurrentTarget;Assert.That(target,Is.Not.Null,scene);
                var rule=mission.rules[before];
                if(rule.kind==MissionRuleKind.Fight)
                {
                    yield return null;foreach(var e in rule.enemies)if(e&&e.Alive)e.GetComponent<Health>().Damage(100000,CombatTeam.Player);
                }
                else if(rule.kind==MissionRuleKind.Reach){player.GetComponent<Rigidbody2D>().position=target.position;}
                else if(target.TryGetComponent<PhotoTarget>(out var photo))photo.Photograph();
                else if(target.TryGetComponent<StoryInteractable>(out var story)){story.Interact();while(dialogue&&dialogue.Active)dialogue.Advance();}
                else if(target.TryGetComponent<EquipmentShop>(out var shop)){shop.Interact();while(dialogue.Active)dialogue.Advance();if(shop.Active)shop.Close();}
                else if(target.TryGetComponent<EvidencePuzzle>(out var puzzle)){puzzle.Interact();puzzle.Submit(puzzle.correctIndex);}
                else
                {
                    player.GetComponent<Rigidbody2D>().position=target.position;yield return new WaitForSeconds(5.8f);
                }
                yield return null;yield return new WaitForFixedUpdate();yield return null;
                Assert.That(SceneManager.GetActiveScene().name!=scene||!level||level.Step!=before||(ending&&ending.Active),Is.True,"Quest stalled: "+scene+" step "+before);
            }
            Assert.Fail("Campaign exceeded expected quest operations.");
        }
    }
}
