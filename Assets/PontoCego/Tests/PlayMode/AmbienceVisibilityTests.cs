using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PontoCego.Tests
{
    public sealed class AmbienceVisibilityTests
    {
        [Test] public void ClockMixHasCorrectEndpointsAndConstantPower()
        {
            Assert.That(WorldAmbience2D.MixForDaylight(1).x, Is.EqualTo(1).Within(.001f));
            Assert.That(WorldAmbience2D.MixForDaylight(1).y, Is.EqualTo(0).Within(.001f));
            Assert.That(WorldAmbience2D.MixForDaylight(0).x, Is.EqualTo(0).Within(.001f));
            Assert.That(WorldAmbience2D.MixForDaylight(0).y, Is.EqualTo(1).Within(.001f));
            for (float daylight = 0; daylight <= 1; daylight += .1f)
                Assert.That(WorldAmbience2D.MixForDaylight(daylight).sqrMagnitude, Is.EqualTo(1).Within(.001f));
        }

        [UnityTest] public IEnumerator CullingNeverDisablesPhysicsAndRestoresRendererState()
        {
            var cameraObject = new GameObject("QA / câmera de visibilidade");
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 5;
            var scenery = new GameObject("Paisagem / QA"); scenery.transform.position = new Vector2(100, 100);
            scenery.AddComponent<WorldStaticVisual2D>();
            var collider = scenery.AddComponent<BoxCollider2D>();
            var texture = new Texture2D(4, 4);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * .5f, 4);
            var renderer = scenery.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
            var control = new GameObject("QA / controlador de visibilidade").AddComponent<WorldVisibility2D>(); control.view = camera;
            Assert.That(control.Register(renderer), Is.True);
            control.EvaluateNow();
            Assert.That(renderer.forceRenderingOff, Is.True);
            Assert.That(renderer.enabled, Is.True);
            Assert.That(collider.enabled, Is.True);
            Assert.That(scenery.activeSelf, Is.True);
            camera.transform.position = new Vector3(100, 100, -10); control.EvaluateNow();
            Assert.That(renderer.forceRenderingOff, Is.False, "The visual must return before reaching the camera edge.");
            camera.transform.position = new Vector3(0, 0, -10); control.EvaluateNow();
            control.enabled = false;
            Assert.That(renderer.forceRenderingOff, Is.False, "Disabling the optimizer must restore the original state.");
            Assert.That(collider.enabled, Is.True);
            Object.Destroy(control.gameObject); Object.Destroy(scenery); Object.Destroy(cameraObject); Object.Destroy(sprite); Object.Destroy(texture);
            yield return null;
        }

        [UnityTest] public IEnumerator QuestAndCombatActorsAreExcludedFromStaticCulling()
        {
            var actor = new GameObject("Paisagem / ator não estático");
            actor.AddComponent<WorldStaticVisual2D>(); actor.AddComponent<Health>();
            var renderer = actor.AddComponent<SpriteRenderer>();
            Assert.That(WorldVisibility2D.Eligible(renderer), Is.False);
            Object.Destroy(actor); yield return null;
        }

        [UnityTest] public IEnumerator AmbienceBootsWithoutExtraListenerAndPausesWithSimulation()
        {
            GameSession.ExploreNeighborhood("01_Riberal"); yield return null; yield return null;
            var ambience = Object.FindAnyObjectByType<WorldAmbience2D>();
            Assert.That(ambience, Is.Not.Null);
            Assert.That(ambience.DaySource, Is.Not.Null);
            Assert.That(ambience.DaySource.loop, Is.True);
            Assert.That(ambience.NightSource.loop, Is.True);
            Assert.That(ambience.DaySource.spatialBlend, Is.Zero);
            Assert.That(ambience.SpatialEmitterCount, Is.LessThanOrEqualTo(8));
            Assert.That(ambience.GetComponentsInChildren<AudioListener>().Length, Is.Zero);
            var input = Object.FindAnyObjectByType<PlayerInputReader>(); input.SetPaused(true);
            yield return null; Assert.That(ambience.Paused, Is.True);
            input.SetPaused(false); yield return null; Assert.That(ambience.Paused, Is.False);
            float saved = GamePreferences.Effects; GamePreferences.Effects = 0;
            yield return null;
            Assert.That(ambience.DaySource.volume, Is.Zero);
            Assert.That(ambience.NightSource.volume, Is.Zero);
            GamePreferences.Effects = saved;
        }
    }
}
