using UnityEngine;using UnityEngine.InputSystem;
namespace PontoCego3D
{
    public sealed class OpenDistrict:MonoBehaviour
    {
        public PontoCego.GameUiArt ui;public int currency=150,flashCharges=12;bool phone;PontoCego.PhoneApp phoneApp;public ThirdPersonMotor player;public Transform[] goals;public string[] objectives;
        public int Step{get;private set;}public int Kills{get;private set;}public int kits=3;
        public float FlashAt{get;set;}=-10;public string Message{get;private set;}="";public float MessageUntil{get;private set;}
        public Vector3 spawn;public bool Capturing;float staminaAlpha;Vector2 scroll;bool bindings;
        GUIStyle title,text,small,button;public GameObject[] encounter;public string[] names={"Dona Lúcia","João","Rafael","Ana","Marina","Carlos"};
        public Transform Goal=>goals!=null&&Step<goals.Length?goals[Step]:null;
        public string Objective=>Step<objectives.Length?objectives[Step]:"Bairro liberado. Explore Patos de Minas.";
        void Start(){Feedback("Amaral: mais um dia na Riberal. O Adryan está contando comigo. Preciso bater o ponto.",9);}
        public void Feedback(string message,float duration=5){Message=message;MessageUntil=Time.time+duration;}
        void Update()
        {
            if(UnityEngine.InputSystem.Keyboard.current!=null&&UnityEngine.InputSystem.Keyboard.current.tKey.wasPressedThisFrame){if(phone)ClosePhone();else OpenPhone(PontoCego.PhoneApp.Home);}
            if(phone)return;
            if(!player)return;var input=player.GetComponent<ThirdPersonInput>();if(input.Paused)return;
            float target=player.Stamina<player.staminaCapacity-.02f?1:0;staminaAlpha=Mathf.MoveTowards(staminaAlpha,target,Time.deltaTime*3);
            if(!input.Down("Interact")||!player.GetComponent<CombatActor>().Alive)return;
            if(Goal&&Vector3.Distance(player.transform.position,Goal.position)<3)
            {
                if(Step==0){Advance("Paulo: Você cobre a gala esta noite. Antes, busca a lente com o Ribeiro na padaria.");}
                else if(Step==1)Feedback("Mire na fachada da padaria e use o flash para testar a lente.");
                else if(Step==2){Advance("Martins: Tem homens estranhos no pátio da distribuidora. Toma cuidado, Amaral.");foreach(var e in encounter)e.SetActive(true);}
                else if(Step==3)Feedback("Os capangas fecharam o pátio. Flash abre a guarda; clique dá o golpe. Controle a distância.");
                else if(Step==4){Advance("Amaral: A rota está no recibo. Vou fotografar tudo na gala. O Adryan não precisa saber dessa confusão ainda.");}
                return;
            }
            foreach(var civilian in FindObjectsByType<StreetPedestrian>())
            {
                if(civilian.hostile||Vector3.Distance(player.transform.position,civilian.transform.position)>2.5f)continue;
                Feedback(civilian.GetComponent<CombatActor>().displayName+": "+(civilian.Fleeing?"Sai de perto! Estão brigando ali!":"Boa tarde. A agência fica perto da praça; a padaria está na outra esquina."));return;
            }
        }
        void Advance(string text){Step++;Feedback(text,9);PlayerPrefs.SetInt("PontoCego3D.Progress",Step);}
        public void Photograph()
        {
            if(Step==1&&Goal)
            {
                Vector3 delta=Goal.position+Vector3.up*1.8f-Camera.main.transform.position;
                if(Vector3.Distance(player.transform.position,Goal.position)<12&&Vector3.Dot(player.visual.forward,(Goal.position-player.transform.position).WithoutY().normalized)>.65f)Advance("Ribeiro: Foco certinho. Passa na oficina do Martins antes da gala. Ele quer falar com você.");
                else Feedback("Aproxime-se da padaria e mire na fachada.");
            }
        }
        public void EnemyDown(){Kills++;if(Step==3){bool any=false;foreach(var e in encounter)if(e&&e.GetComponent<CombatActor>().Alive)any=true;if(!any)Advance("O caminho está livre. Recolha o documento deixado no pátio.");}}
        public void RaiseAlarm(Vector3 point){foreach(var npc in FindObjectsByType<StreetPedestrian>())if(Vector3.Distance(npc.transform.position,point)<16)npc.Alert(point);}
        public void UseKit(){var actor=player.GetComponent<CombatActor>();if(kits<=0||actor.Health>=actor.maximum||!actor.Alive)return;kits--;actor.Heal(50);Feedback("Kit usado. +50 de vida.");}
        public void Respawn(){player.Warp(spawn);player.GetComponent<CombatActor>().Restore();player.orbit.Snap();Feedback("Amaral recuperou o fôlego perto da Riberal.");}
        public void StartCombatTest()
        {
            Step=3;player.Warp(new Vector3(63,.3f,12));player.GetComponent<CombatActor>().Restore();player.orbit.yaw=0;player.orbit.Snap();kits=3;
            foreach(var enemy in encounter){enemy.SetActive(true);enemy.GetComponent<CombatActor>().Restore();var agent=enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();if(UnityEngine.AI.NavMesh.SamplePosition(enemy.transform.position,out var hit,4,UnityEngine.AI.NavMesh.AllAreas)){agent.enabled=true;agent.Warp(hit.position);}}
            player.GetComponent<ThirdPersonInput>().SetPaused(false);Feedback("TESTE DE COMBATE: clique golpeia; CTRL esquiva; Q atordoa com o flash. H usa kit.",9);
        }
        void Init()
        {
            if(title!=null)return;title=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold,normal={textColor=Color.white}};
            text=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true,normal={textColor=Color.white}};small=new GUIStyle(text){fontSize=13};button=new GUIStyle(GUI.skin.button){fontSize=19,padding=new RectOffset(10,10,10,10)};
        }
        void Fill(Rect r,Color color){GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
        void OnGUI()
        {
            if(!player)return;Init();GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1280f,Screen.height/720f,1));
            var actor=player.GetComponent<CombatActor>();var input=player.GetComponent<ThirdPersonInput>();
            if(ui){PontoCego.CleanGui.Icon(new Rect(875,73,82,56),ui.icons[0]);PontoCego.CleanGui.Icon(new Rect(970,72,28,26),ui.icons[1]);PontoCego.CleanGui.Icon(new Rect(973,100,23,27),ui.icons[2]);PontoCego.CleanGui.Icon(new Rect(976,138,55,33),ui.caramelo);}
            PontoCego.CleanGui.Round(new Rect(1002,76,207,18),new Color(.25f,.07f,.08f));PontoCego.CleanGui.Round(new Rect(1002,76,207*actor.Health/actor.maximum,18),new Color(.81f,.08f,.14f));
            PontoCego.CleanGui.Round(new Rect(1002,104,207,18),new Color(.28f,.23f,.06f));PontoCego.CleanGui.Round(new Rect(1002,104,207*player.Stamina/player.staminaCapacity,18),new Color(1,.74f,0));GUI.Label(new Rect(893,130,80,35),flashCharges.ToString(),title);GUI.Label(new Rect(1035,136,210,40),"R$"+currency,new GUIStyle(title){normal={textColor=new Color(.2f,.52f,.15f)}});
            DrawMap(new Color(.04f,.06f,.07f,.9f),new Color(1,.7f,.2f));
            if(Time.time-FlashAt<.15f)Fill(new Rect(0,0,1280,720),new Color(1,1,1,.3f*(1-(Time.time-FlashAt)/.15f)));
            if(Time.time<MessageUntil&&Message!="IMPACTO"){Fill(new Rect(275,574,730,66),new Color(.025f,.035f,.05f,.88f));GUI.Label(new Rect(294,586,695,58),Message,text);}
            if(phone)DrawPhone();
            if(!actor.Alive||input.Paused)
            {
                Fill(new Rect(0,0,1280,720),new Color(0,0,0,.75f));GUILayout.BeginArea(new Rect(410,180,460,400));GUILayout.Label(actor.Alive?"PONTO CEGO / PAUSA":"AMARAL CAIU",title);
                if(GUILayout.Button(actor.Alive?"CONTINUAR":"VOLTAR À RIBERAL",button)){if(!actor.Alive)Respawn();input.SetPaused(false);}if(GUILayout.Button("TESTAR COMBATE",button))StartCombatTest();if(GUILayout.Button("VER CIDADE DE DIA",button)){PontoCego.WorldEnvironment.DayNightCycle.Instance.SetHour(12);input.SetPaused(false);}if(GUILayout.Button("VER CIDADE À NOITE",button)){PontoCego.WorldEnvironment.DayNightCycle.Instance.SetHour(0);input.SetPaused(false);}if(GUILayout.Button("SAIR",button))Application.Quit();GUILayout.EndArea();
            }
            GUI.matrix=Matrix4x4.identity;
        }
        public bool PhoneOpen=>phone;
        public PontoCego.PhoneApp ActivePhoneApp=>phoneApp;
        public void OpenPhone(PontoCego.PhoneApp app){if(!player||player.GetComponent<ThirdPersonInput>().Paused||!player.GetComponent<CombatActor>().Alive)return;phone=true;phoneApp=app;player.Locked=true;}
        public void ClosePhone(){phone=false;player.Locked=false;}
        void DrawPhone()
        {
            if(PontoCego.GamePhoneView.Frame(ui?ui.phone:null,ref phoneApp,PontoCego.WorldEnvironment.DayNightCycle.Instance.Clock))ClosePhone();
            var content=PontoCego.GamePhoneView.Content;float y=content.y+22;var taxi=FindAnyObjectByType<CityTaxi>();
            if(phoneApp==PontoCego.PhoneApp.Uber)
            {
                PontoCego.GamePhoneView.Message(ref y,taxi.Requested?(taxi.Arrived?"Higor chegou. Selecione o destino para marcar o trajeto.":"Higor está a caminho.\nChegada em "+Mathf.CeilToInt(taxi.Remaining)+" segundos."):"Higor / motorista de Uber\nChegada em 1 minuto.",95);
                if(!taxi.Requested&&PontoCego.GamePhoneView.Button(ref y,"CHAMAR UBER"))taxi.Request();
                if(taxi.Requested&&!taxi.Arrived&&PontoCego.GamePhoneView.Button(ref y,"CANCELAR CHAMADA"))taxi.Cancel();
                if(taxi.Arrived){foreach(var goal in goals)if(PontoCego.GamePhoneView.Button(ref y,goal.name)){taxi.Destination=goal;Feedback("Higor: marquei o destino no mapa.");ClosePhone();}}
                PontoCego.GamePhoneView.Message(ref y,taxi.Notice,85);
            }
            else if(phoneApp==PontoCego.PhoneApp.Map)
            {
                PontoCego.GamePhoneView.Message(ref y,Objective,100);var r=new Rect(content.x,y,content.width,180);Fill(r,new Color(.10f,.16f,.19f));for(int i=-2;i<=2;i++){float x=r.center.x+i*r.width/5.5f,z=r.center.y+i*r.height/5.5f;Fill(new Rect(x,r.y,4,r.height),new Color(.4f,.44f,.46f));Fill(new Rect(r.x,z,r.width,4),new Color(.4f,.44f,.46f));}Vector2 Mini(Vector3 p)=>r.center+new Vector2(p.x/224*r.width,-p.z/224*r.height);var me=Mini(player.transform.position);Fill(new Rect(me.x-3,me.y-3,6,6),Color.white);var target=taxi&&taxi.Destination?taxi.Destination:Goal;if(target){var at=Mini(target.position);Fill(new Rect(at.x-4,at.y-4,8,8),new Color(1,.7f,.2f));}y+=195;PontoCego.GamePhoneView.Message(ref y,target?"Destino: "+target.name:"Explore a cidade.",100);
            }
            else if(phoneApp==PontoCego.PhoneApp.Messages)PontoCego.GamePhoneView.Message(ref y,"MISSÃO\n"+Objective+"\n\n"+Message,300);
            else if(phoneApp==PontoCego.PhoneApp.Camera){PontoCego.GamePhoneView.Message(ref y,"Mire com o botão direito do mouse. Use Q para disparar o flash e registrar a fachada da missão.",130);if(PontoCego.GamePhoneView.Button(ref y,"VOLTAR PARA FOTOGRAFAR"))ClosePhone();}
            else if(phoneApp==PontoCego.PhoneApp.News)PontoCego.GamePhoneView.Message(ref y,"Comunidade de Patos\n\nO Parque do Mocambo está aberto. Confira a quadra, os bancos e a nova iluminação pública ao anoitecer.",210);
            else if(phoneApp==PontoCego.PhoneApp.Browser)PontoCego.GamePhoneView.Message(ref y,"Guia local\n\nRiberal: agência de marketing.\nPadaria: encontro e fotografia.\nOficina: Martins.\nMocambo: lazer e basquete.\nDistribuidora: capangas e pistas.",270);
            else if(phoneApp==PontoCego.PhoneApp.Music)PontoCego.GamePhoneView.Message(ref y,"Player de música\n\nNenhuma faixa na biblioteca do aparelho.",140);
            else if(phoneApp==PontoCego.PhoneApp.Videos)PontoCego.GamePhoneView.Message(ref y,"Galeria de vídeos\n\nNenhum vídeo gravado neste aparelho.",140);
            GUI.depth=0;
        }
        void DrawMap(Color ink,Color gold)
        {
            Fill(new Rect(24,522,205,180),ink);
            for(int i=-2;i<=2;i++){float v=126+i*33;Fill(new Rect(v,532,5,155),new Color(.3f,.33f,.35f));Fill(new Rect(35,610+i*29,183,5),new Color(.3f,.33f,.35f));}
            Vector2 Mini(Vector3 p)=>new Vector2(126+p.x*.78f,611-p.z*.69f);
            var me=Mini(player.transform.position);Fill(new Rect(me.x-3,me.y-3,6,6),Color.white);
            foreach(var npc in FindObjectsByType<StreetPedestrian>()){var at=Mini(npc.transform.position);Fill(new Rect(at.x-1.5f,at.y-1.5f,3,3),npc.hostile?Color.red:new Color(.6f,.7f,.8f));}
            var navigation=FindAnyObjectByType<CityTaxi>();var mapTarget=navigation&&navigation.Destination?navigation.Destination:Goal;if(mapTarget){var at=Mini(mapTarget.position);Fill(new Rect(at.x-3,at.y-3,6,6),gold);}GUI.Label(new Rect(40,678,180,25),"BAIRRO ABERTO / CENTRO",small);
        }
    }
}
