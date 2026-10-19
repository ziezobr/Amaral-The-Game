using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class MocamboTests
    {
        [UnityTest] public IEnumerator ParkHasReadableSignAndReachableMissionPoints()
        {
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("24_Mocambo");yield return null;
            var sign=GameObject.Find("Placa / PARQUE DO MOCAMBO");Assert.That(sign,Is.Not.Null);
            Assert.That(sign.GetComponentInChildren<TextMesh>().text,Is.EqualTo("PARQUE DO MOCAMBO"));
            Assert.That(sign.GetComponentInChildren<TextMesh>().GetComponent<Renderer>().bounds.size.x,Is.LessThanOrEqualTo(6.1f),"Sign text must fit the physical plaque");
            var level=Object.FindAnyObjectByType<LevelProgress>();Assert.That(level.objectives.Length,Is.EqualTo(7));
            var nav=Object.FindAnyObjectByType<GridNavigator>();
            for(int i=0;i<level.objectiveTargets.Length;i++)
            {
                Vector2 point=level.objectiveTargets[i].position;
                Assert.That(Physics2D.OverlapCircle(point,.23f,LayerMask.GetMask("World")),Is.Null,"Objective "+i);
                if(i>0)Assert.That(nav.Path(level.objectiveTargets[i-1].position,point).Count,Is.GreaterThan(0),"Route "+i);
            }
        }
        [UnityTest] public IEnumerator PivoThrowsBallsThenEntersSecondPhase()
        {
            GameSession.EncounterDemo("24_Mocambo",4);yield return null;yield return null;
            foreach(var ally in Object.FindObjectsByType<AllyAgent>())ally.gameObject.SetActive(false);
            var boss=Object.FindAnyObjectByType<BossAbilities>();Assert.That(boss.GetComponent<EnemyAgent>().role,Is.EqualTo(EnemyRole.Pivo));
            boss.GetComponent<EnemyAgent>().speed=0;float deadline=Time.time+6;
            while(Object.FindObjectsByType<BasketballShot>().Length==0&&Time.time<deadline)yield return null;
            Assert.That(Object.FindObjectsByType<BasketballShot>().Length,Is.GreaterThan(0));
            var hp=boss.GetComponent<Health>();hp.Damage(hp.maximum*.55f,CombatTeam.Player,true);Assert.That(boss.Phase,Is.EqualTo(2));
            var input=Object.FindAnyObjectByType<PlayerInputReader>();input.SetGameplayLocked(true);
            var ball=Object.FindAnyObjectByType<BasketballShot>();Vector3 at=ball.transform.position;yield return new WaitForSeconds(.2f);
            Assert.That(Vector3.Distance(at,ball.transform.position),Is.LessThan(.01f));input.SetGameplayLocked(false);
        }
    }
}
