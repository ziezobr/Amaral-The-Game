using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PontoCego.WorldEnvironment;
namespace PontoCego3D.Editor
{
    public static partial class DistrictBuilder
    {
        static void CityDetails()
        {
            // Give each block an identity, keeping the road lanes and mission approaches clear.
            var wood=Mat("Madeira / detalhes",new Color(.45f,.25f,.12f));var metal=Mat("Aço / grafite",new Color(.12f,.16f,.18f));var paving=Mat("Piso da praça",new Color(.69f,.61f,.45f));var green=Mat("Jardim",new Color(.24f,.39f,.18f));
            for(int ix=-2;ix<2;ix++)for(int iz=-2;iz<2;iz++)
            {
                Vector3 center=new Vector3(ix*42+21,.2f,iz*42+21);
                foreach(float side in new[]{-1f,1f})
                {
                    Vector3 at=center+new Vector3(side*13,0,0);Bench(at);var bin=Part(PrimitiveType.Cylinder,"Lixeira / coleta seletiva",at+new Vector3(0,.55f,2.2f),new Vector3(.65f,.55f,.65f),Mat("Lixeira verde",new Color(.14f,.38f,.24f)),true);Box("Boca da lixeira",at+new Vector3(0,1.12f,2.2f),new Vector3(.48f,.04f,.38f),metal);
                    Box("Floreira / base",center+new Vector3(side*8,.32f,13),new Vector3(3,.55f,1.2f),wood,true);Part(PrimitiveType.Sphere,"Floreira / arbusto",center+new Vector3(side*8,.85f,13),new Vector3(3.1f,.9f,1.4f),green);
                    var pole=Part(PrimitiveType.Cylinder,"Placa de trânsito / poste",center+new Vector3(side*14,1.5f,10),new Vector3(.08f,1.5f,.08f),metal,true);Box("Placa de rua / fundo",pole.transform.position+Vector3.up*1.1f,new Vector3(2.4f,.5f,.08f),Mat("Placa de rua azul",new Color(.08f,.25f,.36f)));Sign(ix==0?"RUA DA RIBERAL":"PATOS DE MINAS",pole.transform.position+new Vector3(0,1.1f,-.05f),2.1f,Quaternion.identity,Color.white);
                }
                // Facade details replace repeated empty walls with shopfronts and lived-in roofs.
                foreach(var x in new[]{-8f,8f})
                {
                    Box("Toldo comercial",center+new Vector3(x,2.8f,-13.5f),new Vector3(5,.15f,1.5f),Mat("Toldo "+(ix+iz),Color.Lerp(new Color(.65f,.22f,.12f),new Color(.13f,.37f,.35f),(ix+iz+4)/6f)));
                    Box("Ar condicionado",center+new Vector3(x,4,-13),new Vector3(1.1f,.55f,.45f),Mat("Equipamento creme",new Color(.72f,.72f,.65f)));
                }
            }
            // Mocambo occupies a complete block; no decorative building remains inside the park.
            foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())if(root.name.StartsWith("Prédio /")&&root.transform.position.x> -38&&root.transform.position.x< -4&&root.transform.position.z>4&&root.transform.position.z<38)UnityEngine.Object.DestroyImmediate(root);
            Box("Parque do Mocambo / gramado",new Vector3(-21,.18f,21),new Vector3(31,.12f,31),green);
            Box("Mocambo / alameda",new Vector3(-21,.27f,21),new Vector3(4,.10f,31),paving);Box("Mocambo / travessia",new Vector3(-21,.28f,21),new Vector3(31,.10f,3),paving);
            Box("Quadra / piso terracota",new Vector3(-28,.31f,25),new Vector3(11,.10f,17),Mat("Quadra terracota",new Color(.54f,.24f,.16f)));
            var line=Mat("Quadra / linhas",new Color(.94f,.88f,.69f));foreach(float x in new[]{-33f,-23f})Box("Linha lateral da quadra",new Vector3(x,.37f,25),new Vector3(.10f,.02f,16),line);foreach(float z in new[]{17f,25f,33f})Box("Linha de fundo da quadra",new Vector3(-28,.37f,z),new Vector3(10,.02f,.10f),line);
            foreach(float z in new[]{17.7f,32.3f}){Part(PrimitiveType.Cylinder,"Basquete / suporte",new Vector3(-28,1.8f,z),new Vector3(.15f,1.5f,.15f),metal,true);Box("Basquete / tabela",new Vector3(-28,3.1f,z),new Vector3(2,1.2f,.15f),line);var hoop=Part(PrimitiveType.Cylinder,"Basquete / aro",new Vector3(-28,2.8f,z+(z<25?.5f:-.5f)),new Vector3(.6f,.025f,.6f),Mat("Aro laranja",new Color(1,.37f,.04f)));}
            foreach(var at in new[]{new Vector3(-11,.2f,11),new Vector3(-11,.2f,30),new Vector3(-34,.2f,10)}){Tree(at);Bench(at+Vector3.right*3);}
            Box("Mocambo / placa",new Vector3(-21,2,5.5f),new Vector3(9,1.3f,.2f),Mat("Placa parque",new Color(.06f,.25f,.18f)));Sign("PARQUE DO MOCAMBO",new Vector3(-21,2,5.35f),8.4f,Quaternion.identity,new Color(.96f,.9f,.65f));
            BusStop(new Vector3(-9,.2f,-7));BusStop(new Vector3(48,.2f,10));
            foreach(var at in new[]{new Vector3(11,.2f,-7),new Vector3(35,.2f,8),new Vector3(-48,.2f,7)}){Part(PrimitiveType.Cylinder,"Hidrante",at+Vector3.up*.45f,new Vector3(.35f,.45f,.35f),Mat("Hidrante vermelho",new Color(.65f,.12f,.07f)),true);Box("Hidrante / válvulas",at+Vector3.up*.65f,new Vector3(.7f,.15f,.15f),metal);}
            // Give all existing luminaires actual light sources, and all windows a night emission channel.
            foreach(var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>())
            {
                if(renderer.name.Contains("Luminária")){var glow=renderer.gameObject.AddComponent<NightLight>();glow.glow=renderer;var go=new GameObject("Iluminação pública");go.transform.position=renderer.transform.position-Vector3.up*.25f;var light=go.AddComponent<Light>();light.type=LightType.Point;light.range=18;light.shadows=LightShadows.None;glow.lamp=light;glow.intensity=4.5f;}
                else if(renderer.name.Contains("janela")||renderer.name.Contains("Vitrine")){var glow=renderer.gameObject.AddComponent<NightLight>();glow.glow=renderer;}
            }
            Physics.SyncTransforms();foreach(var item in UnityEngine.Object.FindObjectsByType<MeshRenderer>())if(item.name=="Ar condicionado"||item.name=="Toldo comercial")if(!Physics.Raycast(item.transform.position,Vector3.forward,1.1f,1<<8))UnityEngine.Object.DestroyImmediate(item.gameObject);
            CourtCircle(new Vector3(-28,.39f,25),2.3f,.08f,line);
            foreach(float z in new[]{17.7f,32.3f}){var old=GameObject.Find("Basquete / aro");if(old)UnityEngine.Object.DestroyImmediate(old);CourtCircle(new Vector3(-28,2.8f,z+(z<25?.5f:-.5f)),.3f,.055f,Mat("Aro laranja",new Color(1,.37f,.04f)));}
            Part(PrimitiveType.Sphere,"Bola de basquete",new Vector3(-27.5f,.7f,24),Vector3.one*.55f,Mat("Bola de basquete laranja",new Color(.8f,.26f,.035f)));
            for(int i=0;i<8;i++){float t=i/12f;var route=new[]{new Vector3(2.3f,.2f,-39.7f),new Vector3(44.3f,.2f,-39.7f),new Vector3(44.3f,.2f,2.3f),new Vector3(2.3f,.2f,2.3f)};Vector3 at=Vector3.Lerp(route[i%4],route[(i%4+1)%4],.15f+(i/4)*.25f);CreateVehicle(at,new[]{new Color(.72f,.74f,.74f),new Color(.16f,.19f,.21f),new Color(.82f,.80f,.70f),new Color(.46f,.10f,.08f),new Color(.11f,.23f,.37f),new Color(.31f,.37f,.26f)}[i%6],true,i).GetComponent<CityVehicle>().route=RotateRoute(route,i%4);}
        }
        static void CourtCircle(Vector3 at,float radius,float width,Material material)
        {
            var go=new GameObject("Basquete / círculo");var line=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.loop=true;line.useWorldSpace=true;line.positionCount=48;line.startWidth=line.endWidth=width;line.alignment=LineAlignment.TransformZ;go.transform.rotation=Quaternion.Euler(90,0,0);for(int i=0;i<48;i++){float angle=i*Mathf.PI*2/48;line.SetPosition(i,at+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*radius);}
        }
        static Vector3[] RotateRoute(Vector3[] route,int first){var r=new Vector3[route.Length];for(int i=0;i<r.Length;i++)r[i]=route[(first+i)%r.Length];return r;}
        static void BusStop(Vector3 at)
        {
            var metal=Mat("Abrigo de ônibus",new Color(.12f,.23f,.27f));foreach(float x in new[]{-2f,2f})Part(PrimitiveType.Cylinder,"Ponto de ônibus / pilar",at+new Vector3(x,1.4f,0),new Vector3(.12f,1.4f,.12f),metal,true);Box("Ponto de ônibus / cobertura",at+Vector3.up*2.9f,new Vector3(5,.18f,2.6f),metal);Bench(at);Sign("ÔNIBUS / CENTRO",at+new Vector3(0,2.6f,-.12f),3.5f,Quaternion.identity,Color.white);
        }
        static GameObject CreateVehicle(Vector3 at,Color color,bool moving,int variant)
        {
            var root=new GameObject(moving?"Trânsito 3D / veículo "+variant:"Carro estacionado 3D");root.layer=8;root.transform.position=at;
            var paint=Mat("Carro / pintura "+variant+color.ToString(),color);var rubber=Mat("Pneus",new Color(.045f,.05f,.055f));var chrome=Mat("Rodas / liga",new Color(.42f,.45f,.47f));var windows=Mat("Vidros dos carros",new Color(.09f,.20f,.27f));
            float length=variant%3==0?4.1f:3.7f;
            Hull(root.transform,new[]{new Vector2(-length*.5f,.7f),new Vector2(-length*.42f,.95f),new Vector2(length*.40f,.95f),new Vector2(length*.5f,.68f),new Vector2(length*.48f,.4f),new Vector2(-length*.48f,.4f)},.86f,paint,"Carroceria facetada");
            Hull(root.transform,new[]{new Vector2(-1.1f,.94f),new Vector2(-.65f,1.65f),new Vector2(.55f,1.65f),new Vector2(1.15f,.94f)},.73f,windows,"Cabine / vidros");
            Box("Teto pintado",at+new Vector3(0,1.67f,-.05f),new Vector3(1.5f,.10f,1.25f),paint,false,root.transform);Box("Para-choque dianteiro",at+new Vector3(0,.55f,length*.5f),new Vector3(1.6f,.18f,.12f),rubber,false,root.transform);
            var wheels=new List<Transform>();foreach(float x in new[]{-.84f,.84f})foreach(float z in new[]{-1.18f,1.18f}){var wheel=Part(PrimitiveType.Cylinder,"Roda / pneu",at+new Vector3(x,.42f,z),new Vector3(.68f,.13f,.68f),rubber,false,root.transform);wheel.transform.rotation=Quaternion.Euler(0,0,90);wheels.Add(wheel.transform);var rim=Part(PrimitiveType.Cylinder,"Roda / calota",at+new Vector3(x*1.035f,.42f,z),new Vector3(.39f,.14f,.39f),chrome,false,root.transform);rim.transform.rotation=Quaternion.Euler(0,0,90);}
            foreach(float x in new[]{-.56f,.56f}){Box("Farol do veículo",at+new Vector3(x,.78f,length*.49f),new Vector3(.4f,.23f,.08f),Mat("Farol creme",new Color(1,.91f,.68f)),false,root.transform);Box("Lanterna do veículo",at+new Vector3(x,.78f,-length*.49f),new Vector3(.4f,.23f,.08f),Mat("Lanterna vermelha",new Color(.85f,.08f,.035f)),false,root.transform);}
            root.AddComponent<BoxCollider>().center=Vector3.up*.85f;root.GetComponent<BoxCollider>().size=new Vector3(1.75f,1.2f,length);if(moving){var body=root.AddComponent<Rigidbody>();body.isKinematic=true;body.interpolation=RigidbodyInterpolation.Interpolate;root.AddComponent<CityVehicle>().wheels=wheels.ToArray();}return root;
        }
        static void Hull(Transform parent,Vector2[] profile,float halfWidth,Material material,string name)
        {
            int n=profile.Length;var vertices=new Vector3[n*2];for(int i=0;i<n;i++){vertices[i]=new Vector3(-halfWidth,profile[i].y,profile[i].x);vertices[i+n]=new Vector3(halfWidth,profile[i].y,profile[i].x);}var tris=new List<int>();for(int i=1;i<n-1;i++){tris.AddRange(new[]{0,i+1,i,n,n+i,n+i+1});}for(int i=0;i<n;i++){int j=(i+1)%n;tris.AddRange(new[]{i,j,j+n,i,j+n,i+n});}var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=tris.ToArray();mesh.RecalculateNormals();AssetDatabase.AddObjectToAsset(mesh,Mat("Malha / armazenamento",Color.white));var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
    }
}
