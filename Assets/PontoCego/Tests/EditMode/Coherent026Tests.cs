using System.Linq;using NUnit.Framework;using UnityEditor;using UnityEngine;
namespace PontoCego.Tests
{
    public sealed class Coherent026Tests
    {
        [Test]public void EveryNewAtlasContainsCompleteIsolatedPaddedSprites()
        {
            var sprites=AssetDatabase.FindAssets("t:UrbanSpriteBank",new[]{"Assets/PontoCego/Urban/Animation026"}).SelectMany(id=>AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(id))).OfType<Sprite>().ToArray();Assert.AreEqual(680,sprites.Length);
            foreach(var sprite in sprites)
            {
                var texture=sprite.texture;Assert.Greater(sprite.rect.width,40,sprite.name);Assert.Greater(sprite.rect.height,100,sprite.name);
                for(int x=0;x<texture.width;x++){Assert.Zero(texture.GetPixel(x,0).a,sprite.name);Assert.Zero(texture.GetPixel(x,texture.height-1).a,sprite.name);}
                for(int y=0;y<texture.height;y++){Assert.Zero(texture.GetPixel(0,y).a,sprite.name);Assert.Zero(texture.GetPixel(texture.width-1,y).a,sprite.name);}
                if(sprite.name.StartsWith("vehicles"))Assert.AreEqual(.5f,sprite.pivot.y/sprite.rect.height,.001f,sprite.name);
                else Assert.That(sprite.bounds.size.y,Is.InRange(sprite.name.StartsWith("adryan")?1.3f:2.05f,sprite.name.StartsWith("adryan")?1.5f:2.3f),sprite.name);
            }
        }
    }
}
