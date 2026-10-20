using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace PontoCego
{
    public sealed class ContinuousWorldCapture:MonoBehaviour
    {
        string folder;bool capturingPause;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Launch()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--ponto-world-capture");if(index<0||index+1>=args.Length)return;
            var go=new GameObject("Verificação visual / cidade contínua");DontDestroyOnLoad(go);go.AddComponent<ContinuousWorldCapture>().folder=args[index+1];
        }
        void Update(){var input=FindAnyObjectByType<PlayerInputReader>();if(input){if(input.Paused&&!capturingPause)input.SetPaused(false);input.GetComponent<Health>().Restore();}}
        IEnumerator Walk(Vector2 target)
        {
            var player=FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();float deadline=Time.time+35;
            while(Vector2.Distance(player.transform.position,target)>.65f&&Time.time<deadline&&WorldHub.Instance.Active==null)
            {input.Mobile((target-(Vector2)player.transform.position).normalized,Vector2.down,true,false,false,false);yield return null;}
            input.Mobile(Vector2.zero,Vector2.down,false,false,false,false);
            if(Time.time>=deadline)throw new InvalidOperationException("Walking route blocked at "+player.transform.position+" toward "+target);
        }
        IEnumerator Shot(string name){yield return new WaitForSeconds(.5f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSeconds(.4f);}
        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--polish-review")>=0){yield return Polish021Review.Run(folder);Application.Quit();yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--quick-art-review")>=0)
            {
                GameSession.ExploreNeighborhood("01_Riberal");yield return new WaitForSeconds(2);
                var quickPlayer=FindAnyObjectByType<PlayerMotor>();var quickCamera=FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
                var quickClock=WorldEnvironment.DayNightCycle.Instance;quickClock.frozen=true;quickClock.SetHour(12);WorldWeather2D.Instance.automaticWeather=false;WorldWeather2D.Instance.SetWeather(WeatherKind.Clear,true);
                var sites=new[]{new Vector2(-82,0),new Vector2(32,0),new Vector2(-64,-55),new Vector2(-57,-73),new Vector2(0,54),Mateus.CapturePosition,new Vector2(-37,-20)};
                var names=new[]{"01_Agencia_Riberal_HD","02_Oficina_Henzo_HD","03_Mocambo_quadra_HD","04_Portal_Mocambo_HD","05_Ferro_velho_HD","06_Mateus_escondido_HD","07_Cidade_mais_densa"};
                var zooms=new[]{22f,28f,12f,10f,13f,7f,9f};
                for(int site=0;site<sites.Length;site++){quickPlayer.Body.position=sites[site];quickPlayer.transform.position=sites[site];quickPlayer.Body.linearVelocity=Vector2.zero;quickCamera.Lens.OrthographicSize=zooms[site];yield return new WaitForSeconds(1.5f);yield return Shot(names[site]);}
                Debug.Log("PONTO_ART_REVIEW_OK");yield return new WaitForSeconds(1);Application.Quit();yield break;
            }
            yield return Shot("00_Menu_usuario");GameSession.ExploreNeighborhood("01_Riberal");yield return new WaitForSeconds(2);
            var world=WorldHub.Instance;var originalPlayer=FindAnyObjectByType<PlayerMotor>();var originalCamera=Camera.main;
            yield return Shot("01_Cidade_continua");var shop=FindAnyObjectByType<EquipmentShop>();shop.Interact();yield return Shot("01b_Loja_Henzo");shop.Close();var phone=originalPlayer.GetComponent<PlayerPhone>();phone.Toggle();yield return Shot("01c_Celular_inicio");phone.OpenApp(PhoneApp.Uber);TaxiService.Instance.RequestPickup();yield return Shot("01d_Celular_Uber");phone.Close();TaxiService.Instance.Cancel();
            foreach(var point in new[]{new Vector2(-10.88f,-23),new Vector2(-10.88f,-30),new Vector2(-37,-30),new Vector2(-37,-20),new Vector2(-60,-20),world.Find("01_Riberal").entry}){yield return Walk(point);if(point==new Vector2(-37,-20))yield return Shot("01e_Ruas_entre_missoes");}
            yield return new WaitForSeconds(1);yield return Shot("02_Entrada_Riberal_sem_carregamento");
            if(world.Active==null||FindAnyObjectByType<PlayerMotor>()!=originalPlayer||Camera.main!=originalCamera)throw new InvalidOperationException("World identity changed on mission entry");
            FindAnyObjectByType<StoryDialogue>().DismissForValidation();world.Complete("02_Gala");yield return Shot("03_Missao_concluida_mesma_cidade");
            var player=FindAnyObjectByType<PlayerMotor>();
            foreach(var block in FindObjectsByType<Transform>())if(block.name=="Cidade / quarteirão da rota / 0"){player.Body.position=(Vector2)block.position+new Vector2(5,-1);player.transform.position=player.Body.position;yield return new WaitForSeconds(2);yield return Shot("03b_Novos_quarteiroes");break;}
            world.Navigate("24_Mocambo");player.Body.position=world.Find("24_Mocambo").entry;player.transform.position=player.Body.position;
            yield return new WaitForSeconds(2);yield return Shot("04_Mocambo_integrado");
            world.Active.level.RestoreStep(4);yield return new WaitForSeconds(1);yield return Shot("05_Combate_no_mundo");
            FindAnyObjectByType<StoryDialogue>().DismissForValidation();world.Complete("02_Gala");
            var clock=WorldEnvironment.DayNightCycle.Instance;clock.frozen=true;clock.SetHour(12);yield return Shot("06_Cidade_dia");clock.SetHour(21);yield return Shot("07_Cidade_noite");clock.SetHour(16);
            player.Body.position=Mateus.CapturePosition;player.transform.position=player.Body.position;yield return new WaitForSeconds(2);yield return Shot("08_Mateus");
            world.Navigate("13_Estrada");player.Body.position=world.Find("13_Estrada").entry;player.transform.position=player.Body.position;yield return new WaitForSeconds(1);FindAnyObjectByType<StoryDialogue>().DismissForValidation();yield return new WaitForSeconds(2);yield return Shot("09_Perseguicao");
            var input=player.GetComponent<PlayerInputReader>();capturingPause=true;input.SetPaused(true);for(int tab=0;tab<6;tab++){PauseMenuView.ActiveFolder=tab;yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"10_Pausa_pasta_"+tab+".png"));yield return new WaitForSecondsRealtime(.3f);}GamePreferences.TextScale=1.3f;GamePreferences.ColorMode=2;PauseMenuView.ActiveFolder=5;yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"11_Acessibilidade_texto130.png"));yield return new WaitForSecondsRealtime(.4f);GamePreferences.TextScale=1;GamePreferences.ColorMode=0;PauseMenuView.ActiveFolder=0;input.SetPaused(false);capturingPause=false;
            world.Complete("02_Gala");player.Body.position=new Vector2(-60,70);player.transform.position=player.Body.position;yield return new WaitForSeconds(2);clock.SetHour(12);WorldWeather2D.Instance.automaticWeather=false;WorldWeather2D.Instance.SetWeather(WeatherKind.Clear,true);yield return Shot("12_Ruinas_dia");
            player.Body.position=new Vector2(-52,-72);player.transform.position=player.Body.position;yield return new WaitForSeconds(2);clock.SetHour(5);WorldWeather2D.Instance.SetWeather(WeatherKind.Cloudy,true);yield return Shot("13_Madrugada_trilha");clock.SetHour(21);WorldWeather2D.Instance.SetWeather(WeatherKind.Storm,true);yield return new WaitForSeconds(2);yield return Shot("14_Tempestade_noite");
            var originalText=GamePreferences.TextScale;GamePreferences.TextScale=1.3f;phone.OpenApp(PhoneApp.Messages);yield return Shot("15_Celular_texto130");phone.Close();GamePreferences.TextScale=originalText;
            player.Body.position=new Vector2(-60,-54);player.transform.position=player.Body.position;clock.SetHour(12);WorldWeather2D.Instance.SetWeather(WeatherKind.Clear,true);yield return new WaitForSeconds(2);yield return Shot("16_Quadra_Mocambo");player.Body.position=new Vector2(0,54);player.transform.position=player.Body.position;yield return new WaitForSeconds(2);yield return Shot("17_Ferro_velho_reconstruido");
            var virtualCamera=FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();float zoom=virtualCamera.Lens.OrthographicSize;virtualCamera.Lens.OrthographicSize=22;player.Body.position=new Vector2(-82,0);player.transform.position=player.Body.position;yield return new WaitForSeconds(2);yield return Shot("18_Agencia_fachada_HD");virtualCamera.Lens.OrthographicSize=28;player.Body.position=new Vector2(32,0);player.transform.position=player.Body.position;yield return new WaitForSeconds(2);yield return Shot("19_Oficina_Henzo_HD");virtualCamera.Lens.OrthographicSize=zoom;
            Debug.Log("PONTO_WORLD_CAPTURE_OK / same player and camera / real walking route verified");yield return new WaitForSeconds(1);Application.Quit();
        }
    }
}
