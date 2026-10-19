using UnityEngine;using UnityEngine.InputSystem;
namespace PontoCego3D
{
    /// <summary>Owns a remappable action map. Mouse/gamepad share gameplay semantics.</summary>
    public sealed class ThirdPersonInput:MonoBehaviour
    {
        public InputActionAsset template;public InputActionMap Actions{get;private set;}
        InputActionAsset instance;public bool Paused{get;private set;}
        public Vector2 Move=>Vector2.ClampMagnitude(Actions["Move"].ReadValue<Vector2>(),1);
        public bool Aim=>Actions["Aim"].IsPressed();public bool Sprint=>Actions["Sprint"].IsPressed();
        public bool Down(string name)=>Actions[name].WasPressedThisFrame();
        void Awake(){instance=Instantiate(template);Actions=instance.FindActionMap("ThirdPerson",true);if(PlayerPrefs.HasKey("PontoCego3D.Bindings.v1"))instance.LoadBindingOverridesFromJson(PlayerPrefs.GetString("PontoCego3D.Bindings.v1"));Actions.Enable();}
        void Update(){if(Down("Pause"))SetPaused(!Paused);}
        public void SetPaused(bool paused){Paused=paused;Time.timeScale=paused?0:1;bool iso=GetComponent<ThirdPersonMotor>()&&GetComponent<ThirdPersonMotor>().orbit&&GetComponent<ThirdPersonMotor>().orbit.isometric;Cursor.lockState=paused||iso?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=paused||iso;}
        void Start()=>SetPaused(false);
        void OnDestroy(){if(instance){Actions.Disable();Destroy(instance);}Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        public void Rebind(string action,int binding)
        {
            Actions.Disable();var target=Actions[action];target.PerformInteractiveRebinding(binding).WithCancelingThrough("<Keyboard>/escape").WithControlsExcluding("<Mouse>/position").WithControlsExcluding("<Mouse>/delta")
            .OnComplete(op=>{op.Dispose();PlayerPrefs.SetString("PontoCego3D.Bindings.v1",instance.SaveBindingOverridesAsJson());PlayerPrefs.Save();Actions.Enable();})
            .OnCancel(op=>{op.Dispose();Actions.Enable();}).Start();
        }
        public void ResetBindings(){instance.RemoveAllBindingOverrides();PlayerPrefs.DeleteKey("PontoCego3D.Bindings.v1");}
    }
}
