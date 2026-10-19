using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class GroundContactTests
    {
        [UnityTest] public IEnumerator ProjectedCharacterShadowStaysAttachedToTheFootContact()
        {
            GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;yield return null;
            var player=Object.FindAnyObjectByType<PlayerMotor>();
            Transform contact=null,projection=null;
            foreach(var child in player.GetComponentsInChildren<Transform>()){if(child.name=="Sombra"||child.name=="Foot shadow"||child.name.StartsWith("Sombra nos pés"))contact=child;if(child.name=="Sombra projetada / silhueta")projection=child;}
            Assert.That(contact,Is.Not.Null);Assert.That(projection,Is.Not.Null);
            var renderer=projection.GetComponent<SpriteRenderer>();Assert.That(renderer.sprite,Is.Not.Null);
            var bounds=renderer.sprite.bounds;
            Vector3 projectedFoot=projection.TransformPoint(new Vector3(bounds.center.x,bounds.min.y,0));
            Assert.That(Vector2.Distance(projectedFoot,contact.position),Is.LessThan(.06f),"A detached shadow makes the character appear suspended above the ground.");
        }
        [UnityTest] public IEnumerator StreetBenchesHaveGroundedFeetAndVisibleLampFixtures()
        {
            GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;
            int benches=0,lamps=0;
            foreach(var root in Object.FindObjectsByType<Transform>())
            {
                if(root.name=="Mobiliário da rua / banco com pés")
                {
                    benches++;int feet=0;
                    foreach(var part in root.GetComponentsInChildren<SpriteRenderer>())if(part.name=="Banco / pé no chão")
                    {feet++;Assert.That(part.bounds.min.y,Is.LessThanOrEqualTo(root.position.y+.025f));}
                    Assert.That(feet,Is.EqualTo(2));Assert.That(root.GetComponent<BoxCollider2D>(),Is.Not.Null);
                }
                if(root.name=="Mobiliário da rua / poste com base")
                {
                    lamps++;var lamp=root.GetComponent<WorldLamp2D>();Assert.That(lamp,Is.Not.Null);
                    Assert.That(lamp.bulb.bounds.size.x,Is.GreaterThan(.20f));
                    Assert.That(lamp.bulb.bounds.size.y,Is.GreaterThan(.08f));
                }
            }
            Assert.That(benches,Is.GreaterThan(0));Assert.That(lamps,Is.EqualTo(benches));
        }
    }
}
