using UnityEngine;
namespace PontoCego
{
    /// <summary>The user's title artwork with accessible controls, preserved campaign and test slots.</summary>
    public sealed class MainMenu:MonoBehaviour
    {
        bool selecting,accessibility,choosingDifficulty;int chosenDifficulty=1;string selectedPhase;Vector2 phaseScroll;float reveal,panelReveal;
        static readonly string[] colourNames={"Original","Protanopia","Deuteranopia","Tritanopia"};
        public bool AccessibilityOpen=>accessibility;
        public AudioClip menuMusic;AudioSource musicSource;
        public Texture2D background,portrait;public GameUiArt art;
        void Start(){if(!GetComponent<GameAutoUpdater>())gameObject.AddComponent<GameAutoUpdater>();Cursor.visible=true;if(menuMusic){musicSource=gameObject.AddComponent<AudioSource>();musicSource.clip=menuMusic;musicSource.loop=true;musicSource.playOnAwake=false;musicSource.spatialBlend=0;musicSource.volume=0;musicSource.Play();}}
        void Update(){if(musicSource)musicSource.volume=Mathf.MoveTowards(musicSource.volume,.4f*GamePreferences.Music,Time.unscaledDeltaTime*.5f);reveal=UiTheme.StepVisibility(reveal,true,Time.unscaledDeltaTime,GamePreferences.ReduceMotion);panelReveal=UiTheme.StepVisibility(panelReveal,selecting||accessibility||choosingDifficulty,Time.unscaledDeltaTime,GamePreferences.ReduceMotion);}
        public void StartGame(){UiSound.PlayClick();if(GameSession.HasSave)GameSession.Continue();else OpenDifficulty(null);}
        public void OpenAccessibility(){accessibility=true;selecting=false;panelReveal=0;UiSound.PlayOpen();}
        public void OpenDifficulty(string phase){selectedPhase=phase;choosingDifficulty=true;selecting=accessibility=false;panelReveal=0;UiSound.PlayOpen();}
        void OpenPhases(){selecting=true;accessibility=false;panelReveal=0;UiSound.PlayOpen();}
        void OnGUI()
        {
            var oldMatrix=GUI.matrix;var oldColor=GUI.color;int oldDepth=GUI.depth;bool oldEnabled=GUI.enabled;
            try
            {
                GUI.matrix=UiTheme.SafeMatrix();
                if(art&&art.mainMenu)GUI.DrawTexture(new Rect(0,0,1280,720),art.mainMenu,ScaleMode.StretchToFill);
                float alpha=UiTheme.MenuEase(reveal);GUI.color=oldColor*new Color(1,1,1,alpha);
                if(selecting||accessibility||choosingDifficulty)
                {
                    GUI.color*=new Color(0,0,0,.65f);GUI.DrawTexture(new Rect(0,0,1280,720),Texture2D.whiteTexture);
                    float panel=UiTheme.MenuEase(panelReveal);GUI.color=oldColor*new Color(1,1,1,panel);GUI.matrix=UiTheme.SafeMatrix()*Matrix4x4.Translate(new Vector3(0,GamePreferences.ReduceMotion?0:(1-panel)*18,0));
                    if(choosingDifficulty)DrawDifficulty();else if(selecting)DrawPhaseSelector();else DrawAccessibility();
                    if(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.Escape){selecting=accessibility=choosingDifficulty=false;UiSound.PlayClick();Event.current.Use();}
                    return;
                }
                GUI.matrix=UiTheme.SafeMatrix()*Matrix4x4.Translate(new Vector3(0,GamePreferences.ReduceMotion?0:(1-alpha)*14,0));
                float y=378;
                if(GameSession.HasSave){if(Button(new Rect(490,y,300,49),"CONTINUAR"))GameSession.Continue();y+=59;}
                if(Button(new Rect(490,y,300,49),"NOVO JOGO"))OpenDifficulty(null);y+=59;
                if(Button(new Rect(490,y,300,45),"ESCOLHER FASE"))OpenPhases();y+=55;
                if(Button(new Rect(490,y,300,42),"ACESSIBILIDADE",15))OpenAccessibility();
                if(Event.current.type==EventType.KeyDown&&(Event.current.keyCode==KeyCode.Return||Event.current.keyCode==KeyCode.Space)){StartGame();Event.current.Use();}
                CleanGui.ShadowLabel(new Rect(26,691,1150,25),"DEMO 2D • "+Application.version+" • OBJETIVOS / ROTAS / DIA E NOITE",UiTheme.Label(11,Color.white));
            }
            finally{GUI.matrix=oldMatrix;GUI.color=oldColor;GUI.depth=oldDepth;GUI.enabled=oldEnabled;}
        }
        static bool Button(Rect r,string text,int size=19,bool selected=false)
        {
            bool hover=r.Contains(Event.current.mousePosition);CleanGui.Round(r,selected?UiTheme.Accent:hover?Color.Lerp(UiTheme.Panel,Color.white,.12f):UiTheme.Surface);
            var style=new GUIStyle(UiTheme.Label(size,selected?Color.black:UiTheme.Accent,true)){alignment=TextAnchor.MiddleCenter,wordWrap=false};
            while(style.fontSize>12&&style.CalcSize(new GUIContent(text)).x>r.width-22)style.fontSize--;
            GUI.Label(r,text,style);bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);if(clicked)UiSound.PlayClick();return clicked;
        }
        static void Panel(string title,string subtitle)
        {
            CleanGui.Round(new Rect(210,60,860,600),UiTheme.Surface);GUI.Label(new Rect(245,85,790,55),title,UiTheme.Label(27,UiTheme.Accent,true));GUI.Label(new Rect(245,146,790,50),subtitle,UiTheme.Label(14,UiTheme.Muted));
        }
        void DrawPhaseSelector()
        {
            Panel("ESCOLHER FASE","Partida de teste • sua campanha em Continuar será preservada.");
            var destinations=UberDriver.Destinations;phaseScroll=GUI.BeginScrollView(new Rect(245,210,790,350),phaseScroll,new Rect(0,0,765,(destinations.Length-1)*51));int row=0;
            foreach(var id in destinations){if(id=="25_Bairro")continue;if(Button(new Rect(0,row++*51,755,44),UberDriver.TitleFor(id),16))OpenDifficulty(id);}
            GUI.EndScrollView();if(Button(new Rect(245,586,790,44),"VOLTAR",16))selecting=false;
        }
        void DrawDifficulty()
        {
            Panel("ESCOLHA A DIFICULDADE","A escolha será salva nesta partida. Continuar preserva a dificuldade anterior.");
            string[] descriptions={"HISTÓRIA • inimigos com menos vida, ataques mais lentos e dano reduzido.","NORMAL • experiência original, recursos limitados e combate equilibrado.","DIFÍCIL • inimigos mais resistentes, ágeis e perigosos. Esquiva e cobertura importam."};
            for(int i=0;i<3;i++)if(Button(new Rect(245,215+i*83,790,68),descriptions[i],14,chosenDifficulty==i))chosenDifficulty=i;
            if(Button(new Rect(245,493,790,48),"INICIAR • "+DifficultyRules.Name((GameDifficulty)chosenDifficulty),17)){if(string.IsNullOrEmpty(selectedPhase))GameSession.NewCampaign(false,chosenDifficulty);else GameSession.StartSelectedMission(selectedPhase,chosenDifficulty);}
            if(Button(new Rect(245,586,790,44),"VOLTAR",16))choosingDifficulty=false;
        }
        void DrawAccessibility()
        {
            Panel("ACESSIBILIDADE","Estas preferências também valem durante a partida.");GUI.changed=false;
            GUI.Label(new Rect(245,220,590,38),"Tamanho dos textos",UiTheme.Label(18));GUI.Label(new Rect(925,220,110,38),Mathf.RoundToInt(GamePreferences.TextScale*100)+"%",UiTheme.Label(18,UiTheme.Accent,true));
            GamePreferences.TextScale=GUI.HorizontalSlider(new Rect(245,269,790,26),GamePreferences.TextScale,.9f,1.3f);
            GUI.Label(new Rect(245,310,790,38),"Cores acessíveis",UiTheme.Label(18));for(int i=0;i<colourNames.Length;i++)if(Button(new Rect(245+i*200,358,190,43),colourNames[i],13,GamePreferences.ColorMode==i)){GamePreferences.ColorMode=i;GUI.changed=true;}
            GUI.Label(new Rect(245,439,580,40),"Reduzir animações da interface",UiTheme.Label(17));
            if(Button(new Rect(845,438,190,40),GamePreferences.ReduceMotion?"ATIVADO":"DESATIVADO",13,GamePreferences.ReduceMotion)){GamePreferences.ReduceMotion=!GamePreferences.ReduceMotion;GUI.changed=true;}
            GUI.Label(new Rect(245,506,790,60),"Vida, fôlego e objetivos usam textos e valores além das cores. A arte original é preservada.",UiTheme.Label(14,UiTheme.Muted));
            if(GUI.changed)GamePreferences.Save();if(Button(new Rect(245,586,790,44),"VOLTAR",16))accessibility=false;
        }
    }
}
