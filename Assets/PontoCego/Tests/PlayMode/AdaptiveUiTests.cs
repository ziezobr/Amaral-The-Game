using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class AdaptiveUiTests
    {
        [UnitySetUp]public IEnumerator Setup(){GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;var player=Object.FindAnyObjectByType<PlayerMotor>();player.GetComponent<StoryDialogue>().DismissForValidation();player.GetComponent<PlayerInputReader>().SetPaused(false);player.GetComponent<PlayerInputReader>().SetGameplayLocked(false);}
        [TearDown]public void Cleanup(){Time.timeScale=1;}
        [UnityTest]public IEnumerator PhoneDoesNotUnlockAnExistingCutscene()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();var phone=player.GetComponent<PlayerPhone>();
            input.SetGameplayLocked(true);phone.Toggle();Assert.IsFalse(phone.Active);phone.Close();Assert.IsTrue(input.GameplayLocked);input.SetGameplayLocked(false);yield return null;
        }
        [UnityTest]public IEnumerator PhoneAnimationContinuesWhileSimulationIsPaused()
        {
            bool old=GamePreferences.ReduceMotion;GamePreferences.ReduceMotion=false;
            try
            {
                var player=Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();var phone=player.GetComponent<PlayerPhone>();phone.Toggle();Assert.IsTrue(phone.Active);input.SetPaused(true);
                yield return new WaitForSecondsRealtime(.35f);Assert.That(phone.AnimationVisibility,Is.EqualTo(1).Within(.01f));phone.Close();yield return new WaitForSecondsRealtime(.35f);Assert.That(phone.AnimationVisibility,Is.EqualTo(0).Within(.01f));input.SetPaused(false);
            }
            finally{GamePreferences.ReduceMotion=old;}
        }
        [UnityTest]public IEnumerator LowHealthKeepsHudFullyVisible()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();player.GetComponent<Health>().Restore(20);yield return new WaitForSecondsRealtime(.6f);
            var hud=Object.FindAnyObjectByType<GameHud>();Assert.That(hud.LifeFraction,Is.LessThan(.4f));Assert.That(hud.HudOpacity,Is.GreaterThan(.98f));
        }
        [UnityTest]public IEnumerator PauseHidesPhoneWithoutLosingAppOrGameplayLock()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();var phone=player.GetComponent<PlayerPhone>();
            phone.OpenApp(PhoneApp.Messages);yield return new WaitForSecondsRealtime(.3f);Assert.IsTrue(phone.Active);Assert.IsTrue(phone.Visible);
            input.SetPaused(true);yield return null;Assert.IsFalse(phone.Visible);Assert.IsTrue(phone.Active);Assert.IsTrue(input.GameplayLocked);
            input.SetPaused(false);yield return null;Assert.IsTrue(phone.Visible);Assert.That(phone.App,Is.EqualTo(PhoneApp.Messages));Assert.IsTrue(input.GameplayLocked);phone.Close();Assert.IsFalse(input.GameplayLocked);
        }
    }
}
