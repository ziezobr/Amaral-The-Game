using System.Collections;using NUnit.Framework;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.TestTools;using PontoCego.WorldEnvironment;
namespace PontoCego3D.Tests
{
 public sealed class EnvironmentTests
 {
  [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;yield return SceneManager.LoadSceneAsync("CentroAberto");yield return null;DayNightCycle.Instance.frozen=true;}
  [UnityTearDown]public IEnumerator Cleanup(){DayNightCycle.Instance.frozen=false;DayNightCycle.Instance.SetHour(16);Time.timeScale=1;yield return null;}
  [UnityTest]public IEnumerator NightActivatesLampsAndDayTurnsThemOff(){var clock=DayNightCycle.Instance;clock.SetHour(0);yield return null;var lights=Object.FindObjectsByType<NightLight>();Assert.That(lights.Length,Is.GreaterThan(20));bool any=false;foreach(var light in lights)if(light.lamp&&light.lamp.enabled&&light.lamp.intensity>1)any=true;Assert.That(any,Is.True);Assert.That(clock.Night,Is.GreaterThan(.99f));clock.SetHour(12);yield return null;foreach(var light in lights)if(light.lamp)Assert.That(light.lamp.enabled,Is.False);Assert.That(clock.Daylight,Is.GreaterThan(.99f));}
  [UnityTest]public IEnumerator ClockSurvivesSceneChangesAndWrapsMidnight(){var clock=DayNightCycle.Instance;clock.SetHour(25);Assert.That(clock.hour,Is.EqualTo(1));clock.SetHour(19.5f);yield return SceneManager.LoadSceneAsync("01_Riberal");yield return null;Assert.That(DayNightCycle.Instance,Is.SameAs(clock));Assert.That(clock.hour,Is.EqualTo(19.5f));Assert.That(Object.FindObjectsByType<DayNightCycle>().Length,Is.EqualTo(1));}
  [UnityTest]public IEnumerator CameraAndVehiclesUseRealThreeDimensionalGeometry(){var hero=Object.FindAnyObjectByType<ThirdPersonMotor>();Assert.That(hero.orbit.isometric,Is.True);Assert.That(Camera.main.orthographic,Is.True);var cars=Object.FindObjectsByType<CityVehicle>();Assert.That(cars.Length,Is.EqualTo(8));foreach(var car in cars){Assert.That(car.GetComponentsInChildren<MeshFilter>().Length,Is.GreaterThan(8));Assert.That(car.GetComponent<BoxCollider>(),Is.Not.Null);Assert.That(car.GetComponentsInChildren<SpriteRenderer>().Length,Is.Zero);}Assert.That(GameObject.Find("Quadra / piso terracota"),Is.Not.Null);yield return null;}
 }
}
