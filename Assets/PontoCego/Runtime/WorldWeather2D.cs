using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using PontoCego.WorldEnvironment;

namespace PontoCego
{
    public enum WeatherKind { Clear, Cloudy, Rain, Storm }

    /// <summary>Deterministic, allocation-free weather model. Wet ground dries gradually after rain.</summary>
    [Serializable]
    public sealed class WeatherState2D
    {
        public WeatherKind Kind { get; private set; }
        public float Rain { get; private set; }
        public float Wind { get; private set; } = .18f;
        public float Cloud { get; private set; }
        public float Wetness { get; private set; }

        public void Set(WeatherKind kind, bool immediate = false)
        {
            Kind = kind;
            if (!immediate) return;
            Rain = TargetRain(kind); Wind = TargetWind(kind); Cloud = TargetCloud(kind);
        }
        public void Step(float seconds, float daylight)
        {
            if (seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return;
            Rain = Mathf.MoveTowards(Rain, TargetRain(Kind), seconds * .16f);
            Wind = Mathf.MoveTowards(Wind, TargetWind(Kind), seconds * .10f);
            Cloud = Mathf.MoveTowards(Cloud, TargetCloud(Kind), seconds * .075f);
            float drying = Mathf.Lerp(.0015f, .006f, Mathf.Clamp01(daylight)) * (1 + Wind * .4f);
            Wetness = Mathf.Clamp01(Wetness + seconds * (Rain * .035f - drying * (1 - Rain)));
        }
        static float TargetRain(WeatherKind k) => k == WeatherKind.Storm ? 1 : k == WeatherKind.Rain ? .65f : 0;
        static float TargetWind(WeatherKind k) => k == WeatherKind.Storm ? .95f : k == WeatherKind.Rain ? .52f : k == WeatherKind.Cloudy ? .35f : .18f;
        static float TargetCloud(WeatherKind k) => k == WeatherKind.Storm ? .9f : k == WeatherKind.Rain ? .7f : k == WeatherKind.Cloudy ? .52f : .12f;
    }

    /// <summary>
    /// Shared 2D atmosphere. Fixed sprite pools cover only the camera; no physics, meshes or scene changes.
    /// The light shafts and reflections are painted 2D effects, not volumetric or PBR lighting.
    /// </summary>
    [DefaultExecutionOrder(-350)]
    public sealed class WorldWeather2D : MonoBehaviour
    {
        public static WorldWeather2D Instance { get; private set; }
        public WeatherKind Kind => state.Kind;
        public float Rain => state.Rain;
        public float Wind => state.Wind;
        public float Wetness => state.Wetness;
        public float CloudCover => state.Cloud;
        public float SunVisibility => 1 - CloudCover * .72f;
        public Vector2 WindVector => new Vector2(.8f, .24f) * Wind;
        public bool automaticWeather = true;
        public int ParticleCapacity => RainCount + CloudCount + FogCount + RayCount + FireflyCount;
        public int PuddleCount => puddles == null ? 0 : puddles.Length;
        public bool GameplayActive => sceneRoot && sceneRoot.activeSelf;
        public Transform PlayerTransform => player ? player.transform : null;
        public Camera ViewCamera => viewCamera;

