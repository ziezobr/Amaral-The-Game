using UnityEngine;
namespace PontoCego
{
    public sealed class PlayerPhone:MonoBehaviour
    {
        public bool Active{get;private set;}PlayerInputReader input;bool choosing;Vector2 destinationsScroll,contentScroll;float visibility;GameHud hud;
        public float AnimationVisibility=>visibility;
        public bool Visible=>visibility>.001f&&input&&!input.Paused;
        void Awake(){input=GetComponent<PlayerInputReader>();}
        void Update(){var action=input.Actions.FindAction("Phone");if(action!=null&&action.WasPressedThisFrame())Toggle();visibility=UiTheme.StepVisibility(visibility,Active,Time.unscaledDeltaTime,GamePreferences.ReduceMotion);}
        public void Toggle()
        {
            if(Active){Close();return;}if(input.Paused||!GetComponent<Health>().Alive)return;
            foreach(var shop in FindObjectsByType<EquipmentShop>())if(shop.Active)shop.Close();
            foreach(var driver in FindObjectsByType<UberDriver>())if(driver.Active)driver.Close();
            if(GetComponent<StoryDialogue>().Active||input.GameplayLocked||TaxiService.Instance&&TaxiService.Instance.TripActive)return;
            Active=true;App=PhoneApp.Home;contentScroll=Vector2.zero;input.SetGameplayLocked(true);UiSound.PlayOpen();
        }
        public void Close(){if(!Active)return;Active=false;choosing=false;bool other=GetComponent<StoryDialogue>().Active||!GetComponent<Health>().Alive||TaxiService.Instance&&TaxiService.Instance.TripActive;foreach(var shop in FindObjectsByType<EquipmentShop>())other|=shop.Active;foreach(var puzzle in FindObjectsByType<EvidencePuzzle>())other|=puzzle.Active;input.SetGameplayLocked(other);}
        public PhoneApp App{get;private set;}
        public void OpenApp(PhoneApp app){if(!Active)Toggle();if(Active){App=app;contentScroll=Vector2.zero;}}
        void OnGUI()
        {
            if(!Visible)return;var old=GUI.matrix;var oldColor=GUI.color;int oldDepth=GUI.depth;bool oldEnabled=GUI.enabled;
            try
            {
            float amount=UiTheme.MenuEase(visibility);GUI.matrix=UiTheme.SafeMatrix()*Matrix4x4.Translate(new Vector3(GamePreferences.ReduceMotion?0:(1-amount)*65,0,0));GUI.color=oldColor*new Color(1,1,1,amount);GUI.enabled=oldEnabled&&Active&&!input.Paused;
            if(!hud)hud=FindAnyObjectByType<GameHud>();var skin=hud&&hud.art?hud.art.phone:null;var app=App;
            if(GamePhoneView.Frame(App==PhoneApp.Uber&&hud&&hud.art?hud.art.uber:skin,ref app,PontoCego.WorldEnvironment.DayNightCycle.Instance?PontoCego.WorldEnvironment.DayNightCycle.Instance.Clock:"16:00"))Close();if(App!=app)contentScroll=Vector2.zero;App=app;
            var content=GamePhoneView.Content;float y=content.y+22;var taxi=TaxiService.Instance;
            bool contentScrolling=App!=PhoneApp.Uber&&App!=PhoneApp.Home;
            if(contentScrolling){GUI.BeginGroup(new Rect(content.x,content.y+20,content.width,430));contentScroll=GUI.BeginScrollView(new Rect(0,0,content.width,430),contentScroll,new Rect(0,0,content.width-18,650));GamePhoneView.LocalContent=new Rect(0,0,content.width-20,650);y=0;}
            if(App==PhoneApp.Uber)
            {
                if(taxi&&string.IsNullOrEmpty(taxi.SelectedDestination))taxi.SelectDestination(GameSession.Current.mainMission);
                var destination=taxi&&WorldHub.Instance?WorldHub.Instance.Find(taxi.SelectedDestination):null;string state=!taxi||!taxi.Requested?"HIGOR DISPONÍVEL":taxi.Arrived?"HIGOR CHEGOU":"CHEGANDO: "+Mathf.CeilToInt(taxi.Remaining)+"s";
                bool select;bool request=UberPhoneView.Draw(hud&&hud.art?hud.art.uber:null,state,taxi?taxi.Fare:0,destination!=null?destination.level.title:"ESCOLHER DESTINO",out select);if(select)choosing=!choosing;
                if(request&&taxi){if(!taxi.Requested)taxi.RequestPickup();else if(taxi.Arrived)taxi.BeginRide();}
                if(taxi&&taxi.Requested){var info=GamePhoneView.Place(new Rect(765,595,395,37));GUI.Label(info,taxi.Arrived?"Aproxime-se da porta e pressione E.":"Higor está vindo em seu Peugeot 206 cinza.",new GUIStyle(GamePhoneView.Text){fontSize=UiTheme.Font(10)});}
                if(choosing&&WorldHub.Instance)
                {
                    var list=GamePhoneView.Place(new Rect(753,280,422,470));var contentColour=GUI.color;GUI.color*=UiTheme.Surface;GUI.DrawTexture(list,Texture2D.whiteTexture);GUI.color=contentColour;GUILayout.BeginArea(list);destinationsScroll=GUILayout.BeginScrollView(destinationsScroll);foreach(var mission in WorldHub.Instance.missions)if(GUILayout.Button(mission.level.title,GamePhoneView.ButtonStyle,GUILayout.Height(42))){taxi.SelectDestination(mission.id);choosing=false;UiSound.PlayClick();}GUILayout.EndScrollView();GUILayout.EndArea();
                }
                if(taxi&&taxi.Requested&&!taxi.TripActive&&GUI.Button(GamePhoneView.Place(new Rect(819,853,287,39)),"CANCELAR CHAMADA",GamePhoneView.ButtonStyle)){taxi.Cancel();UiSound.PlayClick();}

            }
            else if(App==PhoneApp.Map)
            {
                var gps=FindAnyObjectByType<CityNavigation>();if(gps){gps.DrawMap(new Rect(0,y,GamePhoneView.Content.width,190));y+=200;GamePhoneView.Message(ref y,gps.Objective+"\nDistância: "+Mathf.CeilToInt(gps.Distance)+" m",95);if(GamePhoneView.Button(ref y,"ROTA DA HISTÓRIA"))gps.FollowMateus=false;if(GamePhoneView.Button(ref y,"SERVIÇOS DO MATEUS"))gps.FollowMateus=true;}

            }
            else if(App==PhoneApp.Messages)
            {
                var level=hud?hud.level:null;GamePhoneView.Message(ref y,"OBJETIVO\n"+(level?level.Objective:"Explore a cidade."),150);GamePhoneView.Message(ref y,GoldenWeaponQuest.Status,85);var s=GameSession.Current;GamePhoneView.Message(ref y,"Peças do PC: "+PcCollectible.TypesCollected+"/6\nFlash: "+s.flashCharges+"\nMunição: "+s.magazine+" / "+s.ammoReserve+"\nKits: "+s.medkits+"\nSaldo: R$ "+s.currency,170);
            }
            else if(App==PhoneApp.Camera){GamePhoneView.Message(ref y,"Use a câmera para registrar provas. Mire com o mouse e fotografe com o botão esquerdo.",140);if(GamePhoneView.Button(ref y,"VOLTAR PARA FOTOGRAFAR"))Close();}
            else if(App==PhoneApp.News)GamePhoneView.Message(ref y,"Comunidade de Patos\n\nA praça, o Mocambo e os comércios estão abertos. Henzo negocia equipamentos e ajuda na montagem do PC.",210);
            else if(App==PhoneApp.Browser)GamePhoneView.Message(ref y,"Guia local\n\nHenzo: munição e melhorias.\nHigor: transporte e destinos.\nMocambo: quadra e desafio.\nRiberal: história principal.",230);
            else if(App==PhoneApp.Music)GamePhoneView.Message(ref y,"Player de música\n\nNenhuma faixa na biblioteca do aparelho.",120);
            else if(App==PhoneApp.Videos)GamePhoneView.Message(ref y,"Galeria de vídeos\n\nNenhum vídeo gravado. As provas fotográficas aparecem no seu progresso.",150);
            if(contentScrolling){GUI.EndScrollView();GUI.EndGroup();GamePhoneView.LocalContent=null;}
            }
            finally{GamePhoneView.LocalContent=null;GUI.depth=oldDepth;GUI.matrix=old;GUI.color=oldColor;GUI.enabled=oldEnabled;}
        }
    }
}
