using System;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Dialogue owns the gameplay lock and advances objectives only after the last line.</summary>
    public sealed class StoryDialogue:MonoBehaviour
    {
        public string Speaker{get;private set;}
        public Texture2D Portrait{get;private set;}
        public bool Active=>lines!=null;
        public string CurrentLine=>Active?lines[Mathf.Min(index,lines.Length-1)]:"";
        public int LineNumber=>index+1;
        public int LineCount=>Active?lines.Length:0;
        string[] lines;int index;Action completed;PlayerInputReader input;
        void Awake(){input=GetComponent<PlayerInputReader>();}
        public void Begin(string speaker,Texture2D portrait,string[] text,Action onComplete=null)
        {
            if(Active)return;Speaker=speaker;Portrait=portrait;
            lines=text!=null&&text.Length>0?text:new[]{"..."};index=0;completed=onComplete;input.SetGameplayLocked(true);
        }
        public void Advance()
        {
            if(!Active)return;if(++index<lines.Length)return;
            lines=null;input.SetGameplayLocked(false);var callback=completed;completed=null;callback?.Invoke();
        }
        public void DismissForValidation(){while(Active)Advance();}
    }
}
