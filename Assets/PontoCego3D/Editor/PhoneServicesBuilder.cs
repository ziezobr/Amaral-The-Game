using UnityEngine;
namespace PontoCego3D.Editor
{
 public static partial class DistrictBuilder
 {
  static void AddPhoneServices(OpenDistrict district)
  {
   var taxi=new GameObject("Serviço de Uber / Higor").AddComponent<CityTaxi>();taxi.vehicleTemplate=CreateVehicle(new Vector3(0,-1000,0),new Color(.58f,.62f,.65f),false,206);taxi.vehicleTemplate.name="Modelo do Uber / inativo";taxi.vehicleTemplate.SetActive(false);
   var driver=Character("Higor",new Vector3(0,-1000,0),Role3D.Civilian,false,71);driver.AddComponent<CapsuleCollider>().center=Vector3.up*.9f;driver.GetComponent<CapsuleCollider>().height=1.8f;driver.GetComponent<CapsuleCollider>().radius=.28f;
   foreach(var renderer in driver.GetComponentsInChildren<SkinnedMeshRenderer>())if(renderer.sharedMaterials.Length==4){var materials=renderer.sharedMaterials;materials[1]=Mat("Higor / camiseta cinza",new Color(.48f,.48f,.45f));materials[2]=Mat("Higor / calça preta",new Color(.08f,.09f,.10f));renderer.sharedMaterials=materials;}
   taxi.driverTemplate=driver;driver.name="Higor / modelo inativo";driver.SetActive(false);
  }
 }
}
