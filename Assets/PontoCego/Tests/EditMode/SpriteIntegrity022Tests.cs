using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace PontoCego.Tests
{
    public class SpriteIntegrity022Tests
    {
        const string Path="Assets/PontoCego/Urban/Polish022h.asset";
        [TestCase("amaral")][TestCase("ribeiro")][TestCase("cristyan")][TestCase("henzo")][TestCase("martins")][TestCase("paulo")][TestCase("kevyn")]
        public void LocomotionKeepsFullBodyAndConsistentScale(string actor)
        {
            var sprites=AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Sprite>().Where(s=>s.name.StartsWith(actor+" / ")).ToArray();
            Assert.That(sprites.Length,Is.GreaterThanOrEqualTo(32));
            float idle=sprites.First(s=>s.name==actor+" / idle alinhado / 0").bounds.size.y;
            foreach(var s in sprites){Assert.That(s.bounds.size.y,Is.InRange(idle*.73f,idle*1.22f),s.name);Assert.That(s.rect.width,Is.GreaterThan(20),s.name);}
        }
        [Test]public void DarkCapAndClothesAreRetained()
        {
            var sprite=AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Sprite>().First(s=>s.name=="ribeiro / idle alinhado / 0");
            var pixels=sprite.texture.GetPixels();int dark=pixels.Count(c=>c.a>.8f&&c.r<.25f&&c.g<.28f&&c.b<.35f);
            Assert.That(dark,Is.GreaterThan(900),"Cap/shirt matte must not erase dark clothing");
        }
        [Test]public void VehicleSpritesUseCenterPivotAndNoMissingViews()
        {
            var cars=AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Sprite>().Where(s=>s.name.StartsWith("022 Carro ")).ToArray();Assert.That(cars.Length,Is.EqualTo(32));
            foreach(var s in cars){Assert.That(s.pivot.y/s.rect.height,Is.EqualTo(.5f).Within(.001f));Assert.That(s.rect.width,Is.GreaterThan(100));Assert.That(s.rect.height,Is.GreaterThan(70));}
        }
    }
}
