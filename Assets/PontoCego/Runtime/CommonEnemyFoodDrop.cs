using UnityEngine;
namespace PontoCego
{
    /// <summary>Exactly one rare-food roll on a common enemy's death. Bosses are excluded even if misconfigured.</summary>
    public sealed class CommonEnemyFoodDrop:MonoBehaviour
    {
        public Sprite[] foodSprites;Health health;EnemyAgent enemy;bool rolled;
        public static FoodKind? FoodForRoll(float roll)=>roll<0||roll>=.08f?null:roll<.04f?FoodKind.Banana:roll<.07f?FoodKind.OrangeJuice:FoodKind.Steak;
        void Awake(){health=GetComponent<Health>();enemy=GetComponent<EnemyAgent>();if(health)health.Died+=OnDeath;}
        void OnDeath(Health source)
        {
            if(rolled)return;rolled=true;
            if(source.boss||!enemy||enemy.role==EnemyRole.Cenoura||enemy.role==EnemyRole.Paulo||enemy.role==EnemyRole.Pivo)return;
            var kind=FoodForRoll(Random.value);if(!kind.HasValue)return;
            Vector2 at=transform.position;var player=FindAnyObjectByType<PlayerMotor>();if(player)at=Vector2.MoveTowards(at,player.transform.position,.65f);
            if(Physics2D.OverlapCircle(at,.25f,LayerMask.GetMask("World")))at=transform.position;
            var go=new GameObject("Item raro / inimigo comum / "+kind.Value);go.layer=LayerMask.NameToLayer("Interactable");go.transform.position=at;
            var trigger=go.AddComponent<CircleCollider2D>();trigger.isTrigger=true;trigger.radius=.4f;go.AddComponent<FoodPickup>().kind=kind.Value;
            var child=new GameObject("Alimento");child.transform.SetParent(go.transform,false);var sr=child.AddComponent<SpriteRenderer>();
            if(foodSprites!=null&&foodSprites.Length>(int)kind.Value)sr.sprite=foodSprites[(int)kind.Value];sr.sortingOrder=WorldSort.At(at.y)+1;
            if(sr.sprite)child.transform.localScale=Vector3.one*(.75f/Mathf.Max(.01f,sr.sprite.bounds.size.y));
        }
        void OnDestroy(){if(health)health.Died-=OnDeath;}
    }
}
