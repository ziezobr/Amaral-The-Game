using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public class Fluid023MotionTests
    {
        [UnityTest]public IEnumerator SecondaryMotionKeepsHierarchyAndSettlesWhenStopped()
        {
            var root=new GameObject("Motion QA");root.transform.position=new Vector3(1000,1000,0);var body=root.AddComponent<Rigidbody2D>();body.gravityScale=0;var child=new GameObject("Original animator binding");child.transform.SetParent(root.transform,false);var sr=child.AddComponent<SpriteRenderer>();var motion=root.AddComponent<ActorSecondaryMotion2D>();motion.visual=sr;
            try{yield return null;body.linearVelocity=Vector2.right*4;yield return new WaitForSeconds(.3f);Assert.That(child.transform.parent,Is.EqualTo(root.transform));Assert.That(child.transform.localScale.x,Is.InRange(.95f,1.05f));body.linearVelocity=Vector2.zero;yield return new WaitForSeconds(.4f);Assert.That(child.transform.localPosition.magnitude,Is.LessThan(.025f));}finally{Object.Destroy(root);}
        }
        [UnityTest]public IEnumerator ReducedMotionRestoresNeutralPoseWithoutScaleAccumulation()
        {
            bool old=GamePreferences.ReduceMotion;var root=new GameObject("Reduced motion QA");var body=root.AddComponent<Rigidbody2D>();body.gravityScale=0;var child=new GameObject("Visual");child.transform.SetParent(root.transform,false);var sr=child.AddComponent<SpriteRenderer>();root.AddComponent<ActorSecondaryMotion2D>().visual=sr;
            try{GamePreferences.ReduceMotion=true;yield return null;body.linearVelocity=Vector2.right*5;yield return new WaitForSeconds(.2f);Assert.That(child.transform.localPosition,Is.EqualTo(Vector3.zero));Assert.That(child.transform.localScale,Is.EqualTo(Vector3.one));}finally{GamePreferences.ReduceMotion=old;Object.Destroy(root);}
        }
    }
}
