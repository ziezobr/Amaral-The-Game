using System.Linq;using UnityEngine;
namespace PontoCego
{
    /// <summary>Three finite city jobs with saved steps, physical targets and one-time payments.</summary>
    public sealed class CityContracts:MonoBehaviour
    {
        public static readonly string[] Titles={"Retratos do Mocambo","Encomenda de bairro","Lacre recuperado"};
        public static int Current{get{int i=0;while(i<3&&GameSession.Current.collected.Contains("city:done:"+i))i++;return i;}}
        public static int Step(int job){int i=0;while(GameSession.Current.collected.Contains("city:step:"+job+":"+i))i++;return i;}
        public static bool Started=>Current<3&&GameSession.Current.collected.Contains("city:start:"+Current);
        public static string Objective=>Current>=3?"Trabalhos de bairro concluídos.":!Started?"Celular > Mensagens: aceite um trabalho de bairro.":Titles[Current]+" • "+(Current==0?"Fotografe os registros do parque ("+Step(0)+"/3).":Current==1?Step(1)==0?"Retire a encomenda na oficina (E).":"Entregue a encomenda perto da casa de Amaral (E).":"Derrote os cobradores e recupere o lacre (E).");
        public Transform Target=>point?point.transform:null;GameObject point;int shown=-1;
        public void Accept(){if(Current>=3)return;GameSession.Current.collected.Add("city:start:"+Current);GameSession.Save();var gps=Object.FindAnyObjectByType<CityNavigation>();gps.FollowContract=true;gps.FollowMateus=gps.FollowWaypoint=false;}
        void Update()
        {
            if(!Started){if(point)Destroy(point);shown=-1;return;}int key=Current*10+Step(Current);if(shown==key&&point)return;if(point)Destroy(point);shown=key;
            var world=WorldHub.Instance;int job=Current,step=Step(job);var mission=world.Find(job==0?"24_Mocambo":job==1&&step==1?"03_Casa":job==1?"06_Oficina":"07_Distribuidora");
            Vector2 wanted=mission.entry+new Vector2(job==0?6+step*3:6,-4);point=new GameObject("Trabalho de bairro / "+Titles[job]);point.layer=LayerMask.NameToLayer("Interactable");point.transform.position=QuestPlacement2D.FindClear(wanted);point.transform.SetParent(transform,true);point.AddComponent<CircleCollider2D>().isTrigger=true;
            var task=point.AddComponent<CityContractPoint>();task.job=job;task.step=step;var visual=point.AddComponent<SpriteRenderer>();var quest=Object.FindAnyObjectByType<GoldenWeaponQuest>();visual.sprite=quest.parcel;visual.sortingOrder=WorldSort.At(point.transform.position.y);if(visual.sprite)point.transform.localScale=Vector3.one*(.7f/visual.sprite.bounds.size.y);
        }
        public static void Complete(int job,int step)
        {
            string id="city:step:"+job+":"+step;if(Current!=job||Step(job)!=step)return;GameSession.Current.collected.Add(id);
            int required=job==0?3:job==1?2:1;if(step+1>=required){GameSession.Current.collected.Add("city:done:"+job);GameSession.RewardOnce("city:pay:"+job,job==2?150:90);Object.FindAnyObjectByType<Interaction>()?.Say("Trabalho concluído. Pagamento recebido; há novos pedidos no celular.");}
            else Object.FindAnyObjectByType<Interaction>()?.Say("Etapa concluída. Próximo ponto marcado no mapa.");GameSession.Save();
        }
    }
    public sealed class CityContractPoint:MonoBehaviour,IInteractable
    {
        public int job,step;EnemyAgent[] guards;bool spawned;PlayerMotor player;
        public string Prompt=>job==0?"Fotografe este registro do Mocambo":"E • "+(job==2?"Recuperar lacre":step==0?"Retirar encomenda":"Entregar encomenda");
        void Update()
        {
            if(!player)player=Object.FindAnyObjectByType<PlayerMotor>();if(job!=2||spawned||!player||Vector2.Distance(player.transform.position,transform.position)>7)return;
            var source=WorldHub.Instance.missions.SelectMany(m=>m.template.GetComponentsInChildren<EnemyAgent>(true)).FirstOrDefault(e=>e.role==EnemyRole.Baton);if(!source)return;spawned=true;guards=new EnemyAgent[2];
            for(int i=0;i<2;i++){var at=QuestPlacement2D.FindClear((Vector2)transform.position+new Vector2(i==0?-2:2,1));var actor=Instantiate(source.gameObject,at,Quaternion.identity,transform.parent);guards[i]=actor.GetComponent<EnemyAgent>();guards[i].rewardId="city:guard:"+i;actor.SetActive(true);}
        }
        public void Interact(){if(job==0||!player||Vector2.Distance(player.transform.position,transform.position)>2.05f)return;if(job==2&&(!spawned||guards.Any(g=>g&&g.Alive))){Object.FindAnyObjectByType<Interaction>()?.Say("Derrote os dois cobradores antes de recuperar o lacre.");return;}CityContracts.Complete(job,step);}
        public bool Photograph(PlayerAim aim){if(job!=0)return false;var delta=(Vector2)transform.position-(Vector2)aim.transform.position;if(delta.magnitude>4.5f||Vector2.Dot(aim.Direction,delta.normalized)<.85f||Physics2D.Raycast(aim.transform.position,delta.normalized,delta.magnitude,LayerMask.GetMask("World")))return false;CityContracts.Complete(job,step);return true;}
        void OnGUI(){if(player&&Vector2.Distance(player.transform.position,transform.position)<7&&!player.GetComponent<PlayerInputReader>().GameplayLocked)CollectibleCover.Hint(transform.position,Prompt);}
    }
}
