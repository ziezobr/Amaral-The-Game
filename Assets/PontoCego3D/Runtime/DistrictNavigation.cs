using UnityEngine;using UnityEngine.AI;
namespace PontoCego3D
{
    [DefaultExecutionOrder(-10000)]public sealed class DistrictNavigation:MonoBehaviour
    {
        public NavMeshData data;NavMeshDataInstance instance;
        void OnEnable(){if(data)instance=NavMesh.AddNavMeshData(data);}
        void OnDisable(){if(instance.valid)instance.Remove();}
    }
}
