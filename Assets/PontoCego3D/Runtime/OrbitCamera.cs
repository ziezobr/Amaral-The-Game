using UnityEngine;using UnityEngine.InputSystem;using Unity.Cinemachine;
namespace PontoCego3D
{
    [DefaultExecutionOrder(-20)]
    public sealed class OrbitCamera:MonoBehaviour
    {
        public Transform target;public ThirdPersonInput input;public CinemachineCamera cameraRig;
        public bool isometric;public float yaw,pitch=13,distance=5.2f;public bool Manual;Vector3 focus,focusVelocity;float currentDistance;
        void Start(){focus=target.position+Vector3.up*1.45f;currentDistance=distance;}
        void LateUpdate()
        {
            if(!target||input.Paused)return;float dt=Time.deltaTime;
            if(isometric&&!Manual)
            {
                focus=Vector3.SmoothDamp(focus,target.position+Vector3.up*.8f,ref focusVelocity,.16f);
                var isoRotation=Quaternion.Euler(52,yaw,0);cameraRig.transform.SetPositionAndRotation(focus-isoRotation*Vector3.forward*30,isoRotation);return;
            }
            if(!Manual)
            {
                var mouse=Mouse.current;var pad=Gamepad.current;Vector2 look=mouse!=null?mouse.delta.ReadValue()*.12f:Vector2.zero;
                if(pad!=null)look+=pad.rightStick.ReadValue()*125*dt;yaw+=look.x;pitch=Mathf.Clamp(pitch-look.y,-24,65);
            }
            focus=Vector3.SmoothDamp(focus,target.position+Vector3.up*(input.Aim?1.5f:1.45f),ref focusVelocity,.055f);
            var rotation=Quaternion.Euler(pitch,yaw,0);float ideal=input.Aim?2.3f:distance;Vector3 shoulder=rotation*Vector3.right*(input.Aim?.65f:.35f);
            var desired=focus+shoulder-rotation*Vector3.forward*ideal;var delta=desired-focus;
            float safe=ideal;
            if(Physics.SphereCast(focus,.22f,delta.normalized,out var hit,delta.magnitude,1<<8,QueryTriggerInteraction.Ignore))safe=Mathf.Max(.45f,hit.distance-.12f);
            currentDistance=safe<currentDistance?safe:Mathf.Lerp(currentDistance,safe,1-Mathf.Exp(-dt*8));
            cameraRig.transform.SetPositionAndRotation(focus+delta.normalized*currentDistance,rotation);
            var lens=cameraRig.Lens;lens.FieldOfView=Mathf.Lerp(lens.FieldOfView,input.Aim?49:63,1-Mathf.Exp(-dt*8));cameraRig.Lens=lens;
        }
        public void Snap(){focus=target.position+Vector3.up*1.45f;focusVelocity=Vector3.zero;currentDistance=distance;}
    }
}
