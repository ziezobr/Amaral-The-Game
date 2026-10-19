using NUnit.Framework;
namespace PontoCego.Tests
{
    public sealed class WeatherModelTests
    {
        [Test] public void RainAndWindTransitionWithoutAnAbruptJump()
        {
            var state = new WeatherState2D(); state.Set(WeatherKind.Clear, true); state.Set(WeatherKind.Storm);
            Assert.That(state.Rain, Is.Zero); state.Step(1, 1);
            Assert.That(state.Rain, Is.InRange(.1f, .2f)); Assert.That(state.Wind, Is.LessThan(.5f));
            state.Step(15, 1); Assert.That(state.Rain, Is.EqualTo(1).Within(.001f)); Assert.That(state.Wind, Is.InRange(.9f, 1));
        }
        [Test] public void PuddlesRemainAfterRainAndDryGradually()
        {
            var state = new WeatherState2D(); state.Set(WeatherKind.Storm, true); state.Step(30, 1);
            Assert.That(state.Wetness, Is.GreaterThan(.8f)); float before = state.Wetness;
            state.Set(WeatherKind.Clear, true); Assert.That(state.Wetness, Is.EqualTo(before));
            state.Step(5, 1); Assert.That(state.Wetness, Is.InRange(.5f, before));
            state.Step(400, 1); Assert.That(state.Wetness, Is.Zero);
        }
        [Test] public void ZeroOrInvalidTimeDoesNotCorruptWeather()
        {
            var state = new WeatherState2D(); state.Set(WeatherKind.Rain, true);
            float rain = state.Rain, wind = state.Wind;
            state.Step(0, 1); state.Step(-1, 1); state.Step(float.NaN, 1); state.Step(float.PositiveInfinity, 1);
            Assert.That(state.Rain, Is.EqualTo(rain)); Assert.That(state.Wind, Is.EqualTo(wind)); Assert.That(state.Wetness, Is.Zero);
        }
        [Test] public void ClearWeatherNeverMakesNegativeWetness()
        {
            var state = new WeatherState2D(); state.Set(WeatherKind.Clear, true);
            state.Step(100000, 1); Assert.That(state.Rain, Is.Zero); Assert.That(state.Wetness, Is.Zero);
        }
    }
}
