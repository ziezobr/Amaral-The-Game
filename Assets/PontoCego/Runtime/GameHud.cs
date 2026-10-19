using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
namespace PontoCego
{
    public sealed class GameHud:MonoBehaviour
    {
        public GameUiArt art;
        public PlayerMotor motor;public PlayerInputReader input;public Interaction interaction;public LevelProgress level;
        public float StaminaOpacity{get;private set;}
        GUIStyle title,label,small,button,heading;Vector2 scroll;StoryDialogue dialogue;Health health;CombatPlayer combat;CombatMission mission;EquipmentShop shop;
        float staminaUsedAt=-100,lastUseAt=-100,nextDangerCheck;float lastLife=-1;
        int lastFlash=-1,lastMagazine=-1,lastReserve=-1,lastMoney=-1;bool inDanger;
        string lastObjective;CityNavigation gps;AmmoInventory inventory;PlayerPhone phone;
        public float HudOpacity{get;private set;}=1;
        public bool InDanger=>inDanger;
        bool journalOpen;Vector2 journalScroll;
        readonly Color gold=new Color(.99f,.73f,.27f),ink=new Color(.035f,.045f,.06f,.94f);
        void Init()
        {
            if(title!=null)return;
            title=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold,normal={textColor=Color.white}};
            label=new GUIStyle(GUI.skin.label){fontSize=20,wordWrap=true,normal={textColor=new Color(.92f,.92f,.88f)}};
            small=new GUIStyle(label){fontSize=13};heading=new GUIStyle(title){fontSize=14,normal={textColor=gold}};
            button=new GUIStyle(GUI.skin.button){fontSize=16,padding=new RectOffset(8,8,8,8)};
            BindReferences();
        }
        void BindReferences()
        {
            if(!input)return;
            dialogue=input.GetComponent<StoryDialogue>();health=input.GetComponent<Health>();combat=input.GetComponent<CombatPlayer>();
            inventory=input.GetComponent<AmmoInventory>();phone=input.GetComponent<PlayerPhone>();
            if(level)mission=level.GetComponent<CombatMission>();shop=FindAnyObjectByType<EquipmentShop>();gps=FindAnyObjectByType<CityNavigation>();
        }
        void Update()
        {
            PauseMenuView.Tick(input);
            if(!motor||motor.Model==null||!input)return;
            if(!health)BindReferences();
            if(motor.Model.ShowStamina)staminaUsedAt=Time.time;
            float target=Time.time-staminaUsedAt<.45f?1:0;
            StaminaOpacity=Mathf.MoveTowards(StaminaOpacity,target,Time.unscaledDeltaTime*5);
            var save=GameSession.Current;
            if(health&&Mathf.Abs(health.Current-lastLife)>.01f||save.flashCharges!=lastFlash||save.magazine!=lastMagazine||save.ammoReserve!=lastReserve||save.currency!=lastMoney)lastUseAt=Time.unscaledTime;
            lastLife=health?health.Current:-1;lastFlash=save.flashCharges;lastMagazine=save.magazine;lastReserve=save.ammoReserve;lastMoney=save.currency;
            if(Time.unscaledTime>=nextDangerCheck)
            {
                nextDangerCheck=Time.unscaledTime+.4f;inDanger=false;
                foreach(var enemy in FindObjectsByType<EnemyAgent>())if(enemy.Alive&&Vector2.SqrMagnitude(enemy.transform.position-motor.transform.position)<144){inDanger=true;break;}
                if(!gps)gps=FindAnyObjectByType<CityNavigation>();
            }
            bool used=Time.unscaledTime-lastUseAt<4||StaminaOpacity>.01f||health&&Time.time-health.LastDamageTime<5||combat&&Time.time-combat.AttackStarted<4||inventory&&inventory.Reloading;
            HudOpacity=Mathf.MoveTowards(HudOpacity,UiTheme.HudTarget(GamePreferences.SmartHud,inDanger,used,LifeFraction<.4f),Time.unscaledDeltaTime*3);
            string objective=level?level.Objective:null;if(lastObjective!=null&&objective!=lastObjective)UiSound.PlayNotify();lastObjective=objective;
        }
        void Fill(Rect r,Color c){var old=GUI.color;GUI.color=old*c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Portrait(Rect r,Texture2D t){Fill(r,Color.black);if(t)GUI.DrawTextureWithTexCoords(r,t,new Rect(.03f,.36f,.94f,.64f));}
        public void SetLevel(LevelProgress selected){level=selected;mission=selected.GetComponent<CombatMission>();shop=FindAnyObjectByType<EquipmentShop>();}
        public float LifeFraction{get{var h=health?health:input?input.GetComponent<Health>():null;return h?Mathf.Clamp01(h.Current/h.maximum):1;}}
        public float StaminaFraction=>motor&&motor.Model!=null?Mathf.Clamp01(motor.Model.Stamina/motor.settings.sprintCapacity):1;
        public int CameraCount=>GameSession.Current.flashCharges;
        void OnGUI()
        {
            if(!input||!motor||!level)return;
            foreach(var driver in FindObjectsByType<UberDriver>())if(driver.Active)return;
            var previousMatrix=GUI.matrix;var previousColor=GUI.color;int previousDepth=GUI.depth;bool previousEnabled=GUI.enabled;
            try
            {
            Init();foreach(var seller in FindObjectsByType<EquipmentShop>())if(seller.Active){shop=seller;break;}
            GUI.matrix=UiTheme.SafeMatrix();
            title.fontSize=UiTheme.Font(25);label.fontSize=UiTheme.Font(20);small.fontSize=UiTheme.Font(13);heading.fontSize=UiTheme.Font(14);button.fontSize=UiTheme.Font(16);
            label.normal.textColor=UiTheme.Text;small.normal.textColor=UiTheme.Text;heading.normal.textColor=UiTheme.Accent;
            var aim=input.GetComponent<PlayerAim>();if(aim&&aim.FlashIntensity>0)Fill(new Rect(0,0,1280,720),new Color(1,1,1,aim.FlashIntensity*.12f));
            GUI.color=new Color(1,1,1,HudOpacity);DrawStatus();GUI.color=previousColor;DrawMap();
            if(!input.GameplayLocked&&!input.Paused&&(!dialogue||!dialogue.Active)){
                if(Time.unscaledTime<interaction.MessageUntil){CleanGui.Round(new Rect(280,634,660,52),ink);GUI.Label(new Rect(294,640,632,44),interaction.Message,new GUIStyle(small){fontSize=UiTheme.Font(14),alignment=TextAnchor.MiddleCenter});}
                else if(!string.IsNullOrEmpty(interaction.Prompt)){CleanGui.Round(new Rect(390,650,500,34),ink);GUI.Label(new Rect(400,652,480,30),interaction.Prompt,new GUIStyle(small){fontSize=UiTheme.Font(14),alignment=TextAnchor.MiddleCenter});}}

            if(motor.Model.InfiniteStaminaRemaining>0)GUI.Label(new Rect(1002,194,220,26),"FÔLEGO ∞ "+Mathf.CeilToInt(motor.Model.InfiniteStaminaRemaining)+" s",small);
            var effects=input.GetComponent<Health>();if(effects.InvincibleRemaining>0)GUI.Label(new Rect(1002,222,220,26),"INVENCÍVEL "+Mathf.CeilToInt(effects.InvincibleRemaining)+" s",small);
            GUI.enabled=previousEnabled&&!input.Paused&&(!health||health.Alive);
            if(dialogue&&dialogue.Active)DrawDialogue();
            if(shop&&shop.Active)DrawShop();
            foreach(var puzzle in FindObjectsByType<EvidencePuzzle>())if(puzzle.Active)DrawPuzzle(puzzle);
            var ending=FindAnyObjectByType<CampaignEnding>();if(ending&&ending.Active)DrawEnding(ending);
            GUI.enabled=previousEnabled;
            if(health&&!health.Alive)DrawDeath();else if(PauseMenuView.Visible)DrawPause();
            }
            finally{GUI.matrix=previousMatrix;GUI.color=previousColor;GUI.depth=previousDepth;GUI.enabled=previousEnabled;}
        }
        void DrawStatus()
        {
            Color hudColor=GUI.color;
            if(art&&art.icons!=null&&art.icons.Length>=3)
            {
                CleanGui.Icon(new Rect(875,73,82,56),art.icons[0]);CleanGui.Icon(new Rect(970,72,28,28),art.icons[1]);
                if(StaminaOpacity>.01f){GUI.color=hudColor*new Color(1,1,1,StaminaOpacity);CleanGui.Icon(new Rect(973,100,24,28),art.icons[2]);GUI.color=hudColor;}
            }
            CleanGui.Round(new Rect(1002,76,207,21),new Color(.07f,.09f,.12f));CleanGui.Round(new Rect(1002,76,207*LifeFraction,21),UiTheme.Life);
            CleanGui.OutlinedLabel(new Rect(1010,76,191,24),"VIDA  "+Mathf.CeilToInt(health?health.Current:100)+" / "+Mathf.CeilToInt(health?health.maximum:100),new GUIStyle(UiTheme.Label(10,Color.white,true)){alignment=TextAnchor.MiddleCenter});
            if(StaminaOpacity>.01f)
            {
                GUI.color=hudColor*new Color(1,1,1,StaminaOpacity);CleanGui.Round(new Rect(1002,105,207,21),new Color(.09f,.1f,.13f));CleanGui.Round(new Rect(1002,105,207*StaminaFraction,21),UiTheme.Stamina);
                CleanGui.OutlinedLabel(new Rect(1010,105,191,24),"FÔLEGO  "+Mathf.RoundToInt(StaminaFraction*100)+"%",new GUIStyle(UiTheme.Label(10,Color.white,true)){alignment=TextAnchor.MiddleCenter});GUI.color=hudColor;
            }
            CleanGui.ShadowLabel(new Rect(882,132,70,31),CameraCount.ToString(),new GUIStyle(UiTheme.Label(23,Color.white,true)){alignment=TextAnchor.MiddleCenter});
            var save=GameSession.Current;
            if(save.hasSidearm)CleanGui.ShadowLabel(new Rect(850,163,135,26),inventory&&inventory.Reloading?"RECARREGANDO":save.magazine+" / "+save.ammoReserve,new GUIStyle(UiTheme.Label(10,UiTheme.Text,true)){alignment=TextAnchor.MiddleCenter,wordWrap=false});
            if(art)CleanGui.Icon(new Rect(976,138,55,33),art.caramelo);
            string money="R$"+save.currency.ToString("N0",new System.Globalization.CultureInfo("pt-BR"));var moneyStyle=UiTheme.Label(28,UiTheme.Money,true);moneyStyle.wordWrap=false;
            while(moneyStyle.fontSize>16&&moneyStyle.CalcSize(new GUIContent(money)).x>205)moneyStyle.fontSize--;
            CleanGui.ShadowLabel(new Rect(1038,134,205,47),money,moneyStyle);GUI.color=hudColor;
        }
        void DrawMap()
        {
            if(!GamePreferences.Minimap)return;
            if(gps){gps.DrawMap(new Rect(30,548,208,134),true);DrawMapCaption(Mathf.CeilToInt(gps.Distance)+" m");return;}
            DrawMapCaption("");
            if(level.mapTexture)GUI.DrawTexture(new Rect(30,548,208,134),level.mapTexture,ScaleMode.StretchToFill);
            var world=WorldHub.Instance;
            if(world){Fill(new Rect(30,548,208,134),new Color(.18f,.23f,.15f));
                if(world.roadStarts!=null)for(int i=0;i<world.roadStarts.Length;i++){var a=world.MapUV(world.roadStarts[i]);var b=world.MapUV(world.roadEnds[i]);Fill(new Rect(30+Mathf.Min(a.x,b.x)*208,548+Mathf.Min(a.y,b.y)*134,Mathf.Max(1.5f,Mathf.Abs(a.x-b.x)*208),Mathf.Max(1.5f,Mathf.Abs(a.y-b.y)*134)),new Color(.55f,.56f,.51f));}
                for(int i=0;i<world.districtRects.Length;i++){var r=world.districtRects[i];var uv=world.MapUV(new Vector2(r.xMin,r.yMax));GUI.DrawTexture(new Rect(30+uv.x*208,548+uv.y*134,r.width/world.bounds.width*208,r.height/world.bounds.height*134),world.districtMaps[i],ScaleMode.StretchToFill);}}
            Vector2 Mini(Vector3 p){var uv=world?world.MapUV(p):new Vector2((p.x-level.mapCenter.x)/level.mapSize.x+.5f,.5f-(p.y-level.mapCenter.y)/level.mapSize.y);return new Vector2(30+uv.x*208,548+uv.y*134); }
            var at=Mini(motor.transform.position);Fill(new Rect(at.x-3,at.y-3,6,6),Color.white);
            if(level.CurrentTarget&&art){var q=Mini(level.CurrentTarget.position);CleanGui.Icon(new Rect(q.x-5,q.y-5,10,10),art.icons[12]);}

        }
        void DrawMapCaption(string distance)
        {
            var clock=WorldEnvironment.DayNightCycle.Instance;
            bool time=GamePreferences.Clock&&clock;var weather=WorldWeather2D.Instance;
            CleanGui.Round(new Rect(30,time?493:512,208,time?50:31),UiTheme.Surface);
            if(time)
            {
                UiTheme.DrawTimeIcon(new Rect(40,497,19,19),clock.Night>.5f);
                GUI.Label(new Rect(67,496,75,25),clock.Clock,UiTheme.Label(11,UiTheme.Text,true));
                GUI.Label(new Rect(143,496,85,25),distance,new GUIStyle(UiTheme.Label(11,UiTheme.Text,true)){alignment=TextAnchor.UpperRight,wordWrap=false});
                UiTheme.DrawWeatherIcon(new Rect(40,521,18,16),weather?weather.Kind:WeatherKind.Clear);
                GUI.Label(new Rect(67,520,165,24),UiTheme.WeatherLabel(weather?weather.Kind:WeatherKind.Clear),UiTheme.Label(10,UiTheme.Muted,true));
            }
            else GUI.Label(new Rect(40,516,190,25),distance,UiTheme.Label(11,UiTheme.Text,true));
        }
        void DrawObjective()
        {
            if(!level.CurrentTarget||input.Paused||dialogue&&dialogue.Active)return;var p=Camera.main.WorldToViewportPoint(level.CurrentTarget.position+Vector3.up*.4f);
            float x=Mathf.Clamp(p.x*1280,40,1240),y=Mathf.Clamp((1-p.y)*720,158,510);
            GUI.Label(new Rect(x-16,y-22,40,46),"◇",new GUIStyle(title){fontSize=32,normal={textColor=gold}});
            if(p.x<0||p.x>1||p.y<0||p.y>1)GUI.Label(new Rect(x-35,y+14,120,24),"OBJETIVO",small);
        }
        void DrawEnemyBars()
        {
            foreach(var enemy in FindObjectsByType<EnemyAgent>())
            {
                if(!enemy.Alive)continue;var h=enemy.GetComponent<Health>();var p=Camera.main.WorldToViewportPoint(enemy.transform.position+Vector3.up*2.25f);
                if(p.x<0||p.x>1||p.y<0||p.y>1)continue;float x=p.x*1280,y=(1-p.y)*720;
                Fill(new Rect(x-24,y,48,5),ink);Fill(new Rect(x-24,y,48*h.Current/h.maximum,5),h.boss?gold:new Color(.9f,.22f,.18f));
                if(!string.IsNullOrEmpty(enemy.StateLabel))GUI.Label(new Rect(x-58,y-22,132,23),enemy.StateLabel,new GUIStyle(small){fontSize=11});
            }
        }
        void DrawDialogue()
        {
            Fill(new Rect(24,492,1232,208),ink);Fill(new Rect(24,492,1232,3),gold);Portrait(new Rect(36,504,176,184),dialogue.Portrait);
            GUI.Label(new Rect(236,511,955,35),dialogue.Speaker.ToUpperInvariant(),new GUIStyle(title){normal={textColor=gold}});
            GUI.Label(new Rect(236,555,966,88),dialogue.CurrentLine,new GUIStyle(label){fontSize=UiTheme.Font(20)});
            GUI.Label(new Rect(236,654,660,28),"E / A para continuar  "+dialogue.LineNumber+" / "+dialogue.LineCount,small);
            if(GUI.Button(new Rect(1060,650,170,35),"CONTINUAR ›",button))dialogue.Advance();
        }
        void DrawBoss()
        {
            foreach(var boss in FindObjectsByType<BossAbilities>())
            {
                var hp=boss.GetComponent<Health>();if(!hp.Alive)continue;var enemy=boss.GetComponent<EnemyAgent>();string name=enemy.role==EnemyRole.Pivo?"PIVÔ / DONO DA QUADRA":enemy.role==EnemyRole.Cenoura?"CENOURA / ESCUDO DE SUCATA":"PAULO / DONO DO ESTÚDIO";
                Fill(new Rect(470,78,510,78),ink);GUI.Label(new Rect(485,84,480,25),name+" • FASE "+boss.Phase,heading);
                Fill(new Rect(485,113,480,9),new Color(.2f,.08f,.08f));Fill(new Rect(485,113,480*hp.Current/hp.maximum,9),boss.Enraged?new Color(1,.28f,.12f):gold);
                string state=boss.Charging?boss.AbilityName:boss.Dashing?"INVESTIDA!":boss.Recovering?"VULNERÁVEL — CONTRA-ATAQUE!":"Use o flash na preparação e ataque após a esquiva.";
                if(enemy.FlashResistant&&!boss.Recovering&&!boss.Dashing)state="RESISTÊNCIA AO FLASH "+enemy.FlashResistanceRemaining.ToString("0.0")+"s • ESQUIVE!";
                GUI.Label(new Rect(485,128,480,24),state,small);break;
            }
        }
        void DrawDeath()
        {
            Fill(new Rect(0,0,1280,720),new Color(0,0,0,.8f));Fill(new Rect(350,210,580,280),ink);
            GUI.Label(new Rect(390,240,500,50),"VOCÊ CAIU",title);GUI.Label(new Rect(390,295,500,50),"Retome do último objetivo salvo.",label);
            if(GUI.Button(new Rect(390,368,500,45),"VOLTAR AO CHECKPOINT",button))GameSession.Retry();
            if(GUI.Button(new Rect(390,425,500,42),"MENU PRINCIPAL",button))GameSession.ReturnToMenu();
        }
        void DrawShop()
        {
            Fill(new Rect(0,0,1280,720),new Color(0,0,0,.96f));
            if(art)CleanGui.Icon(new Rect(936,70,55,28),art.caramelo);
            GUI.Label(new Rect(994,65,250,40),"R$"+GameSession.Current.currency.ToString("N0"),new GUIStyle(title){fontSize=30,normal={textColor=new Color(.18f,.4f,.13f)}});
            for(int i=0;i<EquipmentShop.ProductCount;i++)
            {
                float y=72+i*60;CleanGui.Round(new Rect(473,y,45,45),Color.Lerp(new Color(.59f,.66f,.35f),new Color(.22f,.31f,.17f),UiTheme.Night));CleanGui.Round(new Rect(527,y,280,45),Color.Lerp(new Color(.89f,.95f,.74f),new Color(.14f,.22f,.17f),UiTheme.Night));
                if(art)CleanGui.Icon(new Rect(477,y+4,37,37),art.icons[EquipmentShop.IconIndices[i]]);
                if(GUI.Button(new Rect(527,y,280,45),GUIContent.none,GUIStyle.none)){shop.Buy(i);UiSound.PlayClick();}
                GUI.Label(new Rect(540,y+3,257,23),EquipmentShop.ProductName(i),new GUIStyle(label){fontSize=UiTheme.Font(15),fontStyle=FontStyle.Bold,normal={textColor=Color.Lerp(new Color(.13f,.2f,.09f),new Color(.84f,.92f,.79f),UiTheme.Night)}});
                GUI.Label(new Rect(540,y+24,257,20),"R$ "+EquipmentShop.Price(i)+(GameSession.Current.currency>=EquipmentShop.Price(i)?"":" • SEM SALDO"),new GUIStyle(small){fontSize=UiTheme.Font(12),normal={textColor=GameSession.Current.currency>=EquipmentShop.Price(i)?Color.Lerp(new Color(.2f,.32f,.12f),new Color(.63f,.85f,.53f),UiTheme.Night):Color.Lerp(new Color(.65f,.14f,.1f),new Color(1,.65f,.45f),UiTheme.Night)}});
            }
            GUI.Label(new Rect(30,610,395,65),shop.Notice,label);if(GUI.Button(new Rect(1050,640,180,40),"VOLTAR",button)){UiSound.PlayClick();shop.Close();}
        }
        void DrawEnding(CampaignEnding ending)
        {
            Fill(new Rect(240,140,800,440),ink);GUI.Label(new Rect(280,170,720,45),"PONTO CEGO / FIM DESTE TRECHO",title);
            GUI.Label(new Rect(280,242,720,155),ending.closingText,label);
            var time=System.TimeSpan.FromSeconds(GameSession.Current.playedSeconds);GUI.Label(new Rect(280,414,720,45),"Tempo efetivamente jogado: "+time.ToString(@"hh\:mm\:ss"),label);
            if(GUI.Button(new Rect(280,494,720,48),"VOLTAR AO MENU",button))GameSession.ReturnToMenu();
        }
        void DrawPuzzle(EvidencePuzzle puzzle)
        {
            Fill(new Rect(0,0,1280,720),new Color(0,0,0,.65f));Fill(new Rect(250,135,780,450),ink);
            GUI.Label(new Rect(285,160,710,40),"CRUZAR AS PISTAS",title);GUI.Label(new Rect(285,215,710,105),puzzle.question,label);
            for(int i=0;i<puzzle.options.Length;i++)if(GUI.Button(new Rect(285,325+i*54,710,46),(i==puzzle.Selected?"› ":"")+(i+1)+". "+puzzle.options[i],button))puzzle.Submit(i);
            GUI.Label(new Rect(285,493,710,40),puzzle.Notice,small);if(GUI.Button(new Rect(285,536,710,35),"VOLTAR À INVESTIGAÇÃO",button))puzzle.Close();
        }
        void DrawJournal()
        {
            Fill(new Rect(0,0,1280,720),new Color(0,0,0,.65f));Fill(new Rect(200,80,880,560),ink);
            GUI.Label(new Rect(235,105,760,45),"CADERNO DE AMARAL",title);journalScroll=GUI.BeginScrollView(new Rect(235,165,810,390),journalScroll,new Rect(0,0,780,Mathf.Max(370,GameSession.Current.journal.Count*110)));
            if(GameSession.Current.journal.Count==0)GUI.Label(new Rect(0,0,770,80),"Fotografe e examine documentos para reunir as provas.",label);
            for(int i=0;i<GameSession.Current.journal.Count;i++)GUI.Label(new Rect(0,i*110,770,105),GameSession.Current.journal[i],label);GUI.EndScrollView();
            if(GUI.Button(new Rect(235,579,810,40),"FECHAR CADERNO",button)){journalOpen=false;bool other=dialogue&&dialogue.Active||shop&&shop.Active;foreach(var p in FindObjectsByType<EvidencePuzzle>())other|=p.Active;input.SetGameplayLocked(other);}
        }
        void DrawPause(){PauseMenuView.Draw(input,level);}
    }
}
