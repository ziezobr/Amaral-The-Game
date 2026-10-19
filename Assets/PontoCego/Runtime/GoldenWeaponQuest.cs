using UnityEngine;
namespace PontoCego
{
    /// <summary>Persistent three-errand chains. Rewards require all errands and returning to Mateus.</summary>
    public sealed class GoldenWeaponQuest:MonoBehaviour
    {
        public Sprite parcel,glock,razor,bush;GameObject objective;int shown=-1;
        public static bool Glock=>GameSession.Current.collected.Contains("gold:glock");
        public static bool Razor=>GameSession.Current.collected.Contains("gold:razor");
        public static bool Started(int c)=>GameSession.Current.collected.Contains("gold:start:"+c);
        public static int Step(int c){int i=0;while(i<3&&GameSession.Current.collected.Contains("gold:"+c+":"+i))i++;return i;}
        public static readonly Vector2[][] Stops={new[]{new Vector2(-60,-17),new Vector2(60,30),new Vector2(0,33)},new[]{new Vector2(60,-22),new Vector2(-52,-74),new Vector2(0,-76)}};
        public static readonly string[][] Jobs={new[]{"Retire a encomenda junto à Riberal","Busque o carregador na distribuidora","Recupere o estojo no acesso ao ferro-velho"},new[]{"Busque a ferramenta na oficina","Recupere a lâmina no acesso ao Mocambo","Busque o cabo perto da casa de Amaral"}};
        public static string Status{get{int c=!Glock?0:1;return Glock&&Razor?"Mateus: armas douradas obtidas.":!Started(c)?"Mateus está escondido numa clareira no noroeste. Procure a trilha entre as moitas.":Step(c)<3?"Mateus ("+(Step(c)+1)+"/3): "+Jobs[c][Step(c)]:"Volte à clareira de Mateus para liberar a arma.";}}
        public static Vector2 RewardPosition(int chain)=>chain==0?Mateus.GlockStashPosition:Mateus.RazorStashPosition;
        public static Vector2 NavigationTarget
        {
            get{int c=!Glock?0:1,step=Step(c);if(!Started(c)||step==3&&!GameSession.Current.collected.Contains("gold:unlock:"+c))return Mateus.ApproachPosition;return step<3?Stops[c][step]:RewardPosition(c);}
        }
        public void Talk()
        {
            var gps=FindAnyObjectByType<CityNavigation>();if(gps)gps.FollowMateus=true;
            int c=!Glock?0:1;if(Glock&&Razor){Say("Mateus: As duas armas são suas!");return;}
            if(!Started(c)){GameSession.Current.collected.Add("gold:start:"+c);GameSession.Save();}
            if(Step(c)<3){Say("Mateus: Faça três serviços. "+Jobs[c][Step(c)]+". Veja o celular em Mensagens.");return;}
            string unlock="gold:unlock:"+c;if(!GameSession.Current.collected.Contains(unlock)){GameSession.Current.collected.Add(unlock);GameSession.Save();}
            ShowReward(c);Say(c==0?"Mateus: Glock escondida na moita a oeste, ao meu lado.":"Mateus: Navalha escondida na extremidade leste da rodovia.");
        }
        void Update()
        {
            int c=!Glock?0:1;if(Glock&&Razor||!Started(c))return;
            int key=c*10+Step(c);if(shown==key)return;if(objective)Destroy(objective);shown=key;
            if(Step(c)<3)Create(Stops[c][Step(c)],c,Step(c),parcel,false);
            else if(GameSession.Current.collected.Contains("gold:unlock:"+c))ShowReward(c);
        }
        void ShowReward(int c){if(objective)Destroy(objective);Create(RewardPosition(c),c,3,c==0?glock:razor,true);}
        void Create(Vector2 point,int c,int step,Sprite icon,bool hide)
        {
            objective=new GameObject("Mateus / "+(step==3?"arma dourada":Jobs[c][step]));objective.layer=LayerMask.NameToLayer("Interactable");objective.transform.position=point;objective.transform.SetParent(transform,true);objective.AddComponent<CircleCollider2D>().isTrigger=true;var task=objective.AddComponent<GoldenQuestPickup>();task.chain=c;task.step=step;
            var sr=objective.AddComponent<SpriteRenderer>();sr.sprite=icon;sr.sortingOrder=WorldSort.At(point.y)+1;
            if(hide){var go=new GameObject("Moita do esconderijo");go.transform.SetParent(objective.transform,false);var cover=go.AddComponent<SpriteRenderer>();cover.sprite=bush;var h=objective.AddComponent<CollectibleCover>();h.item=sr;h.cover=cover;}
        }
        static void Say(string text)=>Object.FindAnyObjectByType<Interaction>()?.Say(text);
    }
}
