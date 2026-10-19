using UnityEngine;
using UnityEngine.InputSystem;
namespace PontoCego
{
    /// <summary>Pause folders use unscaled animation and preserve every caller's IMGUI state.</summary>
    public static class PauseMenuView
    {
        public static int ActiveFolder;
        static Vector2 controlsScroll;
        static readonly string[] folders={"Jogo","Controles","Áudio","Vídeo","Interface","Acessibilidade"};
        static readonly string[] colourNames={"Original","Protanopia","Deuteranopia","Tritanopia"};
        static float visibility;static PlayerInputReader owner;static bool wasOpen;
        public static bool Visible=>visibility>.001f;
        static Color ink=>UiTheme.Surface;static Color gold=>UiTheme.Accent;static Color muted=>UiTheme.Muted;
        static GUIStyle Text(int size=17,Color? color=null,bool bold=false)=>UiTheme.Label(size,color,bold);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){owner=null;visibility=0;wasOpen=false;ActiveFolder=0;}
        public static void Tick(PlayerInputReader input)
        {
            if(owner!=input){owner=input;visibility=0;wasOpen=false;}
            bool open=input&&input.Paused;
            if(open&&!wasOpen)UiSound.PlayOpen();
            wasOpen=open;visibility=UiTheme.StepVisibility(visibility,open,Time.unscaledDeltaTime,GamePreferences.ReduceMotion);
            ActiveFolder=Mathf.Clamp(ActiveFolder,0,folders.Length-1);
            if(!open||input.Rebinding)return;
            var keys=Keyboard.current;int selected=ActiveFolder;
            if(keys!=null)
            {
                if(keys.tabKey.wasPressedThisFrame)selected=(selected+(keys.shiftKey.isPressed?folders.Length-1:1))%folders.Length;
                if(keys.digit1Key.wasPressedThisFrame)selected=0;if(keys.digit2Key.wasPressedThisFrame)selected=1;
                if(keys.digit3Key.wasPressedThisFrame)selected=2;if(keys.digit4Key.wasPressedThisFrame)selected=3;
                if(keys.digit5Key.wasPressedThisFrame)selected=4;if(keys.digit6Key.wasPressedThisFrame)selected=5;
            }
            var pad=Gamepad.current;if(pad!=null){if(pad.rightShoulder.wasPressedThisFrame)selected=(selected+1)%folders.Length;if(pad.leftShoulder.wasPressedThisFrame)selected=(selected+folders.Length-1)%folders.Length;}
            if(selected!=ActiveFolder){ActiveFolder=selected;UiSound.PlayClick();}
        }
        static bool Button(Rect r,string text,bool primary=false,int size=17)
        {
            bool hover=r.Contains(Event.current.mousePosition);
            CleanGui.Round(r,primary?(hover?Color.Lerp(gold,Color.white,.2f):gold):(hover?Color.Lerp(UiTheme.Panel,Color.white,.12f):UiTheme.Panel));
            GUI.Label(r,text,new GUIStyle(Text(size,primary?new Color(.035f,.045f,.065f):UiTheme.Text,true)){alignment=TextAnchor.MiddleCenter,wordWrap=false});
            bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);if(clicked)UiSound.PlayClick();return clicked;
        }
        static void Label(float y,string name,string detail="")
        {GUI.Label(new Rect(375,y,700,32),name,Text(19,null,true));if(detail.Length>0)GUI.Label(new Rect(375,y+36,700,54),detail,Text(14,muted));}
        static float Slider(float y,string name,float value,float min=0,float max=1)
        {
            GUI.Label(new Rect(375,y,550,36),name,Text());GUI.Label(new Rect(995,y,100,36),Mathf.RoundToInt(value*100)+"%",Text(17,gold,true));
            return GUI.HorizontalSlider(new Rect(375,y+42,700,24),value,min,max);
        }
        static bool Toggle(float y,string name,bool value)
        {GUI.Label(new Rect(375,y+1,555,40),name,Text());if(Button(new Rect(950,y,125,38),value?"ATIVADO":"DESATIVADO",value,14)){GUI.changed=true;return !value;}return value;}
        public static void Draw(PlayerInputReader input,LevelProgress level)
        {
            if(!Visible)return;var oldColor=GUI.color;var oldMatrix=GUI.matrix;bool oldEnabled=GUI.enabled;
            try
            {
                float amount=UiTheme.MenuEase(visibility);GUI.color=oldColor*new Color(1,1,1,amount);
                GUI.color*=new Color(.015f,.025f,.035f,.82f);GUI.DrawTexture(new Rect(0,0,1280,720),Texture2D.whiteTexture);GUI.color=oldColor*new Color(1,1,1,amount);
                GUI.matrix=oldMatrix*Matrix4x4.Translate(new Vector3(0,GamePreferences.ReduceMotion?0:(1-amount)*22,0));
                CleanGui.Round(new Rect(125,53,1040,626),ink);CleanGui.Round(new Rect(145,138,194,516),UiTheme.Panel);
                GUI.Label(new Rect(157,76,550,42),"PONTO CEGO",Text(29,gold,true));GUI.Label(new Rect(157,116,620,28),"PAUSA  /  "+folders[ActiveFolder].ToUpperInvariant(),Text(13,muted));
                GUI.enabled=oldEnabled&&input.Paused&&!input.Rebinding;
                if(Button(new Rect(994,80,139,40),"CONTINUAR",false,14))input.SetPaused(false);
                for(int i=0;i<folders.Length;i++)
                {
                    var r=new Rect(157,165+i*64,170,49);bool selected=ActiveFolder==i;CleanGui.Round(new Rect(r.x+5,r.y-6,57,10),selected?gold:Color.Lerp(UiTheme.Panel,gold,.15f));
                    if(Button(r,folders[i].ToUpperInvariant(),selected,i==5?12:15))ActiveFolder=i;
                }
                GUI.Label(new Rect(157,565,172,62),"1–6 / TAB: ABAS\nLB / RB: ABAS\nESC: VOLTAR",Text(11,muted,true));
                GUI.Label(new Rect(375,161,700,44),folders[ActiveFolder],Text(29,null,true));CleanGui.Round(new Rect(375,211,700,2),Color.Lerp(UiTheme.Panel,gold,.2f));
                switch(ActiveFolder)
                {
                    case 0:
                        Label(233,level?level.chapter:"Cidade",level?level.title:"Explore Patos de Minas");GUI.Label(new Rect(375,326,700,84),level?level.Objective:"",Text(18));
                        if(Button(new Rect(375,430,700,48),"RETOMAR O JOGO",true))input.SetPaused(false);
                        if(Button(new Rect(375,489,340,45),"MAPA E OBJETIVOS",false,15)){input.SetPaused(false);input.GetComponent<PlayerPhone>()?.OpenApp(PhoneApp.Map);}
                        if(Button(new Rect(735,489,340,45),"RETOMAR CHECKPOINT",false,15))GameSession.Retry();
                        if(Button(new Rect(375,548,340,45),"MENU PRINCIPAL",false,15))GameSession.ReturnToMenu();
                        if(Button(new Rect(735,548,340,45),"SAIR DO JOGO",false,15)){GameSession.Save();Application.Quit();}break;
                    case 1:
                        GUI.Label(new Rect(375,235,700,58),input.Rebinding?"PRESSIONE UM CONTROLE • Esc cancela":"Selecione um comando para alterar a tecla ou botão.",Text(15,input.Rebinding?gold:muted));
                        int rows=0;foreach(var a in input.Actions)foreach(var b in a.bindings)if(!b.isComposite&&a.name!="Pointer")rows++;
                        controlsScroll=GUI.BeginScrollView(new Rect(375,300,720,260),controlsScroll,new Rect(0,0,690,rows*49));float y=0;
                        foreach(var action in input.Actions){if(action.name=="Pointer")continue;for(int i=0;i<action.bindings.Count;i++){if(action.bindings[i].isComposite)continue;var binding=action.bindings[i];GUI.Label(new Rect(0,y+4,280,42),ActionName(action.name)+(binding.isPartOfComposite?" / "+binding.name:""),Text(13));if(Button(new Rect(293,y,390,42),action.GetBindingDisplayString(i),false,14))input.BeginRebind(action.name,i);y+=49;}}
                        GUI.EndScrollView();if(Button(new Rect(375,580,700,43),"RESTAURAR CONTROLES PADRÃO",false,15))input.ResetBindings();break;
                    case 2:
                        GUI.Label(new Rect(375,236,700,36),"Ajuste cada grupo de som separadamente.",Text(15,muted));GUI.changed=false;
                        GamePreferences.Master=Slider(291,"Volume geral",GamePreferences.Master);GamePreferences.Music=Slider(388,"Música",GamePreferences.Music);GamePreferences.Effects=Slider(485,"Efeitos e ambiente",GamePreferences.Effects);if(GUI.changed)GamePreferences.Save();break;
                    case 3:
                        GUI.changed=false;bool fullscreen=Toggle(239,"Tela cheia",Screen.fullScreen);if(fullscreen!=Screen.fullScreen)Screen.fullScreen=fullscreen;
                        GUI.Label(new Rect(375,296,700,33),"Resolução",Text(19,null,true));int[] widths={1280,1600,1920};int[] heights={720,900,1080};for(int i=0;i<3;i++)if(Button(new Rect(375+i*238,338,224,42),widths[i]+" × "+heights[i],Screen.width==widths[i]&&Screen.height==heights[i],15))Screen.SetResolution(widths[i],heights[i],Screen.fullScreen);
                        bool vsync=Toggle(410,"Sincronização vertical",QualitySettings.vSyncCount>0);QualitySettings.vSyncCount=vsync?1:0;GUI.Label(new Rect(375,477,700,33),"Limite de quadros",Text(19,null,true));int[] fps={30,60,120,-1};for(int i=0;i<4;i++)if(Button(new Rect(375+i*178,521,165,42),fps[i]<0?"SEM LIMITE":fps[i]+" FPS",Application.targetFrameRate==fps[i],14))Application.targetFrameRate=fps[i];
                        GUI.Label(new Rect(375,588,700,35),"Atual: "+Screen.width+" × "+Screen.height,Text(14,muted));if(GUI.changed)GamePreferences.Save();break;
                    case 4:
                        GUI.changed=false;GamePreferences.Minimap=Toggle(236,"Minimapa e rota",GamePreferences.Minimap);GamePreferences.Objectives=Toggle(290,"Objetivos no cenário",GamePreferences.Objectives);GamePreferences.EnemyBars=Toggle(344,"Vida dos inimigos",GamePreferences.EnemyBars);GamePreferences.Clock=Toggle(398,"Relógio e clima",GamePreferences.Clock);GamePreferences.SmartHud=Toggle(452,"HUD discreto fora de perigo",GamePreferences.SmartHud);GamePreferences.AimSize=Slider(516,"Tamanho da mira",GamePreferences.AimSize,.8f,1.5f);if(GUI.changed)GamePreferences.Save();break;
                    case 5:
                        GUI.changed=false;GamePreferences.TextScale=Slider(236,"Tamanho dos textos",GamePreferences.TextScale,.9f,1.3f);
                        GUI.Label(new Rect(375,319,700,38),"Cores acessíveis",Text(19,null,true));
                        for(int i=0;i<colourNames.Length;i++)if(Button(new Rect(375+i*177,366,165,43),colourNames[i],GamePreferences.ColorMode==i,12)){GamePreferences.ColorMode=i;GUI.changed=true;}
                        GamePreferences.ReduceMotion=Toggle(441,"Reduzir animações da interface",GamePreferences.ReduceMotion);
                        GUI.Label(new Rect(375,495,700,56),"Cores acompanhadas de nomes e valores. A arte original dos personagens e do celular é preservada.",Text(14,muted));
                        CleanGui.Round(new Rect(375,577,330,28),UiTheme.Life);CleanGui.Round(new Rect(745,577,330,28),UiTheme.Stamina);
                        GUI.Label(new Rect(385,576,310,30),"VIDA  100 / 100",Text(14,Color.black,true));GUI.Label(new Rect(755,576,310,30),"FÔLEGO  100%",Text(14,Color.black,true));
                        if(GUI.changed)GamePreferences.Save();break;
                }
                GUI.Label(new Rect(375,642,700,25),"Preferências independentes do progresso da campanha.",Text(11,muted));
            }
            finally{GUI.matrix=oldMatrix;GUI.color=oldColor;GUI.enabled=oldEnabled;}
        }
        static string ActionName(string key)=>key=="Move"?"Movimento":key=="Aim"?"Mira":key=="Sprint"?"Correr":key=="Dodge"?"Esquivar":key=="Interact"?"Interagir":key=="Flash"?"Fotografar / flash":key=="Shoot"?"Disparar":key=="Melee"?"Golpe / corrente":key=="Heal"?"Curar":key=="Reload"?"Recarregar":key=="Phone"?"Celular":key=="Pause"?"Pausa":key=="Artifact"?"Artefato":key;
    }
}
