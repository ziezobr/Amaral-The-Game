using System.Collections;using NUnit.Framework;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class BossTests
    {
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;yield return SceneManager.LoadSceneAsync("00_Laboratorio");yield return null;}
        [UnityTest]public IEnumerator WarningWaitsBeforeDamageAndPausesDuringDialogue()
        {
            var p=Object.FindAnyObjectByType<PlayerInputReader>();var h=p.GetComponent<Health>();GroundWarning.Spawn(p.transform.position,2,.3f,20);
            yield return new WaitForSeconds(.1f);Assert.That(h.Current,Is.EqualTo(h.maximum));p.SetGameplayLocked(true);yield return new WaitForSeconds(.4f);Assert.That(h.Current,Is.EqualTo(h.maximum));
            p.SetGameplayLocked(false);yield return new WaitForSeconds(.3f);Assert.That(h.Current,Is.EqualTo(h.maximum-20));
        }
        [UnityTest]public IEnumerator WarningCannotDamageThroughWalls()
        {
            var p=Object.FindAnyObjectByType<PlayerInputReader>();var h=p.GetComponent<Health>();Vector2 at=p.transform.position;
            var wall=new GameObject("Wall");wall.layer=8;wall.transform.position=at+Vector2.right*.5f;wall.AddComponent<BoxCollider2D>().size=new Vector2(.1f,3);Physics2D.SyncTransforms();
            GroundWarning.Spawn(at+Vector2.right,2,.15f,30);yield return new WaitForSeconds(.3f);Assert.That(h.Current,Is.EqualTo(h.maximum));Object.Destroy(wall);
        }
        [UnityTest]public IEnumerator BossHasAbilitiesAndEntersSecondPhase()
        {
            yield return SceneManager.LoadSceneAsync("15_Cenoura");yield return null;var mission=Object.FindAnyObjectByType<CombatMission>();var boss=mission.rules[2].enemies[0];boss.gameObject.SetActive(true);yield return null;
            var ability=boss.GetComponent<BossAbilities>();Assert.That(ability,Is.Not.Null);Assert.That(ability.Enraged,Is.False);var hp=boss.GetComponent<Health>();hp.Damage(hp.maximum*2,CombatTeam.Player);Assert.That(ability.Enraged,Is.True);
        }
        [UnityTest]public IEnumerator BossDemoRestoresTheFightWithoutTouchingCampaignSave()
        {
            GameSession.EncounterDemo("15_Cenoura",2);yield return null;yield return null;
            var level=Object.FindAnyObjectByType<LevelProgress>();Assert.That(level.Step,Is.EqualTo(2));
            var player=Object.FindAnyObjectByType<PlayerMotor>();Assert.That(Physics2D.OverlapCircle(player.transform.position,.23f,LayerMask.GetMask("World")),Is.Null);
            foreach(var enemy in Object.FindAnyObjectByType<CombatMission>().rules[2].enemies)Assert.That(Vector2.Distance(player.transform.position,enemy.transform.position),Is.GreaterThan(2));
            Assert.That(Object.FindAnyObjectByType<BossAbilities>(),Is.Not.Null);Assert.That(GameSession.CanPersist,Is.False);
        }
        [UnityTest]public IEnumerator FlashStunCancelsTheBossWindupAndGroundWarnings()
        {
            GameSession.EncounterDemo("15_Cenoura",2);yield return null;yield return null;
            var ability=Object.FindAnyObjectByType<BossAbilities>();var boss=ability.GetComponent<EnemyAgent>();boss.speed=0;
            foreach(var ally in Object.FindObjectsByType<AllyAgent>())ally.gameObject.SetActive(false);
            foreach(var e in Object.FindObjectsByType<EnemyAgent>())if(e!=boss)e.gameObject.SetActive(false);
            var player=Object.FindAnyObjectByType<PlayerMotor>();player.Body.position=boss.transform.position+Vector3.down*3;
            float deadline=Time.time+5;while(!ability.Charging&&Time.time<deadline)yield return null;
            Assert.That(ability.Charging,Is.True);Assert.That(Object.FindObjectsByType<GroundWarning>().Length,Is.GreaterThan(0));
            boss.Stun(2);yield return null;yield return null;
            Assert.That(ability.Charging,Is.False);Assert.That(Object.FindObjectsByType<GroundWarning>().Length,Is.EqualTo(0));Assert.That(boss.enabled,Is.True);
        }
        [UnityTest]public IEnumerator ThirdMeleeHitIsAStrongerFinisher()
        {
            yield return SceneManager.LoadSceneAsync("04_Emboscada");yield return null;
            foreach(var ally in Object.FindObjectsByType<AllyAgent>())ally.gameObject.SetActive(false);
            var enemy=Object.FindAnyObjectByType<CombatMission>().rules[1].enemies[0];enemy.gameObject.SetActive(true);enemy.enabled=false;enemy.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var input=Object.FindAnyObjectByType<PlayerInputReader>();enemy.transform.position=input.transform.position+Vector3.right*.85f;var hp=enemy.GetComponent<Health>();hp.IncreaseMaximum(1000);
            input.Mobile(Vector2.zero,Vector2.right,false,false,false,false);yield return null;float before=hp.Current;
            for(int i=0;i<3;i++){input.Mobile(Vector2.zero,Vector2.right,false,false,false,false,true);yield return new WaitForSeconds(.45f);}
            Assert.That(before-hp.Current,Is.EqualTo(84*.88f).Within(.01f));
        }
        [UnityTest]public IEnumerator DefeatingABossDuringRecoveryDoesNotFreezeItsDeathAnimation()
        {
            GameSession.EncounterDemo("15_Cenoura",2);yield return null;yield return null;
            var ability=Object.FindAnyObjectByType<BossAbilities>();var enemy=ability.GetComponent<EnemyAgent>();enemy.speed=0;
            foreach(var ally in Object.FindObjectsByType<AllyAgent>())ally.gameObject.SetActive(false);
            foreach(var other in Object.FindObjectsByType<EnemyAgent>())if(other!=enemy)other.gameObject.SetActive(false);
            float deadline=Time.time+7;while(!ability.Recovering&&Time.time<deadline)yield return null;
            Assert.That(ability.Recovering,Is.True);enemy.GetComponent<Health>().Damage(10000,CombatTeam.Player);yield return null;yield return null;
            Assert.That(enemy.Alive,Is.False);Assert.That(enemy.enabled,Is.True);
        }
        [UnityTest]public IEnumerator RepeatedFlashCannotPermanentlyStunABoss()
        {
            GameSession.EncounterDemo("15_Cenoura",2);yield return null;yield return null;
            var enemy=Object.FindAnyObjectByType<BossAbilities>().GetComponent<EnemyAgent>();enemy.Stun(2);Assert.That(enemy.Stunned,Is.True);yield return new WaitForSeconds(.8f);
            Assert.That(enemy.Stunned,Is.False);enemy.Stun(2);Assert.That(enemy.Stunned,Is.False);Assert.That(enemy.FlashResistant,Is.True);
        }
        [UnityTest]public IEnumerator AttackSpriteMatchesTheLockedAimDirection()
        {
            var input=Object.FindAnyObjectByType<PlayerInputReader>();var sprites=input.GetComponent<DirectionalCombatSprites>();Assert.That(sprites.poses.Length,Is.EqualTo(32));
            input.Mobile(Vector2.zero,Vector2.right,false,false,false,false);yield return null;
            input.Mobile(Vector2.zero,Vector2.right,false,false,false,false,true);yield return new WaitForSeconds(.17f);
            Assert.That(sprites.body.sprite,Is.EqualTo(sprites.poses[6*4+2]));yield return new WaitForSeconds(.24f);Assert.That(sprites.body.transform.localScale.x,Is.EqualTo(1).Within(.015f));
        }
    }
}
