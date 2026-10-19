using NUnit.Framework;
using UnityEngine;
namespace PontoCego.Tests
{
    public sealed class MateusTests
    {
        [Test]public void FirstCounterHasOnePercentChance(){Assert.That(Mateus.LethalChance(1),Is.EqualTo(.01f));}
        [Test]public void RepeatedAggressionEscalatesButIsCapped(){Assert.That(Mateus.LethalChance(5),Is.GreaterThan(Mateus.LethalChance(1)));Assert.That(Mateus.LethalChance(1000),Is.EqualTo(.5f));}
        [Test]public void AlliesCannotProvokeMateus(){var go=new GameObject();try{var npc=go.AddComponent<Mateus>();Assert.IsFalse(npc.ReceiveAttack(CombatTeam.Ally));Assert.That(npc.aggressions,Is.Zero);}finally{Object.DestroyImmediate(go);}}
    }
}
