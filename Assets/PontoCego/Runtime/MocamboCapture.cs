using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Opt-in visual verification in the real standalone player.</summary>
    public sealed class MocamboCapture:MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Launch()
        {
            string[] args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--ponto-mocambo-capture");
            if(index<0||index+1>=args.Length)return;
            var go=new GameObject("Mocambo visual verification");DontDestroyOnLoad(go);go.AddComponent<MocamboCapture>().folder=args[index+1];
        }
        string folder;
        void Update(){var input=FindAnyObjectByType<PlayerInputReader>();if(input){if(input.Paused)input.SetPaused(false);input.GetComponent<Health>().Restore();}}
        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);yield return null;GameSession.ExploreNeighborhood();yield return new WaitForSeconds(3);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"00_Bairro_Aberto.png"));yield return new WaitForSeconds(.5f);
            var uber=FindAnyObjectByType<UberDriver>();uber.Interact();yield return new WaitForSeconds(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"00_Uber_Seletor.png"));yield return new WaitForSeconds(.5f);uber.Close();
            var shop=FindAnyObjectByType<EquipmentShop>();shop.Interact();yield return new WaitForSeconds(.4f);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"00_Henzo_Municao.png"));yield return new WaitForSeconds(.5f);shop.Close();
            var car=FindAnyObjectByType<DriveableCar>();car.Enter();FindAnyObjectByType<PlayerInputReader>().Mobile(Vector2.right,Vector2.right,false,false,false,false);
            yield return new WaitForSeconds(.7f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"00_Veiculo.png"));yield return new WaitForSeconds(.5f);car.Exit();
            GameSession.EncounterDemo("24_Mocambo",0);
            yield return new WaitForSeconds(2);var player=FindAnyObjectByType<PlayerMotor>();
            player.Body.position=new Vector2(7.6f,-11.6f);yield return new WaitForSeconds(2);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"01_Entrada_Placa.png"));yield return new WaitForSeconds(.5f);
            GameSession.EncounterDemo("24_Mocambo",4);yield return new WaitForSeconds(1);
            foreach(var ally in FindObjectsByType<AllyAgent>())ally.gameObject.SetActive(false);
            var boss=FindAnyObjectByType<BossAbilities>();boss.GetComponent<EnemyAgent>().speed=0;
            float deadline=Time.time+7;while(!boss.Charging&&Time.time<deadline)yield return null;
            yield return new WaitForSeconds(.25f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"02_Pivo_Arremesso.png"));
            yield return new WaitForSeconds(.5f);boss.GetComponent<Health>().Damage(450,CombatTeam.Player);
            yield return new WaitForSeconds(.8f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"03_Quadra_Fase2.png"));
            yield return new WaitForSeconds(1);Debug.Log("PONTO_MOCAMBO_CAPTURE_OK");Application.Quit();
        }
    }
}
