using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace PontoCego.Tests
{
    public class Fluid023Tests
    {
        [Test]public void ExtraFramesDoNotSpeedUpGaitCycles(){float three=ReferenceActorAnimation.FramesPerSecond(4,false,3)/4;float five=ReferenceActorAnimation.FramesPerSecond(4,false,5)/5;Assert.That(five,Is.EqualTo(three).Within(.001f));Assert.That(five,Is.InRange(1,1.65f));}
        [Test]public void SprintHasHigherCadenceThanWalking(){Assert.That(ReferenceActorAnimation.FramesPerSecond(6,true,5),Is.GreaterThan(ReferenceActorAnimation.FramesPerSecond(4,false,5)));}
        [Test]public void DirectionHysteresisAvoidsBoundaryFlicker(){Vector2 At(float degrees)=>new Vector2(-Mathf.Sin(degrees*Mathf.Deg2Rad),-Mathf.Cos(degrees*Mathf.Deg2Rad));Assert.That(ReferenceActorAnimation.StableDirection(At(26),0),Is.Zero);Assert.That(ReferenceActorAnimation.StableDirection(At(33),0),Is.EqualTo(1));Assert.That(ReferenceActorAnimation.StableDirection(At(24),1),Is.EqualTo(1));}
        [Test]public void AtlasContainsFortyCompletePaddedPoses()
        {
            var poses=AssetDatabase.LoadAllAssetsAtPath("Assets/PontoCego/Urban/Fluid023.asset").OfType<Sprite>().ToArray();Assert.That(poses.Length,Is.EqualTo(40));
            foreach(var s in poses){Assert.That(s.rect.height,Is.GreaterThan(130),s.name);Assert.That(s.bounds.size.y,Is.InRange(2,2.3f),s.name);var t=s.texture;for(int x=0;x<t.width;x++){Assert.That(t.GetPixel(x,0).a,Is.Zero,s.name);Assert.That(t.GetPixel(x,t.height-1).a,Is.Zero,s.name);}for(int y=0;y<t.height;y++){Assert.That(t.GetPixel(0,y).a,Is.Zero,s.name);Assert.That(t.GetPixel(t.width-1,y).a,Is.Zero,s.name);}}
        }
    }
}
