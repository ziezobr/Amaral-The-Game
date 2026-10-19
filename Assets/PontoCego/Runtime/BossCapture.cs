using System;using System.Collections;using System.IO;using UnityEngine;
namespace PontoCego
{
    /// <summary>Prepared, opt-in player captures of actual boss telegraphs. Never writes a game save.</summary>
    public sealed class BossCapture:MonoBehaviour
    {
        string path;BossAbilities boss;Health hero;PlayerInputReader input;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Bootstrap()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--ponto-boss-capture");if(at<0||at+1>=args.Length)return;
            var go=new GameObject("Capturas de chefes / QA");DontDestroyOnLoad(go);go.AddComponent<BossCapture>().path=args[at+1];
        }
        IEnumerator Start()
        {
            Application.runInBackground=true;Directory.CreateDirectory(path);yield return null;
            yield return Load("15_Cenoura",2);yield return Until(()=>boss.Charging);
            yield return new WaitForSeconds(.18f);yield return Shot("01_Cenoura_investida");
            yield return Until(()=>boss.Recovering);yield return Shot("02_Cenoura_vulneravel");
            yield return Until(()=>boss.Charging);yield return new WaitForSeconds(.15f);yield return Shot("03_Cenoura_impacto");
            yield return Until(()=>boss.Recovering);Vector2 aim=(boss.transform.position-input.transform.position).normalized;input.Mobile(Vector2.zero,aim,false,false,false,false,true);
            yield return new WaitForSeconds(.17f);yield return Shot("04_Amaral_golpe_animado");
            yield return Load("20_Paulo",3);var hp=boss.GetComponent<Health>();hp.Damage(hp.maximum*.55f,CombatTeam.Player);yield return Until(()=>boss.Charging);
            yield return new WaitForSeconds(.3f);yield return Shot("05_Paulo_holofotes_fase2");
            yield return Until(()=>boss.Recovering);yield return new WaitForSeconds(.13f);yield return Shot("06_Paulo_sobrecarga");
            Debug.Log("PONTO_CEGO_BOSS_CAPTURE_OK");Application.Quit();
        }
        IEnumerator Load(string scene,int step)
        {
            GameSession.EncounterDemo(scene,step);yield return null;yield return null;
            boss=FindAnyObjectByType<BossAbilities>();boss.GetComponent<EnemyAgent>().speed=0;
            input=FindAnyObjectByType<PlayerInputReader>();hero=input.GetComponent<Health>();
            foreach(var ally in FindObjectsByType<AllyAgent>())ally.gameObject.SetActive(false);
            foreach(var enemy in FindObjectsByType<EnemyAgent>())if(enemy.gameObject!=boss.gameObject)enemy.gameObject.SetActive(false);
            input.Mobile(Vector2.zero,Vector2.up,false,false,false,false);yield return new WaitForSeconds(.3f);
        }
        void Update(){if(hero)hero.Restore();}
        IEnumerator Until(Func<bool> condition){float deadline=Time.time+15;while(!condition()&&Time.time<deadline)yield return null;if(!condition())Debug.LogError("QA: Boss state timed out.");}
        IEnumerator Shot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(path,name+".png"));yield return null;}
    }
}
