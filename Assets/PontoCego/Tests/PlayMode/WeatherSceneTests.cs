using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class WeatherSceneTests
    {
        [UnityTest] public IEnumerator WeatherPoolIsBoundedAndNeverAddsPhysicsObstacles()
        {
            Time.timeScale=1; GameSession.ExploreNeighborhood("01_Riberal"); yield return null; yield return new WaitForSecondsRealtime(.65f);
            var weather=WorldWeather2D.Instance; Assert.That(weather,Is.Not.Null); weather.automaticWeather=false;
            int colliders=Object.FindObjectsByType<Collider2D>().Length;
            weather.SetWeather(WeatherKind.Storm,true); yield return null;
            Assert.That(weather.GameplayActive,Is.True); Assert.That(weather.ParticleCapacity,Is.LessThanOrEqualTo(160));
            Assert.That(weather.PuddleCount,Is.InRange(1,32)); Assert.That(Object.FindObjectsByType<Collider2D>().Length,Is.EqualTo(colliders));
            int renderers=weather.GetComponentsInChildren<SpriteRenderer>(true).Length;
            yield return new WaitForSeconds(.25f); Assert.That(weather.GetComponentsInChildren<SpriteRenderer>(true).Length,Is.EqualTo(renderers));
            weather.SetWeather(WeatherKind.Clear,true); weather.automaticWeather=true;
        }
        [UnityTest] public IEnumerator StormOutsideRendersRainAbovePlayer()
        {
            Time.timeScale=1;GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return new WaitForSecondsRealtime(.65f);
            var weather=WorldWeather2D.Instance;Assert.That(weather,Is.Not.Null);
            var motor=Object.FindAnyObjectByType<PlayerMotor>();var input=motor.GetComponent<PlayerInputReader>();
            bool automatic=weather.automaticWeather,reduced=GamePreferences.ReduceMotion,locked=input.GameplayLocked;
            try
            {
                weather.automaticWeather=false;GamePreferences.ReduceMotion=false;input.SetPaused(false);input.SetGameplayLocked(true);
                motor.Body.position=new Vector2(-37,-20);motor.transform.position=motor.Body.position;motor.Body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
                Assert.That(weather.IsSheltered(motor.Body.position),Is.False);weather.SetWeather(WeatherKind.Storm,true);
                yield return new WaitForSecondsRealtime(.8f);yield return null;
                var camera=Camera.main;var actor=motor.GetComponent<DirectionalAnimation>().bodySprite;int visible=0;
                foreach(var drop in weather.GetComponentsInChildren<SpriteRenderer>())
                {
                    if(!drop.name.StartsWith("Chuva / ")||!drop.enabled)continue;
                    var point=camera.WorldToViewportPoint(drop.transform.position);
                    if(point.z<=0||point.x<0||point.x>1||point.y<0||point.y>1)continue;
                    visible++;Assert.That(drop.sortingOrder,Is.GreaterThan(actor.sortingOrder));
                    float worldWidth=drop.sprite.bounds.size.x*Mathf.Abs(drop.transform.lossyScale.x);
                    Assert.That(worldWidth,Is.GreaterThanOrEqualTo(.03f));Assert.That(drop.color.a,Is.GreaterThanOrEqualTo(.5f));
                    Assert.That(weather.IsSheltered(drop.transform.position),Is.False);
                }
                Assert.That(visible,Is.GreaterThanOrEqualTo(4),"Storm must draw legible drops inside the gameplay camera, rather than only outside its viewport.");
            }
            finally
            {
                weather.SetWeather(WeatherKind.Clear,true);weather.automaticWeather=automatic;GamePreferences.ReduceMotion=reduced;
                if(input)input.SetGameplayLocked(locked);
            }
        }
        [UnityTest] public IEnumerator WeatherKnowsRoofsAndStopsDrawingInTheMenu()
        {
            Time.timeScale=1; GameSession.ExploreNeighborhood("01_Riberal"); yield return null; yield return new WaitForSecondsRealtime(.65f);
            var weather=WorldWeather2D.Instance; Assert.That(weather,Is.Not.Null);
            Assert.That(weather.IsSheltered(new Vector2(-60,0)),Is.True); Assert.That(weather.IsSheltered(new Vector2(-37,-20)),Is.False);
            GameSession.ReturnToMenu(); yield return null; yield return null;
            Assert.That(weather.GameplayActive,Is.False);
        }
    }
}
