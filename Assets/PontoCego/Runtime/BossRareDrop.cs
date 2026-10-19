using UnityEngine;
namespace PontoCego
{
    /// <summary>Only successful impacts occurring in the final phase count; each encounter drops once.</summary>
    public sealed class BossRareDrop:MonoBehaviour
    {
        public Sprite[] foodSprites;public int Hits{get;private set;}public bool Dropped{get;private set;}Health hp;BossAbilities boss;
        void Awake(){hp=GetComponent<Health>();boss=GetComponent<BossAbilities>();hp.Damaged+=Impact;}
        void Impact(Health health,float amount)
        {
            if(!boss)boss=GetComponent<BossAbilities>();if(Dropped||amount<=0||!boss)return;float before=Mathf.Min(health.maximum,health.Current+amount);int phase=Mathf.Clamp(1+Mathf.FloorToInt((1-before/health.maximum)*boss.PhaseCount),1,boss.PhaseCount);if(phase!=boss.PhaseCount)return;
            Hits++;if(Hits<5)return;Dropped=true;int kind=Random.Range(0,3);Vector2 point=transform.position;var player=FindAnyObjectByType<PlayerMotor>();if(player)point=Vector2.MoveTowards(point,player.transform.position,1.2f);if(Physics2D.OverlapCircle(point,.25f,LayerMask.GetMask("World")))point=transform.position;
            var go=new GameObject("Drop raro / "+(FoodKind)kind);go.layer=LayerMask.NameToLayer("Interactable");go.transform.position=point;go.AddComponent<CircleCollider2D>().isTrigger=true;var pickup=go.AddComponent<FoodPickup>();pickup.kind=(FoodKind)kind;var sr=go.AddComponent<SpriteRenderer>();if(foodSprites!=null&&foodSprites.Length>kind)sr.sprite=foodSprites[kind];sr.sortingOrder=WorldSort.At(point.y)+1;if(sr.sprite)go.transform.localScale=Vector3.one*(.7f/sr.sprite.bounds.size.y);
            FindAnyObjectByType<Interaction>()?.Say("O chefe derrubou um item raro!");
        }
        void OnDestroy(){if(hp)hp.Damaged-=Impact;}
    }
}
