using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PontoCego.Tests
{
    public sealed class MotorTests
    {
        MovementSettings settings;
        [SetUp] public void Setup() {settings=ScriptableObject.CreateInstance<MovementSettings>();}
        [TearDown] public void Cleanup() {Object.DestroyImmediate(settings);}
        [Test] public void DiagonalHasSameSpeedAsCardinal()
        {
            var a=new MotorModel(settings);var b=new MotorModel(settings);
            for(int i=0;i<100;i++){a.Step(Vector2.right,false,false,.02f);b.Step(Vector2.one,false,false,.02f);}
            Assert.That(b.Velocity.magnitude,Is.EqualTo(a.Velocity.magnitude).Within(.0001f));
        }
        [Test] public void AnalogInputRetainsPartialMagnitude()
        {
            Assert.That(MotorModel.NormalizeInput(new Vector2(.3f,.4f)).magnitude,Is.EqualTo(.5f).Within(.0001f));
        }
        [Test] public void DodgeLocksDirectionAndRejectsRequestsDuringCooldown()
        {
            var m=new MotorModel(settings);m.Step(Vector2.right,false,true,.02f);
            m.Step(Vector2.left,false,true,.02f);
            Assert.That(m.Velocity.x,Is.GreaterThan(0));
            for(int i=0;i<11;i++)m.Step(Vector2.left,false,false,.02f);
            m.Step(Vector2.left,false,true,.02f);
            Assert.That(m.Dodging,Is.False);Assert.That(m.Cooldown,Is.GreaterThan(0));
            for(int i=0;i<60;i++)m.Step(Vector2.left,false,false,.02f);
            m.Step(Vector2.left,false,true,.02f);Assert.That(m.Dodging,Is.True);
        }
        [Test] public void DodgeDistanceIsStableAcrossFixedTimesteps()
        {
            float Integrate(float dt){var m=new MotorModel(settings);float distance=0;for(int i=0;i<Mathf.CeilToInt(settings.dodgeDuration/dt);i++){m.Step(Vector2.right,false,i==0,dt);distance+=m.Velocity.x*dt;}return distance;}
            Assert.That(Integrate(.02f),Is.EqualTo(Integrate(.016f)).Within(.001f));
            Assert.That(Integrate(.02f),Is.EqualTo(settings.dodgeDuration*settings.dodgeSpeed).Within(.001f));
        }
        [Test] public void SprintExhaustsThenRecovers()
        {
            var m=new MotorModel(settings);
            for(int i=0;i<151;i++)m.Step(Vector2.right,true,false,.02f);
            Assert.That(m.Sprinting,Is.False);Assert.That(m.Stamina,Is.LessThan(.1f));
            for(int i=0;i<250;i++)m.Step(Vector2.zero,false,false,.02f);
            Assert.That(m.Stamina,Is.EqualTo(settings.sprintCapacity).Within(.001f));
        }
        [Test] public void StopRemovesMomentum()
        {
            var m=new MotorModel(settings);m.Step(Vector2.up,true,true,.02f);m.Stop();
            Assert.That(m.Velocity,Is.EqualTo(Vector2.zero));Assert.That(m.Dodging,Is.False);
        }
        [Test] public void DefaultActionsSupportGamepadAndKeyboard()
        {
            var actions=PlayerInputReader.CreateDefaults();
            Assert.That(actions.FindAction("Move"),Is.Not.Null);Assert.That(actions.FindAction("Aim"),Is.Not.Null);
            Assert.That(actions["Move"].bindings.Count,Is.GreaterThanOrEqualTo(12));
            bool gamepad=false;foreach(var b in actions["Move"].bindings)gamepad|=b.path=="<Gamepad>/leftStick";
            Assert.That(gamepad,Is.True);Object.DestroyImmediate(actions);
        }
    }
}
