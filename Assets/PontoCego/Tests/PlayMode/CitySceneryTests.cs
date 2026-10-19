using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class CitySceneryTests
    {
        [UnityTest]public IEnumerator CursorSupportsPhonePauseDeathAndReturnToMenu()
        {
            GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;
            var input=Object.FindAnyObjectByType<PlayerInputReader>();var phone=input.GetComponent<PlayerPhone>();
            input.SetPaused(true);Assert.That(input.CursorForInterface,Is.True);input.SetPaused(false);
            phone.Toggle();Assert.That(phone.Active,Is.True);Assert.That(input.CursorForInterface,Is.True);phone.Close();Assert.That(input.CursorForInterface,Is.False);
            yield return new WaitForSeconds(1.1f);input.GetComponent<Health>().Damage(1000000,CombatTeam.Enemy,true);Assert.That(input.CursorForInterface,Is.True);
            GameSession.ReturnToMenu();yield return null;yield return null;Assert.That(Cursor.visible,Is.True);
        }
        [UnityTest]public IEnumerator BuildingsKeepStreetsClearAndMapFootprintsMatch()
        {
            GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;
            var world=WorldHub.Instance;Assert.That(world.streetBlocks.Length,Is.GreaterThanOrEqualTo(12));
            int residents=0;foreach(var walker in Object.FindObjectsByType<StreetWalker>())if(walker.name.StartsWith("Morador /"))residents++;Assert.That(residents,Is.GreaterThanOrEqualTo(8));
            foreach(var block in world.streetBlocks)for(int i=0;i<world.roadStarts.Length;i++)
            {
                var a=world.roadStarts[i];var b=world.roadEnds[i];var corridor=Rect.MinMaxRect(Mathf.Min(a.x,b.x)-5,Mathf.Min(a.y,b.y)-5,Mathf.Max(a.x,b.x)+5,Mathf.Max(a.y,b.y)+5);
                Assert.That(block.Overlaps(corridor),Is.False,"Building overlaps road or sidewalk");
            }
        }
    }
}
