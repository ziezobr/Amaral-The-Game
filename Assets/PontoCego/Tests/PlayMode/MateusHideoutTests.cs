using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class MateusHideoutTests
    {
        [UnitySetUp]public IEnumerator Setup(){GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;}
        [UnityTest]public IEnumerator HiddenMateusUsesFourCleanPosesAndFixedFeet()
        {
            var npc=Object.FindAnyObjectByType<Mateus>();Assert.That(npc,Is.Not.Null);Assert.That(Vector2.Distance(npc.transform.position,Mateus.HomePosition),Is.LessThan(.01f));
            foreach(var sprite in new[]{npc.idle,npc.side,npc.back,npc.fighting}){Assert.That(sprite,Is.Not.Null);Assert.That(sprite.name,Does.StartWith("Mateus HD"));Assert.That(sprite.bounds.size.y,Is.InRange(1.6f,2.21f));Assert.That(sprite.pivot.y,Is.EqualTo(0));}
            CircleCollider2D feet=null;foreach(var collider in npc.GetComponentsInChildren<CircleCollider2D>())if(!collider.isTrigger)feet=collider;Assert.That(feet,Is.Not.Null);Assert.That(feet.radius,Is.EqualTo(.28f));Assert.That(feet.transform.localPosition,Is.EqualTo(Vector3.zero));
            Assert.That(Mateus.HideoutBounds.Contains(Mateus.HomePosition),Is.True);Assert.That(WorldHub.Instance.bounds.Contains(Mateus.HomePosition),Is.True);yield return null;
        }
        [UnityTest]public IEnumerator HideoutPathAndStashRemainAccessible()
        {
            var player=Object.FindAnyObjectByType<PlayerMotor>();Physics2D.SyncTransforms();
            Assert.That(Physics2D.OverlapCircle(Mateus.ApproachPosition,.4f,LayerMask.GetMask("World")),Is.Null);
            Assert.That(Physics2D.OverlapCircle(Mateus.GlockStashPosition,.4f,LayerMask.GetMask("World")),Is.Null);
            var path=TaxiCollisionRoute.Plan(Mateus.TrailEntrance,Mateus.ApproachPosition,player.Body,.35f,.8f);Assert.That(path,Is.Not.Null);Assert.That(path.Length,Is.GreaterThan(1));Assert.That(Vector2.Distance(path[path.Length-1],Mateus.ApproachPosition),Is.LessThan(1));yield return null;
        }
        [UnityTest]public IEnumerator QuestNavigationUsesTheSameHideoutAndRewardCoordinates()
        {
            var save=GameSession.Current;Assert.That(GoldenWeaponQuest.NavigationTarget,Is.EqualTo(Mateus.ApproachPosition));save.collected.Add("gold:start:0");Assert.That(GoldenWeaponQuest.NavigationTarget,Is.EqualTo(GoldenWeaponQuest.Stops[0][0]));
            for(int step=0;step<3;step++)save.collected.Add("gold:0:"+step);Assert.That(GoldenWeaponQuest.NavigationTarget,Is.EqualTo(Mateus.ApproachPosition));save.collected.Add("gold:unlock:0");Assert.That(GoldenWeaponQuest.NavigationTarget,Is.EqualTo(Mateus.GlockStashPosition));
            save.collected.Add("gold:glock");Assert.That(GoldenWeaponQuest.NavigationTarget,Is.EqualTo(Mateus.ApproachPosition));Assert.That(GoldenWeaponQuest.RewardPosition(1),Is.EqualTo(Mateus.RazorStashPosition));yield return null;
        }
    }
}
