using System;using System.IO;using System.Collections;using UnityEngine;using PontoCego.WorldEnvironment;
namespace PontoCego3D
{
 public sealed class CityEnvironmentCapture:MonoBehaviour
 {
  IEnumerator Start(){var args=System.Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--capture-city");if(i<0||i+1>=args.Length)yield break;string folder=args[i+1];Directory.CreateDirectory(folder);yield return new WaitForSeconds(2);var hero=FindAnyObjectByType<ThirdPersonMotor>();hero.Locked=true;hero.orbit.Manual=false;DayNightCycle.Instance.frozen=true;foreach(float hour in new[]{12f,18.3f,0f}){DayNightCycle.Instance.SetHour(hour);hero.Warp(new Vector3(3,.3f,-10));hero.orbit.Snap();yield return new WaitForSeconds(2);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Centro_"+hour.ToString("0")+"h.png"));yield return new WaitForSeconds(.4f);}hero.Warp(new Vector3(-20,.4f,16));hero.orbit.Snap();DayNightCycle.Instance.SetHour(16);yield return new WaitForSeconds(2);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"Mocambo_3D.png"));yield return new WaitForSeconds(1);Debug.Log("PONTO_CITY_DAY_NIGHT_CAPTURE_OK");Application.Quit();}
 }
}
