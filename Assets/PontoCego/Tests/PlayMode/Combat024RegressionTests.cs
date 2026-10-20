using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class Combat024RegressionTests
    {
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;yield return SceneManager.LoadSceneAsync("00_Laboratorio");yield return null;var input=Object.FindAnyObjectByType<PlayerInputReader>();input.SetPaused(false);input.SetGameplayLocked(false);input.Mobile(Vector2.zero,Vector2.right,false,false,false,false);}
        [UnityTest]public IEnumerator BufferedStrikeChainsThroughAllThreeAttacks()
        {
            var combat=Object.FindAnyObjectByType<CombatPlayer>();combat.QueueAttack();yield return new WaitForSeconds(.26f);Assert.That(combat.ComboStep,Is.EqualTo(1));combat.QueueAttack();yield return new WaitForSeconds(.15f);Assert.That(combat.ComboStep,Is.EqualTo(2));
            yield return new WaitForSeconds(.4f);combat.QueueAttack();yield return null;yield return null;Assert.That(combat.ComboStep,Is.EqualTo(3));
        }
        [UnityTest]public IEnumerator ReloadTransfersOnlyAvailableRoundsAndShootingConsumesOne()
        {
            var gun=Object.FindAnyObjectByType<AmmoInventory>();var save=GameSession.Current;save.hasSidearm=true;save.weaponLevel=0;save.magazine=2;save.ammoReserve=3;
            gun.Reload();Assert.That(gun.Reloading,Is.True);yield return new WaitForSeconds(1.3f);Assert.That(save.magazine,Is.EqualTo(5));Assert.That(save.ammoReserve,Is.Zero);Assert.That(gun.Shoot(),Is.True);Assert.That(save.magazine,Is.EqualTo(4));Assert.That(gun.Shoot(),Is.False);Assert.That(save.magazine,Is.EqualTo(4));
        }
    }
}
