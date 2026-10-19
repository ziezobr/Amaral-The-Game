using UnityEngine;
namespace PontoCego3D
{
    [RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
    public sealed class CityVehicle:MonoBehaviour
    {
        public Vector3[] route;public float speed=5;public Transform[] wheels;int next=1;Rigidbody body;float velocity;
        void Awake(){body=GetComponent<Rigidbody>();}
        void FixedUpdate()
        {
            var input=FindAnyObjectByType<ThirdPersonInput>();if(!input||input.Paused||route==null||route.Length<2)return;
            Vector3 delta=(route[next]-body.position).WithoutY();if(delta.magnitude<.7f){next=(next+1)%route.Length;delta=(route[next]-body.position).WithoutY();}
            Vector3 forward=delta.normalized;float target=speed;
            foreach(var hit in Physics.BoxCastAll(body.position+Vector3.up*.8f+forward*2,new Vector3(.65f,.4f,.2f),forward,Quaternion.LookRotation(forward),3.8f,(1<<8)|(1<<9)|(1<<13),QueryTriggerInteraction.Ignore))
            {if(hit.collider.transform.IsChildOf(transform)||hit.collider.attachedRigidbody==body)continue;target=Mathf.Min(target,Mathf.Max(0,(hit.distance-.7f)*2));}
            velocity=Mathf.MoveTowards(velocity,target,(target<velocity?12:3)*Time.fixedDeltaTime);body.MovePosition(Vector3.MoveTowards(body.position,route[next],velocity*Time.fixedDeltaTime));body.MoveRotation(Quaternion.Slerp(body.rotation,Quaternion.LookRotation(forward),Time.fixedDeltaTime*7));
            if(wheels!=null)foreach(var wheel in wheels)wheel.Rotate(velocity*Time.fixedDeltaTime*100,0,0,Space.Self);
        }
    }
}