        const int RainCount = 112, CloudCount = 6, FogCount = 6, RayCount = 5, FireflyCount = 14, PuddleCapacity = 32;
        readonly WeatherState2D state = new WeatherState2D();
        static readonly WeatherKind[] forecast = { WeatherKind.Cloudy, WeatherKind.Clear, WeatherKind.Cloudy, WeatherKind.Rain, WeatherKind.Clear, WeatherKind.Clear, WeatherKind.Storm, WeatherKind.Rain };
        static readonly Vector2[] lowlands = { new Vector2(-60, -55), new Vector2(-26, -68), new Vector2(78, -55) };
        Effect[] rain, clouds, fog, rays, fireflies, puddles;
        GameObject sceneRoot; Camera viewCamera; PlayerMotor player;
        Sprite softDisc, cloudSprite, rainSprite, raySprite, puddleSprite;
        readonly System.Collections.Generic.List<Texture2D> textures = new System.Collections.Generic.List<Texture2D>();
        readonly System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        int bucket = -1; float bindAt, decorateAt, outdoor = 1; bool dirty = true; Rect[] shelters = Array.Empty<Rect>(); Vector2[] greeneryPoints=Array.Empty<Vector2>();
        struct Effect { public SpriteRenderer renderer; public Vector2 uv, seed; public float phase; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (!Instance) new GameObject("Clima global / atmosfera 2D").AddComponent<WorldWeather2D>();
        }
        void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            state.Set(WeatherKind.Clear, true);
            SceneManager.sceneLoaded += Loaded;
        }
        void Loaded(Scene scene, LoadSceneMode mode)
        {
            dirty = true; bindAt = 0; decorateAt = 0; viewCamera = null; player = null;
            if (sceneRoot) sceneRoot.SetActive(false);
        }
        public void SetWeather(WeatherKind value, bool immediate = false)
        {
            if (!Enum.IsDefined(typeof(WeatherKind), value)) value = WeatherKind.Clear;
            state.Set(value, immediate);
            var clock = DayNightCycle.Instance;
            bucket = clock ? Mathf.FloorToInt(clock.hour / 3) : -1;
        }
        void Update()
        {
            if (Time.unscaledTime >= bindAt && (dirty || !player || !viewCamera))
            {
                bindAt = Time.unscaledTime + .5f;
                Bind();
            }
            if (!player || !viewCamera || !sceneRoot || !sceneRoot.activeSelf) return;
            var clock = DayNightCycle.Instance;
            float daylight = clock ? clock.Daylight : 1;
            if (automaticWeather && clock)
            {
                int current = Mathf.Clamp(Mathf.FloorToInt(clock.hour / 3), 0, forecast.Length - 1);
                if (current != bucket) { bucket = current; state.Set(forecast[current]); }
            }
            state.Step(Time.deltaTime, daylight);
            if (Time.unscaledTime >= decorateAt)
            {
                decorateAt = Time.unscaledTime + 6;
                DecorateVegetation();
            }
            RenderAtmosphere(clock, daylight);
        }
        void Bind()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == "00_Menu" || scene.name.StartsWith("3D", StringComparison.OrdinalIgnoreCase)) return;
            player = FindAnyObjectByType<PlayerMotor>(); viewCamera = Camera.main;
            if (!player || !viewCamera || !viewCamera.orthographic) return;
            if (sceneRoot) Destroy(sceneRoot);
            EnsureSprites();
            sceneRoot = new GameObject("Atmosfera visível / pools fixos");
            sceneRoot.transform.SetParent(transform, false);
            rain = Pool("Chuva", RainCount, rainSprite, 32740, 11);
            clouds = Pool("Sombra de nuvem", CloudCount, cloudSprite, 32710, 23);
            fog = Pool("Névoa baixa", FogCount, cloudSprite, 32715, 37);
            rays = Pool("Facho solar 2D", RayCount, raySprite, 32712, 41);
            fireflies = Pool("Vagalume", FireflyCount, softDisc, 32730, 59);
            BuildPuddles(); BuildShelters(); dirty = false;
            DecorateVegetation();
        }
        Effect[] Pool(string name, int count, Sprite sprite, int order, int seed)
        {
            var result = new Effect[count]; var random = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject(name + " / " + i); go.transform.SetParent(sceneRoot.transform, false);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = order; sr.enabled = false;
                result[i] = new Effect { renderer = sr, uv = new Vector2((float)random.NextDouble(), (float)random.NextDouble()), seed = new Vector2((float)random.NextDouble(), (float)random.NextDouble()), phase = (float)random.NextDouble() * 6.28f };
            }
            return result;
        }
        void BuildPuddles()
        {
            var world = WorldHub.Instance;
            int segments = world && world.roadStarts != null ? world.roadStarts.Length : 0;
            int count = Mathf.Min(PuddleCapacity, segments * 2);
            puddles = Pool("Poça / reflexo pintado 2D", count, puddleSprite, -8998, 79);
            for (int i = 0; i < count; i++)
            {
                int segment = Mathf.Min(segments-1,i*segments/Mathf.Max(1,count)); Vector2 a = world.roadStarts[segment], b = world.roadEnds[segment];
                Vector2 direction = (b - a).normalized, side = new Vector2(-direction.y, direction.x);
                var effect = puddles[i];
                effect.renderer.transform.position = Vector2.Lerp(a, b, .22f + effect.seed.x * .56f) + side * ((effect.seed.y - .5f) * 4.6f);
                effect.renderer.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                SetSize(effect.renderer, 1.1f + effect.seed.x * 1.3f, .30f + effect.seed.y * .32f);
            }
        }
        void BuildShelters()
        {
            var world = WorldHub.Instance;
            if (!world || world.districtMaps == null || world.districtRects == null) { shelters = Array.Empty<Rect>(); return; }
            var result = new System.Collections.Generic.List<Rect>();
            for (int i=0;i<Mathf.Min(world.districtMaps.Length,world.districtRects.Length);i++)
            {
                if (!world.districtMaps[i]) continue;
                string map = world.districtMaps[i].name.ToLowerInvariant();
                if (map.Contains("riberal") || map.Contains("gala") || map=="workshop" || map=="warehouse" || map=="home")
                    result.Add(world.districtRects[i]);
            }
            shelters = result.ToArray();
        }
        public bool IsSheltered(Vector2 point)
        {
            foreach (var shelter in shelters) if (shelter.Contains(point)) return true;
            return false;
        }
        void RenderAtmosphere(DayNightCycle clock, float daylight)
        {
            float height = viewCamera.orthographicSize * 2 + 6, width = height * viewCamera.aspect + 6;
            Vector2 center = viewCamera.transform.position;
            float now = Time.time, dt = Time.deltaTime, night = 1 - daylight;
            bool reduced = GamePreferences.ReduceMotion;
            outdoor = Mathf.MoveTowards(outdoor, IsSheltered(player.transform.position) ? 0 : 1, dt * 2);
            int visibleRain = Mathf.RoundToInt(RainCount * Rain * (reduced ? .22f : 1));
            float fall = reduced ? .12f : .68f;
            for (int i = 0; i < rain.Length; i++)
            {
                var e = rain[i]; bool visible = i < visibleRain; e.renderer.enabled = visible;
                if (!visible) continue;
                e.uv.y = Mathf.Repeat(e.uv.y - dt * fall, 1); e.uv.x = Mathf.Repeat(e.uv.x - dt * Wind * .12f, 1); rain[i] = e;
                e.renderer.transform.position = center + new Vector2((e.uv.x - .5f) * width, (e.uv.y - .5f) * height);
                e.renderer.enabled = !IsSheltered(e.renderer.transform.position) && outdoor > .01f;
                e.renderer.transform.rotation = Quaternion.Euler(0, 0, -15 - Wind * 18);
                SetSize(e.renderer, .038f, reduced ? .25f : .45f + e.seed.y * .3f);
                e.renderer.color = new Color(.76f, .87f, 1, Mathf.Lerp(.64f, .50f, daylight) * outdoor);
            }
            for (int i = 0; i < clouds.Length; i++)
            {
                var e = clouds[i]; e.renderer.enabled = CloudCover > .05f;
                float drift = reduced ? 0 : now * .002f * (1 + Wind);
                e.renderer.transform.position = center + new Vector2((Mathf.Repeat(e.uv.x + drift, 1) - .5f) * (width + 15), (e.uv.y - .5f) * height);
                SetSize(e.renderer, 12 + e.seed.x * 9, 5 + e.seed.y * 5);
                e.renderer.color = new Color(.10f, .15f, .24f, CloudCover * Mathf.Lerp(.035f, .09f, daylight) * outdoor);
            }
            float hour = clock ? clock.hour : 12;
            float dawn = Mathf.Clamp01(1 - Mathf.Abs(hour - 5) / 3.5f);
            float proximity = WorldHub.Instance ? LowlandProximity(center) : .35f;
            float fogDensity = (dawn * .11f + Rain * .025f) * proximity * outdoor;
            for (int i = 0; i < fog.Length; i++)
            {
                var e = fog[i]; e.renderer.enabled = fogDensity > .003f;
                float drift = reduced ? 0 : now * .003f;
                e.renderer.transform.position = center + new Vector2((Mathf.Repeat(e.uv.x + drift, 1) - .5f) * (width + 12), (e.uv.y - .5f) * height * .8f - 2);
                SetSize(e.renderer, 14 + e.seed.x * 8, 2.2f + e.seed.y * 2.5f);
                e.renderer.color = new Color(.65f, .76f, .84f, fogDensity);
            }
            Vector2 lightDirection = clock ? clock.LightDirection2D : new Vector2(-.6f, .8f);
            float rayAlpha = daylight * SunVisibility * (1 - Rain) * .032f * outdoor;
            for (int i = 0; i < rays.Length; i++)
            {
                var e = rays[i]; e.renderer.enabled = rayAlpha > .003f;
                float sway = reduced ? 0 : Mathf.Sin(now * .12f + e.phase) * .45f;
                e.renderer.transform.position = center + new Vector2((e.uv.x - .5f) * width + sway, height * .18f);
                e.renderer.transform.rotation = Quaternion.Euler(0, 0, 20 + lightDirection.x * 18);
                SetSize(e.renderer, .65f + e.seed.x * 1.1f, height * .9f);
                e.renderer.color = new Color(1, .91f, .64f, rayAlpha * (.65f + e.seed.y * .35f));
            }
            float greenery = WorldHub.Instance ? Mathf.Clamp01(1 - Vector2.Distance(center, lowlands[0]) / 40) : .4f;
            foreach(var green in greeneryPoints) greenery=Mathf.Max(greenery,Mathf.Clamp01(1-Vector2.Distance(center,green)/13));
            for (int i = 0; i < fireflies.Length; i++)
            {
                var e = fireflies[i]; e.renderer.enabled = night * greenery > .05f;
                float sway = reduced ? 0 : Mathf.Sin(now * .45f + e.phase) * .4f;
                e.renderer.transform.position = center + new Vector2((e.uv.x - .5f) * width + sway, (e.uv.y - .5f) * height * .65f + sway * .5f);
                e.renderer.transform.localScale = Vector3.one * (.065f + e.seed.x * .07f);
                float pulse = reduced ? .65f : .6f + Mathf.Sin(now * .7f + e.phase) * .25f;
                e.renderer.color = new Color(.74f, 1, .39f, night * greenery * pulse * .6f * outdoor);
            }
            for (int i = 0; i < puddles.Length; i++)
            {
                var sr = puddles[i].renderer; sr.enabled = Wetness > .04f;
                if (!sr.enabled) continue;
                float distance = ((Vector2)sr.transform.position - center).sqrMagnitude;
                sr.enabled = distance < (width + height) * (width + height) * .28f;
                sr.color = Color.Lerp(new Color(.28f, .40f, .58f, Wetness * .48f), new Color(.67f, .78f, .83f, Wetness * .45f), daylight);
            }
        }
        static void SetSize(SpriteRenderer renderer, float width, float height)
        {
            var size = renderer.sprite.bounds.size;
            renderer.transform.localScale = new Vector3(width / Mathf.Max(.001f,size.x), height / Mathf.Max(.001f,size.y), 1);
        }
        static float LowlandProximity(Vector2 center)
        {
            float proximity = .12f;
            foreach (var low in lowlands) proximity = Mathf.Max(proximity, Mathf.Clamp01(1 - Vector2.Distance(center, low) / 30));
            return proximity;
        }
        void DecorateVegetation()
        {
            var greenery=new System.Collections.Generic.List<Vector2>();
            foreach (var sr in FindObjectsByType<SpriteRenderer>())
            {
                if (!sr.sprite) continue;
                var component=sr.GetComponent<WorldVegetation2D>();
                string label = sr.name.ToLowerInvariant();
                bool plant=component || label.Contains("moita") || label.Contains("jardineira") || label.Contains("árvore") || label.Contains("arvore") || label.Contains("capim") || label.Contains("vegetação") || label.Contains("vegetacao");
                if(!plant)continue;
                if(!component)sr.gameObject.AddComponent<WorldVegetation2D>();
                if(greenery.Count<256)greenery.Add(sr.transform.position);
            }
            greeneryPoints=greenery.ToArray();
        }
        void EnsureSprites()
        {
            if (softDisc) return;
            softDisc = MakeSprite("Halo suave / 2D", 48, 48, (u, v) => new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - new Vector2(u, v).magnitude), 2)));
            cloudSprite = MakeSprite("Nuvem macia / 2D", 96, 64, (u, v) =>
            {
                float d = new Vector2(u, v).magnitude;
                float shape = 1 + .11f * Mathf.Sin(u * 9) * Mathf.Cos(v * 5);
                return new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d / shape), 1.5f));
            });
            rainSprite = MakeSprite("Gota / 2D", 4, 32, (u, v) => new Color(1, 1, 1, Mathf.Pow(1 - Mathf.Abs(u), .55f) * (1 - Mathf.Abs(v))));
            raySprite = MakeSprite("Feixe pintado / 2D", 32, 128, (u, v) =>
            {
                float width = Mathf.Lerp(.95f, .25f, (v + 1) * .5f);
                return new Color(1, 1, 1, Mathf.Clamp01(1 - Mathf.Abs(u) / width) * Mathf.Clamp01(1 - Mathf.Abs(v)));
            });
            puddleSprite = MakeSprite("Reflexo de poça / 2D", 96, 48, (u, v) =>
            {
                float edge = 1 - new Vector2(u, v).magnitude + .09f * Mathf.Sin(u * 15) * Mathf.Cos(v * 11);
                float reflection = Mathf.Abs(v + .17f + Mathf.Sin(u * 6) * .1f) < .06f ? .95f : .58f;
                return new Color(reflection, reflection, reflection, Mathf.SmoothStep(0, .2f, edge));
            });
        }
        Sprite MakeSprite(string name, int width, int height, Func<float, float, Color> pixel)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) pixels[y * width + x] = pixel((x + .5f) / width * 2 - 1, (y + .5f) / height * 2 - 1);
            texture.SetPixels(pixels); texture.Apply(false, true); textures.Add(texture);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * .5f, width, 0, SpriteMeshType.FullRect); sprite.name = name; sprites.Add(sprite); return sprite;
        }
        void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null; SceneManager.sceneLoaded -= Loaded;
            foreach (var sprite in sprites) if (sprite) Destroy(sprite);
            foreach (var texture in textures) if (texture) Destroy(texture);
        }
    }

}
