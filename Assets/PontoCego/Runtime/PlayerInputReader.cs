using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PontoCego
{
    /// <summary>Owns a private action instance and persisted binding overrides. Mobile injects intent only.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        public InputActionAsset bindings;
        public InputActionAsset Actions { get; private set; }
        public Vector2 Move => mobileActive ? mobileMove : Actions["Move"].ReadValue<Vector2>();
        public bool Sprint => mobileActive ? mobileSprint : Actions["Sprint"].IsPressed();
        public bool Paused { get; private set; }
        public bool GameplayLocked { get; private set; }
        public bool Rebinding => rebind != null;
        public bool CursorForInterface => Paused || GameplayLocked || (cursorHealth && !cursorHealth.Alive);
        Health cursorHealth;bool quitting;
        public bool FlashHeld=>Actions!=null&&Actions["Flash"].IsPressed();
        public bool MeleeHeld=>Actions!=null&&Actions.FindAction("Melee")!=null&&Actions["Melee"].IsPressed();
        public event Action PauseChanged;
        bool dodgeQueued, interactQueued, flashQueued, meleeQueued, healQueued;
        bool mobileActive, mobileSprint;
        Vector2 mobileMove, mobileAim;
        InputActionRebindingExtensions.RebindingOperation rebind;
        const string SaveKey = "PontoCego.Bindings.v1";

        public static InputActionAsset CreateDefaults()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Gameplay");
            asset.AddActionMap(map);
            var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Gamepad>/dpad");
            map.AddAction("Aim", InputActionType.Value, "<Gamepad>/rightStick", expectedControlLayout: "Vector2");
            map.AddAction("Pointer", InputActionType.Value, "<Mouse>/position", expectedControlLayout: "Vector2");
            map.AddAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift").AddBinding("<Gamepad>/leftStickPress");
            map.AddAction("Dodge", InputActionType.Button, "<Keyboard>/space").AddBinding("<Gamepad>/buttonEast");
            map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e").AddBinding("<Gamepad>/buttonSouth");
            map.AddAction("Flash", InputActionType.Button, "<Mouse>/leftButton").AddBinding("<Gamepad>/rightTrigger");
            map.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape").AddBinding("<Gamepad>/start");
            map.AddAction("Melee",InputActionType.Button,"<Keyboard>/f").AddBinding("<Gamepad>/buttonWest");
            map.AddAction("Heal",InputActionType.Button,"<Keyboard>/h").AddBinding("<Gamepad>/buttonNorth");
            map.AddAction("Shoot",InputActionType.Button,"<Mouse>/rightButton").AddBinding("<Gamepad>/rightShoulder");
            map.AddAction("Reload",InputActionType.Button,"<Keyboard>/r").AddBinding("<Gamepad>/leftShoulder");
            map.AddAction("Artifact",InputActionType.Button,"<Keyboard>/g").AddBinding("<Gamepad>/leftTrigger");
            map.AddAction("Phone",InputActionType.Button,"<Keyboard>/t").AddBinding("<Gamepad>/select");
            return asset;
        }

        void Awake()
        {
            cursorHealth=GetComponent<Health>();
            Actions = bindings ? Instantiate(bindings) : CreateDefaults();
            if (PlayerPrefs.HasKey(SaveKey))
            {
                try { Actions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(SaveKey)); }
                catch (Exception) { PlayerPrefs.DeleteKey(SaveKey); }
            }
            Actions["Dodge"].performed += _ => dodgeQueued = true;
            Actions["Interact"].performed += _ => interactQueued = true;
            Actions["Flash"].performed += _ => flashQueued = true;
            var melee=Actions.FindAction("Melee");if(melee!=null)melee.performed+=_=>meleeQueued=true;
            var heal=Actions.FindAction("Heal");if(heal!=null)heal.performed+=_=>healQueued=true;
            Actions["Pause"].performed += _ => { if (!Rebinding) SetPaused(!Paused); };
        }
        void OnEnable() { Actions?.Enable(); }
        // Hide only the OS arrow. Pointer coordinates still drive the independent camera aim.
        void LateUpdate() { if(!quitting)Cursor.visible = CursorForInterface || !Application.isFocused; }
        void OnApplicationQuit(){quitting=true;}
        void OnDisable()
        {
            rebind?.Cancel(); Actions?.Disable(); ClearQueues();
            if (Paused) SetPaused(false);
            if(!quitting)Cursor.visible = true;
        }
        void OnDestroy() { rebind?.Dispose(); if (Actions) Destroy(Actions); }
        void OnApplicationFocus(bool focus) { if (!focus && Actions != null) SetPaused(true); }
        void ClearQueues() { dodgeQueued = interactQueued = flashQueued = meleeQueued = healQueued = false; }
        public bool ConsumeMelee(){bool v=meleeQueued;meleeQueued=false;return v;}
        public bool ConsumeHeal(){bool v=healQueued;healQueued=false;return v;}
        public void SetGameplayLocked(bool value) { GameplayLocked = value; ClearQueues(); }
        public bool ConsumeDodge() { bool value = dodgeQueued; dodgeQueued = false; return value; }
        public bool ConsumeInteract() { bool value = interactQueued; interactQueued = false; return value; }
        public bool ConsumeFlash() { bool value = flashQueued; flashQueued = false; return value; }
        public void SetPaused(bool value)
        {
            Paused = value; ClearQueues(); Time.timeScale = value ? 0f : 1f;
            mobileMove = Vector2.zero; mobileSprint = false;
            PauseChanged?.Invoke();
        }
        public Vector2 AimStick => mobileActive ? mobileAim : Actions["Aim"].ReadValue<Vector2>();
        public Vector2 Pointer => Actions["Pointer"].ReadValue<Vector2>();
        public bool PointerMoved => Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > .01f;
        public void Mobile(Vector2 move, Vector2 aim, bool sprint, bool dodge, bool interact, bool flash,bool melee=false,bool heal=false)
        {
            mobileActive = true; mobileMove = move; mobileAim = aim; mobileSprint = sprint;
            if (Paused) return;
            dodgeQueued |= dodge; interactQueued |= interact; flashQueued |= flash;
            meleeQueued|=melee;healQueued|=heal;
        }
        public void BeginRebind(string actionName, int bindingIndex)
        {
            if (Rebinding || !Paused) return;
            var action = Actions[actionName];
            action.Disable();
            rebind = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .WithControlsExcluding("<Mouse>/position").WithControlsExcluding("<Mouse>/delta")
                .OnCancel(_ => FinishRebind(action, false)).OnComplete(_ => FinishRebind(action, true));
            rebind.Start();
        }
        void FinishRebind(InputAction action, bool save)
        {
            rebind.Dispose(); rebind = null; action.Enable();
            if (save) { PlayerPrefs.SetString(SaveKey, Actions.SaveBindingOverridesAsJson()); PlayerPrefs.Save(); }
        }
        public void ResetBindings()
        {
            if (Rebinding) return;
            Actions.RemoveAllBindingOverrides(); PlayerPrefs.DeleteKey(SaveKey); PlayerPrefs.Save();
        }
    }
}
