using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    /// <summary>Ground contact and clock-driven painted shadows preserve the fixed 2D projection.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class GroundedDepth2D : MonoBehaviour
    {
        Rigidbody2D body; SpriteRenderer shadow, visual, projected; Vector3 scale, origin;
        float nextDust; readonly FootDust[] dustPool = new FootDust[6]; int dustIndex;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= Apply; SceneManager.sceneLoaded += Apply;
            Apply(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        static void Apply(Scene scene, LoadSceneMode mode)
        {
            foreach (var body in Resources.FindObjectsOfTypeAll<Rigidbody2D>())
                if (body.gameObject.scene.IsValid() && (body.GetComponent<PlayerMotor>() || body.GetComponent<EnemyAgent>() || body.GetComponent<AllyAgent>()) && !body.GetComponent<GroundedDepth2D>())
                    body.gameObject.AddComponent<GroundedDepth2D>();
        }
        void Start()
        {
            body = GetComponent<Rigidbody2D>();
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>()) if (sr.name == "Sombra" || sr.name == "Foot shadow" || sr.name.StartsWith("Sombra nos pés")) { shadow = sr; break; }
            var character = GetComponent<DirectionalAnimation>(); var enemy = GetComponent<EnemyAgent>(); var ally = GetComponent<AllyAgent>();
            visual = character ? character.bodySprite : enemy ? enemy.visual : ally ? ally.visual : null;
            if (shadow) { scale = shadow.transform.localScale; origin = shadow.transform.localPosition; }
            if (visual)
            {
                var go = new GameObject("Sombra projetada / silhueta"); go.transform.SetParent(transform, false);
                projected = go.AddComponent<SpriteRenderer>(); projected.color = new Color(0, 0, 0, .2f);
            }
        }
        void LateUpdate()
        {
            if (!body) return;
            var clock = WorldEnvironment.DayNightCycle.Instance; var weather = WorldWeather2D.Instance;
            float daylight = clock ? clock.Daylight : 1, sun = weather ? weather.SunVisibility : 1;
            if (projected && visual && visual.sprite)
            {
                projected.sprite = visual.sprite; projected.flipX = visual.flipX;
                Vector2 direction = clock ? -clock.LightDirection2D : new Vector2(.6f, -.8f);
                // Moonlight keeps a small, readable contact shadow; sunrise stretches the silhouette.
                float stretch = Mathf.Lerp(.3f, .7f, clock ? clock.ShadowLength2D : .3f);
                // Position is solved from the sprite foot after rotation/scaling below; no detached silhouette offset.
                projected.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
                projected.transform.localScale = new Vector3(Mathf.Abs(visual.transform.localScale.x), stretch * Mathf.Abs(visual.transform.localScale.y), 1);
                Vector3 contact=shadow?shadow.transform.position:transform.position;
                var bounds=visual.sprite.bounds;
                Vector3 foot=new Vector3(bounds.center.x,bounds.min.y,0);
                projected.transform.position=contact-projected.transform.TransformVector(foot);
                projected.color = new Color(.025f, .04f, .075f, Mathf.Lerp(.1f, .23f * sun, daylight) * visual.color.a);
                projected.sortingOrder = WorldHub.Instance ? WorldSort.At(transform.position.y) - 4 : -2050;
                projected.enabled = visual.enabled;
            }
            if (!shadow) return;
            float speed = body.linearVelocity.magnitude;
            float pressure = GamePreferences.ReduceMotion ? 0 : Mathf.Abs(Mathf.Sin(Time.time * 10)) * Mathf.Clamp01(speed / 6);
            shadow.transform.localPosition = origin + new Vector3(0, -.025f, 0);
            if(WorldHub.Instance)shadow.sortingOrder=WorldSort.At(transform.position.y)-3;
            shadow.transform.localScale = new Vector3(scale.x * (1 + pressure * .12f), scale.y * (1 - pressure * .1f), scale.z);
            if (GamePreferences.ReduceMotion || speed < 4.7f || Time.time < nextDust || (weather && weather.Wetness > .3f)) return;
            nextDust = Time.time + .15f;
            var dust = dustPool[dustIndex];
            if (!dust)
            {
                var go = new GameObject("Poeira / pool de passos"); var sprite = go.AddComponent<SpriteRenderer>(); sprite.sprite = shadow.sprite;
                dust = go.AddComponent<FootDust>(); dust.sprite = sprite; dust.pooled = true; dustPool[dustIndex] = dust;
            }
            dustIndex = (dustIndex + 1) % dustPool.Length;
            dust.Show(transform.position, WorldHub.Instance ? WorldSort.At(transform.position.y) - 5 : -1900);
        }
        void OnDisable()
        {
            foreach (var dust in dustPool) if (dust) dust.gameObject.SetActive(false);
        }
        void OnDestroy()
        {
            foreach (var dust in dustPool) if (dust) Destroy(dust.gameObject);
        }
    }
    public sealed class FootDust : MonoBehaviour
    {
        public SpriteRenderer sprite; public bool pooled; float life = .3f;
        static Material material;
        void Start()
        {
            if (!material) { var shader = Resources.Load<Shader>("FootDustAlpha"); if (shader) material = new Material(shader); }
            if (material) sprite.sharedMaterial = material;
        }
        public void Show(Vector3 position, int order)
        {
            life = .3f; transform.position = position; transform.localScale = Vector3.one * .45f; sprite.sortingOrder = order;
            sprite.color = new Color(.74f, .65f, .50f, .18f); gameObject.SetActive(true);
        }
        void Update()
        {
            life -= Time.deltaTime; sprite.color = new Color(.74f, .65f, .50f, Mathf.Max(0, life) * .6f);
            transform.localScale += Vector3.one * Time.deltaTime * .7f;
            if (life > 0) return;
            if (pooled) gameObject.SetActive(false); else Destroy(gameObject);
        }
    }
}


