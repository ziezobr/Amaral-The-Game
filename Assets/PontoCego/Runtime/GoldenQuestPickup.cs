using UnityEngine;
namespace PontoCego
{
    public enum GoldenTaskKind { Search, Photograph, Recover, Repair, Deliver, Reward }
    /// <summary>Distinct persistent errands: timed investigation, framed photo, encounter, repair and delivery.</summary>
    public sealed class GoldenQuestPickup:MonoBehaviour,IInteractable
    {
        public int chain,step;public bool EncounterReady=>encounterStarted&&guards!=null&&guards.Length==2;public int RemainingGuards{get{int n=0;if(guards!=null)foreach(var guard in guards)if(guard&&guard.Alive)n++;return n;}}
        public GoldenTaskKind Kind=>step==3?GoldenTaskKind.Reward:chain==0?(step==1?GoldenTaskKind.Photograph:step==2?GoldenTaskKind.Recover:GoldenTaskKind.Search):(step==1?GoldenTaskKind.Repair:step==2?GoldenTaskKind.Deliver:GoldenTaskKind.Search);
        public float WorkProgress=>Mathf.Clamp01(work/(Kind==GoldenTaskKind.Repair?3:1.6f));
        public string Prompt=>step==3?"E • "+(chain==0?"Glock dourada":"Navalha dourada"):Kind==GoldenTaskKind.Photograph?"Mire no lacre e fotografe com o flash":working?"Mantenha a posição • "+Mathf.RoundToInt(WorkProgress*100)+"%":"E • "+(Kind==GoldenTaskKind.Repair?"Reparar bancada":Kind==GoldenTaskKind.Deliver?"Entregar peças":Kind==GoldenTaskKind.Recover?"Recuperar estojo":"Investigar encomenda");
        PlayerMotor player;bool working,encounterStarted,completed;float work;Vector2 workPosition;EnemyAgent[] guards;
        bool Current=>GoldenWeaponQuest.Started(chain)&&(step==3?GoldenWeaponQuest.Step(chain)==3:GoldenWeaponQuest.Step(chain)==step);
        void Update()
        {
            if(!player)player=FindAnyObjectByType<PlayerMotor>();if(!player||!Current)return;
            var input=player.GetComponent<PlayerInputReader>();if(input.Paused||input.GameplayLocked)return;
            if(Kind==GoldenTaskKind.Recover&&!encounterStarted&&Vector2.Distance(player.transform.position,transform.position)<6)SpawnGuards();
            if(!working)return;
            if(Vector2.Distance(player.transform.position,transform.position)>2.05f||Vector2.Distance(player.transform.position,workPosition)>.45f){working=false;work=0;return;}
            work+=Time.deltaTime;if(WorkProgress>=1)Complete();
        }
        void SpawnGuards()
        {
            EnemyAgent source=null;var world=WorldHub.Instance;
            if(world)foreach(var mission in world.missions){if(!mission.actors)continue;foreach(var candidate in mission.actors.GetComponentsInChildren<EnemyAgent>(true))if(candidate.role==EnemyRole.Baton){source=candidate;break;}if(source)break;}
            if(!source){Say("O encontro ainda está sendo preparado. Tente novamente em um instante.");return;}encounterStarted=true;guards=new EnemyAgent[2];
            for(int i=0;i<guards.Length;i++)
            {
                Vector2 at=QuestPlacement2D.FindClear((Vector2)transform.position+new Vector2(i==0?-2:2,1));
                var actor=Instantiate(source.gameObject,at,Quaternion.identity,transform);actor.name="Cobrador / serviço do Mateus / "+i;guards[i]=actor.GetComponent<EnemyAgent>();guards[i].cashReward=0;guards[i].rewardId="gold:guard:"+chain+":"+step+":"+i;guards[i].startAlerted=true;actor.SetActive(true);
            }
            Say("Os cobradores guardam o estojo. Desvie dos golpes e use o flash para abrir espaço.");
        }
        bool GuardsAlive(){if(!EncounterReady)return true;foreach(var guard in guards)if(guard&&guard.Alive)return true;return false;}
        void OnGUI(){if(!player||!Current)return;var input=player.GetComponent<PlayerInputReader>();if(input.Paused||input.GameplayLocked)return;if(Vector2.Distance(player.transform.position,transform.position)<9)CollectibleCover.Hint(transform.position,(Kind==GoldenTaskKind.Recover&&RemainingGuards>0?"Cobradores restantes: "+RemainingGuards+" • ":"")+Prompt);}
        public void Interact()
        {
            if(!Current||completed)return;
            if(!player)player=FindAnyObjectByType<PlayerMotor>();if(!player||Vector2.Distance(player.transform.position,transform.position)>2.05f)return;
            if(Kind==GoldenTaskKind.Reward){if(!GameSession.Current.collected.Contains("gold:unlock:"+chain))return;Complete();return;}
            if(Kind==GoldenTaskKind.Photograph){Say("Aproxime-se, enquadre o lacre da caixa e use o flash. Não fotografe através da parede.");return;}
            if(Kind==GoldenTaskKind.Recover){if(!encounterStarted)SpawnGuards();if(GuardsAlive()){Say("Derrote os dois cobradores antes de recuperar o estojo.");return;}}
            if(!player)player=FindAnyObjectByType<PlayerMotor>();if(!player)return;working=true;workPosition=player.transform.position;work=0;Say(Kind==GoldenTaskKind.Repair?"Amaral: Vou apertar os contatos e alinhar a bancada. Preciso ficar parado por três segundos.":Kind==GoldenTaskKind.Deliver?"Amaral: Conferindo e entregando as peças de Mateus...":"Amaral: Vou conferir os detalhes da encomenda. Preciso ficar parado um instante.");
        }
        public bool TryPhotograph(PlayerAim aim)
        {
            if(!Current||completed||Kind!=GoldenTaskKind.Photograph||!aim)return false;Vector2 delta=transform.position-aim.transform.position;
            if(delta.magnitude>4.5f||delta.sqrMagnitude<.01f||Vector2.Dot(aim.Direction,delta.normalized)<.85f)return false;
            var wall=Physics2D.Raycast(aim.transform.position,delta.normalized,delta.magnitude,LayerMask.GetMask("World"));if(wall.collider&&!wall.collider.transform.IsChildOf(transform))return false;
            Complete();return true;
        }
        void Complete()
        {
            if(!Current||completed)return;completed=true;
            if(step==3){string id=chain==0?"gold:glock":"gold:razor";if(GameSession.Current.collected.Contains(id))return;GameSession.Current.collected.Add(id);Say(chain==0?"Ribeiro: Glock dourada! Munição infinita.":"Cristyan: Navalha dourada! Dano dobrado.");}
            else
            {
                GameSession.Current.collected.Add("gold:"+chain+":"+step);
                GameSession.RewardOnce("gold:service-payment:"+chain+":"+step,35);
                GameSession.AddEvidence("Serviço de Mateus • "+GoldenWeaponQuest.Jobs[chain][step]);
                Say(step==2?"Serviços concluídos. Volte a Mateus para receber a localização da arma.":"Serviço concluído • R$35. Próximo: "+GoldenWeaponQuest.Jobs[chain][step+1]);
            }
            GameSession.Save();Destroy(gameObject);
        }
        static void Say(string text)=>Object.FindAnyObjectByType<Interaction>()?.Say(text);
    }
}
