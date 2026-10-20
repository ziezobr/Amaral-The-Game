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
        public static readonly string[][] Jobs={new[]{"Investigue a caixa de correspondência da Riberal (E)","Fotografe o lacre adulterado na distribuidora (mire e use o flash)","Derrote os dois cobradores e recupere o estojo no ferro-velho (E)"},new[]{"Retire a ferramenta de precisão na oficina (E)","Conserte a bancada abandonada no acesso ao Mocambo (E)","Entregue as peças na caixa junto à casa de Amaral (E)"}};
        public static readonly string[][] Briefing={new[]{"A Glock não vai para qualquer um. Paulo usa encomendas falsas para mover dinheiro pela cidade.","Ache o comprovante da Riberal, fotografe o lacre da distribuidora e recupere meu estojo dos cobradores. Não basta só recolher caixas.","A rota fica no mapa. Volte para falar comigo depois dos três serviços."},new[]{"A navalha precisa de uma lâmina bem assentada. A bancada antiga do Mocambo ainda pode servir.","Pegue a ferramenta do Henzo, repare a bancada e entregue as peças perto da casa de Amaral.","Quando terminar, volte aqui. Eu te conto onde escondi a navalha dourada."}};
        public static string Status{get{int c=!Glock?0:1;return Glock&&Razor?"Mateus: armas douradas obtidas.":!Started(c)?"Mateus está escondido numa clareira no noroeste. Procure a trilha entre as moitas.":Step(c)<3?"Mateus ("+(Step(c)+1)+"/3): "+Jobs[c][Step(c)]:"Volte à clareira de Mateus para liberar a arma.";}}
        // Side errands stay outside campaign activation gates, beside the actual district entrances.
        public static Vector2 StopPosition(int chain,int step)
        {
            var world=WorldHub.Instance;if(!world)return Stops[chain][step];
            string[][] districts={new[]{"01_Riberal","07_Distribuidora","14_FerroVelho"},new[]{"06_Oficina","24_Mocambo","03_Casa"}};
            var mission=world.Find(districts[chain][step]);
            return QuestPlacement2D.FindClear(mission!=null?mission.entry+new Vector2(5.3f,-4):Stops[chain][step]);
        }
        public static Vector2 RewardPosition(int chain)=>chain==0?Mateus.GlockStashPosition:Mateus.RazorStashPosition;
        public static Vector2 NavigationTarget
        {
            get{int c=!Glock?0:1,step=Step(c);if(!Started(c)||step==3&&!GameSession.Current.collected.Contains("gold:unlock:"+c))return Mateus.ApproachPosition;var quest=Object.FindAnyObjectByType<GoldenWeaponQuest>();return quest&&quest.objective?(Vector2)quest.objective.transform.position:step<3?StopPosition(c,step):QuestPlacement2D.FindClear(RewardPosition(c));}
        }
        public void Talk()
        {
            var gps=FindAnyObjectByType<CityNavigation>();if(gps)gps.FollowMateus=true;
            int c=!Glock?0:1;if(Glock&&Razor){Say("Mateus: As duas armas são suas!");return;}
            if(!Started(c)){GameSession.Current.collected.Add("gold:start:"+c);GameSession.Save();var dialogue=FindAnyObjectByType<PlayerMotor>()?.GetComponent<StoryDialogue>();if(dialogue)dialogue.Begin("Mateus",null,Briefing[c]);}
            if(Step(c)<3){Say("Mateus: Faça três serviços. "+Jobs[c][Step(c)]+". Veja o celular em Mensagens.");return;}
            string unlock="gold:unlock:"+c;if(!GameSession.Current.collected.Contains(unlock)){GameSession.Current.collected.Add(unlock);GameSession.Save();}
            ShowReward(c);Say(c==0?"Mateus: Glock escondida na moita a oeste, ao meu lado.":"Mateus: Navalha escondida na extremidade leste da rodovia.");
        }
        void Update()
        {
            int c=!Glock?0:1;if(Glock&&Razor||!Started(c))return;
            int key=c*10+Step(c);if(shown==key)return;if(objective)Destroy(objective);shown=key;
            if(Step(c)<3)Create(StopPosition(c,Step(c)),c,Step(c),parcel,false);
            else if(GameSession.Current.collected.Contains("gold:unlock:"+c))ShowReward(c);
        }
        void ShowReward(int c){if(objective)Destroy(objective);Create(QuestPlacement2D.FindClear(RewardPosition(c)),c,3,c==0?glock:razor,true);}
        void Create(Vector2 point,int c,int step,Sprite icon,bool hide)
        {
            objective=new GameObject("Mateus / "+(step==3?"arma dourada":Jobs[c][step]));objective.layer=LayerMask.NameToLayer("Interactable");objective.transform.position=point;objective.transform.SetParent(transform,true);var trigger=objective.AddComponent<CircleCollider2D>();trigger.isTrigger=true;trigger.radius=.55f;var task=objective.AddComponent<GoldenQuestPickup>();task.chain=c;task.step=step;
            var itemVisual=new GameObject("Objeto do serviço");itemVisual.transform.SetParent(objective.transform,false);var sr=itemVisual.AddComponent<SpriteRenderer>();sr.sprite=icon;sr.sortingOrder=WorldSort.At(point.y)+1;
            if(icon)sr.transform.localScale=Vector3.one*(.85f/Mathf.Max(.01f,icon.bounds.size.y));
            if(hide){var go=new GameObject("Moita do esconderijo");go.transform.SetParent(objective.transform,false);var cover=go.AddComponent<SpriteRenderer>();cover.sprite=bush;var h=objective.AddComponent<CollectibleCover>();h.item=sr;h.cover=cover;}
        }
        static void Say(string text)=>Object.FindAnyObjectByType<Interaction>()?.Say(text);
    }
}
