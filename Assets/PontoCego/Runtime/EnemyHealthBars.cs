using UnityEngine;
namespace PontoCego
{
    /// <summary>Visible enemies keep labelled health bars in every accessibility palette.</summary>
    public sealed class EnemyHealthBars:MonoBehaviour
    {
        PlayerInputReader player;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Install(){UnityEngine.SceneManagement.SceneManager.sceneLoaded+=(_,__)=>Ensure();Ensure();}
        static void Ensure(){if(!FindAnyObjectByType<EnemyHealthBars>())new GameObject("Barras de vida dos inimigos").AddComponent<EnemyHealthBars>();}
        void OnGUI()
        {
            if(!GamePreferences.EnemyBars)return;if(!player)player=FindAnyObjectByType<PlayerInputReader>();if(player&&(player.Paused||player.GameplayLocked))return;var cam=Camera.main;if(!cam)return;Color previous=GUI.color;
            try
            {
                float scale=Mathf.Clamp(Mathf.Min(Screen.width/1280f,Screen.height/720f),.6f,2f);
                foreach(var enemy in FindObjectsByType<EnemyAgent>())
                {
                    var hp=enemy.GetComponent<Health>();if(!hp||!hp.Alive)continue;
                    var v=cam.WorldToViewportPoint(enemy.transform.position+Vector3.up*2.5f);if(v.z<0||v.x<0||v.x>1||v.y<0||v.y>1)continue;
                    float w=(hp.boss?116:58)*scale,h=7*scale;var rect=new Rect(v.x*Screen.width-w/2,(1-v.y)*Screen.height,w,h);
                    GUI.color=previous*new Color(.015f,.02f,.03f,.95f);GUI.DrawTexture(rect,Texture2D.whiteTexture);
                    GUI.color=previous*(hp.boss?UiTheme.Accent:UiTheme.Life);GUI.DrawTexture(new Rect(rect.x+scale,rect.y+scale,(w-2*scale)*Mathf.Clamp01(hp.Current/hp.maximum),h-2*scale),Texture2D.whiteTexture);GUI.color=previous;
                    if(hp.boss||Time.time-hp.LastDamageTime<4)
                    {
                        string value=Mathf.CeilToInt(hp.Current)+" / "+Mathf.CeilToInt(hp.maximum);
                        var label=new GUIStyle(UiTheme.Label(10,Color.white,true)){fontSize=Mathf.RoundToInt(UiTheme.Font(10)*scale),alignment=TextAnchor.MiddleCenter,wordWrap=false};
                        var textRect=new Rect(rect.center.x-90*scale,rect.y-20*scale,180*scale,20*scale);var shadow=textRect;shadow.position+=Vector2.one*scale;GUI.Label(shadow,value,new GUIStyle(label){normal={textColor=Color.black}});GUI.Label(textRect,value,label);
                    }
                }
            }
            finally{GUI.color=previous;}
        }
    }
}
