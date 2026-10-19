using UnityEngine;
namespace PontoCego
{
    /// <summary>Diegetic phase selection; destination travel uses the same checkpoint path as normal entrances.</summary>
    public sealed class UberDriver:MonoBehaviour,IInteractable
    {
        public string Prompt=>"Higor • motorista de Uber / escolher destino";
        public bool Active{get;private set;}PlayerInputReader input;Vector2 scroll;
        static readonly string[] scenes={"25_Bairro","01_Riberal","02_Gala","03_Casa","04_Emboscada","05_ResgateMartins","06_Oficina","07_Distribuidora","08_Patio","09_RastroCenoura","10_ArquivoCargas","11_OficinaRotas","12_Transferencia","13_Estrada","14_FerroVelho","15_Cenoura","16_Retorno","17_RiberalTerreo","18_RiberalProducao","19_Servico","20_Paulo","21_Adryan","22_Fuga","23_Amanhecer","24_Mocambo"};
        static readonly string[] titles={"Bairro aberto / Henzo e trabalhos","Agência Riberal / Prólogo","Gala / O click fatal","Casa / A porta arrombada","Riberal / Emboscada","Rua / O resgate de Martins","Oficina / Encontro com Henzo","Distribuidora / Notas frias","Pátio / Carregamento","O rastro do Cenoura","Arquivo de cargas","Oficina / Cruzando as rotas","Transferência clandestina","Estrada / Perseguição","Ferro-velho / O cerco","Cenoura / Chefe","Retorno à agência","Riberal / Térreo","Riberal / Produção","Elevador de serviço / Henzo","Paulo / Chefe final","Adryan / O resgate","Fuga do incêndio","Amanhecer / Padaria","Parque do Mocambo / Pivô"};
        public static string[] Destinations=>(string[])scenes.Clone();public static string TitleFor(string id){int index=System.Array.IndexOf(scenes,id);return index<0?id:titles[index];}
        public void Interact(){if(WorldHub.Instance){FindAnyObjectByType<PlayerPhone>()?.OpenApp(PhoneApp.Uber);return;}input=FindAnyObjectByType<PlayerInputReader>();input.SetGameplayLocked(true);Active=true;}
        public void Close(){Active=false;if(input)input.SetGameplayLocked(false);if(WorldHub.Instance){var phone=FindAnyObjectByType<PlayerPhone>();if(phone&&phone.Active)phone.Close();}}
        void Update(){if(Active&&input&&input.Paused){input.SetPaused(false);Close();}}
        public void Ride(string scene)
        {
            if(WorldHub.Instance){WorldHub.Instance.Navigate(scene);FindAnyObjectByType<PlayerPhone>()?.Close();Close();return;}
            // Only destinations present in this installed build can be selected.
            if(!Application.CanStreamedLevelBeLoaded(scene))return;
            Close();if(scene=="25_Bairro")GameSession.Travel(scene);else{if(scene!="24_Mocambo")GameSession.Current.mainMission=scene;GameSession.EnterMission(scene);}
        }
        void OnGUI()
        {
            if(!Active)return;var old=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1280f,Screen.height/720f,1));
            GUI.color=new Color(0,0,0,.85f);GUI.DrawTexture(new Rect(0,0,1280,720),Texture2D.whiteTexture);GUI.color=Color.white;
            var title=new GUIStyle(GUI.skin.label){fontSize=30,fontStyle=FontStyle.Bold};var button=new GUIStyle(GUI.skin.button){fontSize=21,padding=new RectOffset(14,14,10,10)};
            GUI.Box(new Rect(250,55,780,610),"");GUI.Label(new Rect(285,75,700,45),"HIGOR / PARA ONDE VAMOS?",title);
            GUI.Label(new Rect(285,122,700,30),"Escolha uma fase. A rota será marcada no mapa. Vá até o ponto para iniciar a missão.");
            GUILayout.BeginArea(new Rect(285,165,710,410));scroll=GUILayout.BeginScrollView(scroll);
            for(int i=0;i<scenes.Length;i++)
            {
                string scene=scenes[i];
                if(!Application.CanStreamedLevelBeLoaded(scene))continue;
                string label=titles[i];
                if(GUILayout.Button(label,button))Ride(scene);
            }
            GUILayout.EndScrollView();GUILayout.EndArea();if(GUI.Button(new Rect(285,593,710,43),"CANCELAR CORRIDA",button))Close();GUI.matrix=old;
        }
    }
}
