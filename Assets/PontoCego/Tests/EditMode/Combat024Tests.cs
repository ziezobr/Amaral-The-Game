using NUnit.Framework;
namespace PontoCego.Tests
{
    public sealed class Combat024Tests
    {
        [Test]public void ThirdComboStrikeHasHigherDamage(){Assert.That(CombatPlayer.ComboDamageMultiplier(2),Is.GreaterThan(CombatPlayer.ComboDamageMultiplier(1)));Assert.That(CombatPlayer.ComboDamageMultiplier(3),Is.GreaterThan(CombatPlayer.ComboDamageMultiplier(2)));}
        [Test]public void AutomaticFireAndMovementIncreaseSpreadButItIsBounded(){Assert.That(AmmoInventory.SpreadDegrees(1,5),Is.GreaterThan(AmmoInventory.SpreadDegrees(0,0)));Assert.That(AmmoInventory.SpreadDegrees(100,100),Is.EqualTo(6).Within(.01f));}
    }
}
