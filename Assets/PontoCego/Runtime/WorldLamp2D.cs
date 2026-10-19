using UnityEngine;
namespace PontoCego
{
    /// <summary>Warm local contrast remains readable beneath the cool night grade.</summary>
    public sealed class WorldLamp2D : MonoBehaviour
    {
        public SpriteRenderer glow, bulb;
        void LateUpdate()
        {
            var clock = WorldEnvironment.DayNightCycle.Instance;
            var weather = WorldWeather2D.Instance;
            float night = clock ? clock.Night : 0;
            float overcast = weather ? weather.CloudCover * .15f : 0;
            float emission = Mathf.Clamp01(night + overcast);
            if (glow) glow.color = new Color(1, .76f, .40f, emission * .36f);
            if (bulb) bulb.color = Color.Lerp(new Color(.55f, .58f, .56f), new Color(1, .86f, .51f), emission);
        }
    }
}
