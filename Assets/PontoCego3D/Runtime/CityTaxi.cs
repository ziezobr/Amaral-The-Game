using UnityEngine;
namespace PontoCego3D
{
    /// <summary>A phone booking keeps running while the phone is open. Arrival requires both time and position.</summary>
    public sealed class CityTaxi:MonoBehaviour
    {
        public GameObject vehicleTemplate,driverTemplate;public float arrivalSeconds=60;
        public bool Requested{get;private set;}public bool Arrived{get;private set;}public float Remaining=>Requested?Mathf.Max(0,arrivalSeconds-elapsed):0;
        public Vector3 Pickup{get;private set;}public string Notice{get;private set;}="";public Transform Destination{get;set;}
        GameObject cab,driver;Rigidbody body;float elapsed;Vector3 start,direction;
        public bool Request()
        {
            if(Requested)return false;var player=FindAnyObjectByType<ThirdPersonMotor>();Vector3 p=player.transform.position;float x=Mathf.Clamp(Mathf.Round(p.x/42),-2,2)*42+2.3f,z=Mathf.Clamp(Mathf.Round(p.z/42),-2,2)*42+2.3f;
            if(Mathf.Abs(p.x-x)<Mathf.Abs(p.z-z)){Pickup=new Vector3(x,.2f,Mathf.Clamp(p.z,-85,85));direction=Vector3.forward;}else{Pickup=new Vector3(Mathf.Clamp(p.x,-85,85),.2f,z);direction=Vector3.right;}
            start=Pickup-direction*24;if(Mathf.Abs(start.x)>104||Mathf.Abs(start.z)>104){direction=-direction;start=Pickup-direction*24;}
            elapsed=0;Requested=true;Arrived=false;Notice="Higor está a caminho. Encontro na rua mais próxima.";return true;
        }
        void Update()
        {
            if(!Requested)return;var input=FindAnyObjectByType<ThirdPersonInput>();if(input&&input.Paused)return;elapsed+=Time.deltaTime;
            if(!cab&&elapsed>=arrivalSeconds-5){cab=Instantiate(vehicleTemplate,start,Quaternion.LookRotation(direction));cab.name="Uber do Higor / chegada física";cab.SetActive(true);body=cab.AddComponent<Rigidbody>();body.isKinematic=true;body.interpolation=RigidbodyInterpolation.Interpolate;}
            if(body&&elapsed>=arrivalSeconds&&Vector3.Distance(body.position,Pickup)<.7f&&!Arrived){Arrived=true;Notice="Higor chegou. Escolha um destino ou vá até o motorista.";var offset=Mathf.Abs(direction.x)>.5f?Vector3.back:Vector3.right;driver=Instantiate(driverTemplate,Pickup+offset*2.4f,Quaternion.identity);driver.name="Higor / motorista de Uber";driver.SetActive(true);}
            if(driver&&input.Down("Interact")&&Vector3.Distance(driver.transform.position,input.transform.position)<2.6f)FindAnyObjectByType<OpenDistrict>().OpenPhone(PontoCego.PhoneApp.Uber);
        }
        void FixedUpdate()
        {
            if(!body||Arrived||FindAnyObjectByType<ThirdPersonInput>().Paused)return;Vector3 d=(Pickup-body.position).WithoutY();if(d.magnitude<.08f)return;var forward=d.normalized;
            foreach(var hit in Physics.BoxCastAll(body.position+Vector3.up*.9f+forward*2,new Vector3(.62f,.4f,.18f),forward,Quaternion.LookRotation(forward),.9f,(1<<8)|(1<<9)|(1<<13),QueryTriggerInteraction.Ignore))if(hit.collider.attachedRigidbody!=body&&!hit.collider.transform.IsChildOf(cab.transform)){Notice="Higor aguardando passagem no trânsito.";return;}
            body.MovePosition(Vector3.MoveTowards(body.position,Pickup,5.2f*Time.fixedDeltaTime));
        }
        public void Cancel(){if(cab)Destroy(cab);if(driver)Destroy(driver);cab=driver=null;body=null;Requested=Arrived=false;elapsed=0;Notice="Chamada cancelada.";}
    }
}
