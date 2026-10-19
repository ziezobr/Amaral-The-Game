using System;
using UnityEngine;
namespace PontoCego3D
{
 [DefaultExecutionOrder(200)]public sealed class CityStartMenu:MonoBehaviour
 {
  public Texture2D art;bool active;ThirdPersonInput input;
  void Start(){if(Application.isBatchMode||Array.IndexOf(System.Environment.GetCommandLineArgs(),"--capture-city")>=0||Array.IndexOf(System.Environment.GetCommandLineArgs(),"--capture-3d")>=0){enabled=false;return;}input=FindAnyObjectByType<ThirdPersonInput>();active=true;input.SetPaused(true);}
  void OnGUI(){if(!active)return;GUI.depth=-100;float scale=Mathf.Min(Screen.width/1920f,Screen.height/1080f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1920*scale)/2,(Screen.height-1080*scale)/2),Quaternion.identity,Vector3.one*scale);if(art)GUI.DrawTexture(new Rect(0,0,1920,1080),art);if(GUI.Button(new Rect(805,567,310,73),GUIContent.none,GUIStyle.none)){active=false;input.SetPaused(false);}GUI.matrix=Matrix4x4.identity;}
 }
}
