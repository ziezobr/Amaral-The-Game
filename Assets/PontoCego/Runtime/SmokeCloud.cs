using UnityEngine;
namespace PontoCego
{
    public sealed class SmokeCloud:MonoBehaviour
    {
        public Sprite puff;public float duration=5,radius=3;
        SpriteRenderer[] particles;float started;
        void Start()
        {
            started=Time.time;particles=new SpriteRenderer[18];for(int i=0;i<particles.Length;i++)
            {
                var go=new GameObject("Fumaça");go.transform.SetParent(transform,false);go.transform.localPosition=(Vector3)(Random.insideUnitCircle*radius);
                go.transform.localScale=Vector3.one*Random.Range(2,4);var sr=go.AddComponent<SpriteRenderer>();sr.sprite=puff;sr.sortingOrder=3800;particles[i]=sr;
            }
        }
        void Update()
        {
            float t=(Time.time-started)/duration;if(t>1){Destroy(gameObject);return;}
            foreach(var p in particles){p.transform.position+=Vector3.up*Time.deltaTime*.2f;p.color=new Color(.8f,.86f,.9f,Mathf.Sin(Mathf.PI*t)*.65f);}
            foreach(var enemy in FindObjectsByType<EnemyAgent>())if(Vector2.Distance(enemy.transform.position,transform.position)<radius)enemy.Stun(.2f);
        }
    }
}
