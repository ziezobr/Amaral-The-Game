using NUnit.Framework;
using UnityEngine;
namespace PontoCego.Tests
{
    public sealed class UiAccessibilityTests
    {
        [Test]public void SmartHudKeepsCriticalInformationOpaque()
        {
            Assert.That(UiTheme.HudTarget(true,false,false,false),Is.EqualTo(.48f));
            Assert.That(UiTheme.HudTarget(true,true,false,false),Is.EqualTo(1));
            Assert.That(UiTheme.HudTarget(true,false,true,false),Is.EqualTo(1));
            Assert.That(UiTheme.HudTarget(true,false,false,true),Is.EqualTo(1));
            Assert.That(UiTheme.HudTarget(false,false,false,false),Is.EqualTo(1));
        }
        [Test]public void LifeAndStaminaHaveDifferentColorsInEveryPalette()
        {
            for(int mode=0;mode<4;mode++)
            {
                var life=UiTheme.LifeForMode(mode);var stamina=UiTheme.StaminaForMode(mode);
                Assert.That(Vector3.Distance(new Vector3(life.r,life.g,life.b),new Vector3(stamina.r,stamina.g,stamina.b)),Is.GreaterThan(.35f),"Palette "+mode);
            }
        }
        [Test]public void MenuAnimationUsesExplicitDeltaAndReducedMotionIsImmediate()
        {
            Assert.That(UiTheme.StepVisibility(0,true,.1f,false),Is.EqualTo(.5f).Within(.0001f));
            Assert.That(UiTheme.StepVisibility(.5f,false,.1f,false),Is.EqualTo(0).Within(.0001f));
            Assert.That(UiTheme.StepVisibility(0,true,0,true),Is.EqualTo(1));
            Assert.That(UiTheme.StepVisibility(1,false,0,true),Is.Zero);
        }
        [Test]public void TextScaleIsBoundedForFixedInterfaceColumns()
        {
            float old=GamePreferences.TextScale;
            try{GamePreferences.TextScale=4;Assert.That(UiTheme.Font(20),Is.EqualTo(26));GamePreferences.TextScale=.1f;Assert.That(UiTheme.Font(20),Is.EqualTo(18));}
            finally{GamePreferences.TextScale=old;}
        }
        [Test]public void EasingIsBoundedAndMonotonic()
        {float previous=0;for(int i=0;i<=100;i++){float current=UiTheme.MenuEase(i/100f);Assert.That(current,Is.InRange(previous,1));previous=current;}Assert.That(UiTheme.MenuEase(-1),Is.Zero);Assert.That(UiTheme.MenuEase(2),Is.EqualTo(1));}
        [Test]public void LargePanelsHaveSmallCornersWhileIconsAndButtonsKeepPills()
        {
            Assert.That(CleanGui.CornerRadius(new Rect(125,53,1040,626)),Is.EqualTo(14));
            Assert.That(CleanGui.CornerRadius(new Rect(145,138,194,516)),Is.EqualTo(14));
            Assert.That(CleanGui.CornerRadius(new Rect(0,0,170,49)),Is.EqualTo(24.5f));
            Assert.That(CleanGui.CornerRadius(new Rect(0,0,19,19)),Is.EqualTo(9.5f));
        }
    }
}
