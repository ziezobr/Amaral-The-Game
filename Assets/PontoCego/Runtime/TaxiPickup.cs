using UnityEngine;
namespace PontoCego
{
    public sealed class TaxiPickup:MonoBehaviour,IInteractable
    {
        public TaxiService service;public string Prompt=>service.Arrived?"E • Embarcar / Peugeot 206 cinza":"Uber chegando";
        public void Interact(){if(!string.IsNullOrEmpty(service.SelectedDestination))service.BeginRide();else service.OpenDestinations();}
    }
}
