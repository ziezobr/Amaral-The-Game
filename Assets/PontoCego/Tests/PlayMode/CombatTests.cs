using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class CombatTests
    {
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;yield return SceneManager.LoadSceneAsync("00_Laboratorio");yield return null;Object.FindAnyObjectByType<PlayerInputReader>().SetPaused(false);}
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;yield return null;}
        [UnityTest]public IEnumerator StaminaIsHiddenUntilUsedAndHidesAfterRecovery()
        {
            var hud=Object.FindAnyObjectByType<GameHud>();var input=Object.FindAnyObjectByType<PlayerInputReader>();
            yield return null;Assert.That(hud.StaminaOpacity,Is.LessThan(.01f));
            input.Mobile(Vector2.right,Vector2.up,true,false,false,false);for(int i=0;i<20;i++)yield return new WaitForFixedUpdate();
            Assert.That(hud.StaminaOpacity,Is.GreaterThan(.8f));
            input.Mobile(Vector2.zero,Vector2.up,false,false,false,false);yield return new WaitForSeconds(2.6f);
            Assert.That(hud.StaminaOpacity,Is.LessThan(.05f));
        }
        [UnityTest]public IEnumerator HealthFiltersFriendlyDamageAndDeathFiresOnce()
        {
            var go=new GameObject("Test health");var hp=go.AddComponent<Health>();hp.team=CombatTeam.Enemy;int deaths=0;hp.Died+=_=>deaths++;
            Assert.That(hp.Damage(20,CombatTeam.Enemy),Is.False);Assert.That(hp.Current,Is.EqualTo(100));
            Assert.That(hp.Damage(110,CombatTeam.Player),Is.True);Assert.That(hp.Alive,Is.False);
            hp.Damage(110,CombatTeam.Player);Assert.That(deaths,Is.EqualTo(1));Object.Destroy(go);yield return null;
        }
        [UnityTest]public IEnumerator BulletsStopAtWallsAndDamageWithoutWall()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();var hp=player.GetComponent<Health>();Vector2 at=player.transform.position;
            var wall=new GameObject("Bullet blocker");wall.layer=8;wall.transform.position=at+Vector2.left;wall.AddComponent<BoxCollider2D>().size=new Vector2(.12f,2);
            var sprite=player.GetComponent<DirectionalAnimation>().bodySprite.sprite;
            CombatProjectile.Fire(at+Vector2.left*2,Vector2.right,CombatTeam.Enemy,20,sprite);yield return new WaitForSeconds(.3f);
            Assert.That(hp.Current,Is.EqualTo(hp.maximum));Object.Destroy(wall);yield return new WaitForFixedUpdate();
            CombatProjectile.Fire(at+Vector2.left*2,Vector2.right,CombatTeam.Enemy,20,sprite);yield return new WaitForSeconds(.3f);
            Assert.That(hp.Current,Is.EqualTo(hp.maximum-20).Within(.01f));
        }
        [UnityTest]public IEnumerator PlayerMeleeDamagesAndStunsEnemy()
        {
            yield return SceneManager.LoadSceneAsync("04_Emboscada");yield return null;
            foreach(var ally in Object.FindObjectsByType<AllyAgent>())ally.gameObject.SetActive(false);
            var mission=Object.FindAnyObjectByType<CombatMission>();var enemy=mission.rules[1].enemies[0];enemy.gameObject.SetActive(true);
            var player=Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();input.SetPaused(false);
            enemy.transform.position=player.transform.position+Vector3.right*.85f;
            input.Mobile(Vector2.zero,Vector2.right,false,false,false,false);yield return null;float initial=enemy.GetComponent<Health>().Current;
            input.Mobile(Vector2.zero,Vector2.right,false,false,false,false,true);yield return new WaitForSeconds(.19f);
            Assert.That(enemy.GetComponent<Health>().Current,Is.LessThan(initial));Assert.That(enemy.Stunned,Is.True);
        }
        [UnityTest]public IEnumerator DeathLocksPlayerAndMedkitsHealOnlyWhenNeeded()
        {
            var input=Object.FindAnyObjectByType<PlayerInputReader>();var hp=input.GetComponent<Health>();var combat=input.GetComponent<CombatPlayer>();int initial=combat.Medkits;
            combat.UseMedkit();Assert.That(combat.Medkits,Is.EqualTo(initial));hp.Damage(40,CombatTeam.Enemy);combat.UseMedkit();
            Assert.That(hp.Current,Is.EqualTo(hp.maximum));Assert.That(combat.Medkits,Is.EqualTo(initial-1));
            yield return new WaitForSeconds(.6f);hp.Damage(500,CombatTeam.Enemy);Assert.That(input.GameplayLocked,Is.True);
        }
        // This legacy-template audit loads 25 source scenes; its wall-clock budget includes disk/asset loading.
        [UnityTest,Timeout(480000)]public IEnumerator EveryCampaignPhaseHasReachableObjectivesAndSafeSpawn()
        {
            int checkedScenes=0;
            for(int scene=0;scene<SceneManager.sceneCountInBuildSettings;scene++)
            {
                string path=SceneUtility.GetScenePathByBuildIndex(scene);if(path.EndsWith(WorldHub.SceneName+".unity"))continue; // Persistent world has its own connectivity tests.
                if(path.EndsWith("00_Menu.unity")||path.EndsWith("00_Laboratorio.unity"))continue;
                if(!path.StartsWith("Assets/PontoCego/Urban/Scenes/"))continue; // Unity Test Runner may inject a bootstrap scene.
                yield return SceneManager.LoadSceneAsync(path);yield return null;
                var input=Object.FindAnyObjectByType<PlayerInputReader>();Assert.That(input,Is.Not.Null,"Missing player in "+path+" (active "+SceneManager.GetActiveScene().name+")");input.SetPaused(false);input.SetGameplayLocked(true);
                var level=Object.FindAnyObjectByType<LevelProgress>();Assert.That(level,Is.Not.Null);Assert.That(level.objectives.Length,Is.GreaterThan(path.EndsWith("25_Bairro.unity")?0:1));
                Assert.That(Physics2D.OverlapCircle(input.transform.position,.20f,LayerMask.GetMask("World")),Is.Null,"Spawn intersects wall: "+path);
                var nav=Object.FindAnyObjectByType<GridNavigator>();if(nav&&level.objectiveTargets!=null)
                {
                    foreach(var target in level.objectiveTargets)
                    {
                        if(!target)continue;var route=nav.Path(input.transform.position,target.position);
                        float range=target.GetComponent<PhotoTarget>()?4.2f:1.5f;
                        bool reachable=Vector2.Distance(input.transform.position,target.position)<range||route.Count>0&&Vector2.Distance(route[route.Count-1],target.position)<range;
                        Assert.That(reachable,Is.True,"Unreachable: "+path+" / "+target.name);
                    }
                }
                checkedScenes++;
            }
            Assert.That(checkedScenes,Is.GreaterThanOrEqualTo(20));
        }
    }
}
