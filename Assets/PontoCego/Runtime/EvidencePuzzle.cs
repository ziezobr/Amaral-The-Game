using UnityEngine;
using UnityEngine.InputSystem;
namespace PontoCego
{
    public sealed class EvidencePuzzle:MonoBehaviour,IInteractable
    {
        public string question;public string[] options;public int correctIndex,requiredStep;
        public bool Active{get;private set;}public string Notice{get;private set;}="";
        public string Prompt=>"Cruzar as pistas";
        public int Selected{get;private set;}
        PlayerInputReader input;float openedAt;
        public void Interact()
        {
            var level=FindAnyObjectByType<LevelProgress>();if(level.Step!=requiredStep)return;
            input=FindAnyObjectByType<PlayerInputReader>();input.SetGameplayLocked(true);Active=true;openedAt=Time.unscaledTime;
        }
        public void Submit(int index)
        {
            if(!Active)return;
            if(index!=correctIndex){Notice="Essa conclusão não bate com os registros. Releia as pistas.";return;}
            Close();var level=FindAnyObjectByType<LevelProgress>();GameSession.AddEvidence(level.title+" / conclusão\n"+options[index]);level.Advance();
        }
        public void Close(){Active=false;if(input)input.SetGameplayLocked(false);}
        void Update()
        {
            if(!Active||input.Paused||Time.unscaledTime-openedAt<.2f)return;
            var keyboard=Keyboard.current;
            if(keyboard!=null){if(keyboard.digit1Key.wasPressedThisFrame)Submit(0);if(keyboard.digit2Key.wasPressedThisFrame)Submit(1);if(keyboard.digit3Key.wasPressedThisFrame)Submit(2);}
            var pad=Gamepad.current;if(pad==null)return;
            if(pad.dpad.down.wasPressedThisFrame)Selected=(Selected+1)%options.Length;if(pad.dpad.up.wasPressedThisFrame)Selected=(Selected+options.Length-1)%options.Length;
            if(pad.buttonSouth.wasPressedThisFrame)Submit(Selected);if(pad.buttonEast.wasPressedThisFrame)Close();
        }
    }
}
