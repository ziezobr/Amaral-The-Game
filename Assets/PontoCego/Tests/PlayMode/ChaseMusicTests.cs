using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class ChaseMusicTests
    {
        [UnityTest]public IEnumerator UserTrackLoopsPausesAndFadesOutWithChase()
        {
            GameSession.StartSelectedMission("13_Estrada");yield return null;yield return null;var player=Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();input.SetPaused(false);player.GetComponent<StoryDialogue>().DismissForValidation();yield return null;
            var music=Object.FindAnyObjectByType<ChaseSoundtrack>();Assert.That(music,Is.Not.Null);Assert.That(music.clip.name,Is.EqualTo("Asphalt_Grip"));Assert.That(music.clip.length,Is.GreaterThan(10));Assert.IsTrue(music.Source.loop);Assert.That(music.Source.spatialBlend,Is.Zero);
            input.SetPaused(true);yield return new WaitForSecondsRealtime(.1f);Assert.IsTrue(music.Paused);input.SetPaused(false);yield return null;Assert.IsFalse(music.Paused);
            player.GetComponent<RoadDriver>().RestoreWalking();yield return new WaitForSeconds(1.2f);Assert.IsTrue(!music,"Music should finish fading after leaving the chase.");
        }
    }
}
