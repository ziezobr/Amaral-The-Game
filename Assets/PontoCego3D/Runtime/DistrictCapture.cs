using System;using System.IO;using System.Collections;using UnityEngine;
namespace PontoCego3D
{
    /// <summary>Opt-in rendered-player validation. Never enables itself during an ordinary game.</summary>
    public sealed class DistrictCapture:MonoBehaviour
    {
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--capture-3d");if(at<0||at+1>=args.Length)yield break;
            string path=args[at+1];Directory.CreateDirectory(path);var world=FindAnyObjectByType<OpenDistrict>();world.Capturing=true;var hero=world.player;hero.orbit.Manual=true;hero.Locked=true;
            yield return new WaitForSeconds(2);
            yield return Frame(path,"01_Rua",new Vector3(7,.3f,-15),0,12);
            yield return Frame(path,"02_Amaral",new Vector3(7,.3f,-15),180,6);
            yield return Frame(path,"03_Riberal",new Vector3(22,.3f,7),0,9);
            yield return Frame(path,"04_Padaria",new Vector3(-22,.3f,-5),180,10);
            foreach(var enemy in world.encounter)enemy.SetActive(true);
            yield return Frame(path,"05_Combate",new Vector3(63,.3f,13),0,16);
            hero.GetComponent<CombatActor>().StartMelee("Cross",26);yield return new WaitForSeconds(.3f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(path,"06_Golpe.png"));
            yield return new WaitForSeconds(.5f);Debug.Log("PONTO_CEGO_3D_CAPTURE_OK");Application.Quit();
        }
        IEnumerator Frame(string path,string name,Vector3 at,float yaw,float pitch)
        {
            var hero=FindAnyObjectByType<ThirdPersonMotor>();hero.Warp(at);hero.orbit.yaw=yaw;hero.orbit.pitch=pitch;hero.orbit.Snap();
            yield return new WaitForSeconds(1);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(path,name+".png"));yield return new WaitForSeconds(.4f);
        }
    }
}
