using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class WorkshopArtTests
    {
        [UnityTest]public IEnumerator WorkshopUsesNewArtWithinItsExistingFootprint()
        {
            Time.timeScale=1;GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;
            bool found=false;foreach(var state in Object.FindObjectsByType<InteriorFacade2D>())if(Vector2.Distance(state.footprint.center,new Vector2(60,0))<1)
            foreach(var art in state.exterior)if(art&&art.name.StartsWith("Fachada /"))
            {
                found=true;Assert.That(art.sprite.name,Is.EqualTo("Oficina Henzo / fachada HD v2"));
                Assert.That(art.bounds.min.y,Is.EqualTo(state.footprint.yMin+.35f).Within(.02f));
                Assert.That(art.bounds.size.x,Is.LessThan(state.footprint.width));Assert.That(art.bounds.size.y,Is.LessThan(state.footprint.height));
            }
            Assert.That(found,Is.True);
        }
    }
}
