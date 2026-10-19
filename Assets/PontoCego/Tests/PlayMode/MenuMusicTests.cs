using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class MenuMusicTests
    {
        [UnityTest]public IEnumerator MenuUsesSuppliedTrackAndHasOneListener()
        {Time.timeScale=1;yield return SceneManager.LoadSceneAsync("00_Menu");yield return null;var menu=Object.FindAnyObjectByType<MainMenu>();Assert.That(menu.menuMusic,Is.Not.Null);Assert.That(menu.menuMusic.name,Is.EqualTo("Menu_RidersOnTheStorm"));Assert.That(menu.menuMusic.length,Is.GreaterThan(10));var source=menu.GetComponent<AudioSource>();Assert.That(source.clip,Is.EqualTo(menu.menuMusic));Assert.IsTrue(source.loop);Assert.That(Object.FindObjectsByType<AudioListener>().Length,Is.EqualTo(1));}
    }
}
