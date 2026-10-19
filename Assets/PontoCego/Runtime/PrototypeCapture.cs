using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    public sealed class PrototypeCapture:MonoBehaviour
    {
        static bool running;string path;
        void Start()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--ponto-capture");
            if(index<0||index+1>=args.Length||running)return;
            running=true;path=args[index+1];DontDestroyOnLoad(gameObject);Application.runInBackground=true;StartCoroutine(Capture());
        }
        IEnumerator Frame(string name,Vector2 position,bool dismiss=true)
        {
            var player=FindAnyObjectByType<PlayerMotor>();player.Body.position=position;player.Body.linearVelocity=Vector2.zero;
            if(dismiss)player.GetComponent<StoryDialogue>().DismissForValidation();
            for(int i=0;i<60;i++){player.GetComponent<PlayerInputReader>().SetPaused(false);yield return null;}
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(path,name+".png"));for(int i=0;i<15;i++)yield return null;
        }
        IEnumerator Capture()
        {
            Directory.CreateDirectory(path);
            if(SceneManager.GetActiveScene().name!="01_Riberal")yield return SceneManager.LoadSceneAsync("01_Riberal");yield return null;
            yield return Frame("01_Riberal_entrada",new Vector2(-.36f,-6.84f));
            yield return Frame("02_Riberal_producao",new Vector2(-.4f,-2.5f));
            yield return Frame("03_Riberal_diretoria",new Vector2(9.8f,4.1f));
            StoryInteractable ribeiro=null;foreach(var item in FindObjectsByType<StoryInteractable>())if(item.displayName=="Ribeiro")ribeiro=item;
            FindAnyObjectByType<StoryDialogue>().Begin("Ribeiro",ribeiro.portrait,ribeiro.lines);
            yield return Frame("04_Ribeiro_dialogo",new Vector2(-.5f,-3.2f),false);
            yield return SceneManager.LoadSceneAsync("02_Gala");yield return null;
            yield return Frame("05_Gala",new Vector2(-5.04f,-1.4f));
            yield return Frame("06_Gala_maleta",new Vector2(7.1f,.4f));
            yield return SceneManager.LoadSceneAsync("04_Emboscada");yield return null;
            FindAnyObjectByType<LevelProgress>().RestoreStep(1);yield return null;
            var combatPlayer=FindAnyObjectByType<PlayerInputReader>();combatPlayer.GetComponent<Health>().Damage(24,CombatTeam.Enemy);combatPlayer.SetGameplayLocked(true);
            yield return Frame("07_Combate_Riberal",new Vector2(-.4f,-2.5f));
            combatPlayer.SetGameplayLocked(false);combatPlayer.Mobile(Vector2.right,Vector2.right,true,false,false,false);
            for(int i=0;i<22;i++)yield return new WaitForFixedUpdate();
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(path,"08_Folego_em_uso.png"));
            for(int i=0;i<15;i++)yield return null;
            yield return SceneManager.LoadSceneAsync("06_Oficina");yield return null;
            yield return Frame("09_Oficina",new Vector2(4,-.5f));
            yield return SceneManager.LoadSceneAsync("07_Distribuidora");yield return null;
            FindAnyObjectByType<LevelProgress>().RestoreStep(1);yield return null;FindAnyObjectByType<PlayerInputReader>().SetGameplayLocked(true);
            yield return Frame("10_Distribuidora",new Vector2(-5,-5));
            yield return SceneManager.LoadSceneAsync("13_Estrada");yield return null;
            FindAnyObjectByType<PlayerInputReader>().SetGameplayLocked(true);
            yield return Frame("11_Ninja_300",new Vector2(-600,0));
            yield return SceneManager.LoadSceneAsync("15_Cenoura");yield return null;
            FindAnyObjectByType<LevelProgress>().RestoreStep(2);yield return null;FindAnyObjectByType<PlayerInputReader>().SetGameplayLocked(true);
            yield return Frame("12_Cenoura",new Vector2(0,5));
            yield return SceneManager.LoadSceneAsync("23_Amanhecer");yield return null;
            yield return Frame("13_Amanhecer",new Vector2(-12,-11));
            Debug.Log("PONTO_CEGO_URBAN_CAPTURE_OK");Application.Quit(0);
        }
    }
}
