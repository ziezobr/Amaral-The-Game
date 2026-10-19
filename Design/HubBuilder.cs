using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static void BuildNeighborhood()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);currentMapSize=new Vector2(64,128f/3);
            var map=Import(Art+"/Campaign/bairro.png");Visual("Bairro / cenário próprio",Patch(map,new Rect(0,0,map.width,map.height),"Bairro aberto",64),Vector2.zero,-5000);
            Block("limite norte",.5f,.01f,.99f,.02f);Block("limite sul",.5f,.99f,.99f,.02f);Block("limite oeste",.01f,.5f,.02f,.99f);Block("limite leste",.99f,.5f,.02f,.99f);
            foreach(float x in new[]{.15f,.525f,.86f})foreach(float y in new[]{.14f,.525f,.88f})Block("Edificação / quarteirão",x,y,x<.2f?.25f:x<.6f?.22f:.19f,y<.2f?.23f:y<.6f?.20f:.17f);
            var level=new GameObject("Bairro • destinos e trabalhos").AddComponent<LevelProgress>();level.title="Patos de Minas • bairro aberto";level.chapter="EXPLORAÇÃO";
            level.mapTexture=map;level.mapSize=currentMapSize;level.amaralPortrait=Portrait("Amaral");level.objectives=new[]{"Vá à agência para seguir a missão principal. Mocambo e trabalhos são opcionais."};
            var player=Player(W(.35f,.45f));CameraRig(player.transform);
            Transform Gate(string label,HubDestination kind,float u,float v)
            {
                var go=new GameObject(label);go.layer=10;go.transform.position=Free(W(u,v));go.AddComponent<CircleCollider2D>().isTrigger=true;
                var gate=go.AddComponent<HubGate>();gate.label=label;gate.destination=kind;
                var marker=Visual("Destino",ring,go.transform.position,1400,go.transform);marker.color=kind==HubDestination.MainMission?new Color(1,.78f,.15f):new Color(.35f,1,.75f);
                var labelGo=new GameObject("Placa do destino");labelGo.transform.SetParent(go.transform,false);labelGo.transform.localPosition=Vector3.up*1.2f;
                var t=labelGo.AddComponent<TextMesh>();t.text=label;t.anchor=TextAnchor.MiddleCenter;t.fontSize=48;t.characterSize=.15f;t.GetComponent<MeshRenderer>().sortingOrder=1600;
                return go.transform;
            }
            level.objectiveTargets=new[]{Gate("MISSÃO PRINCIPAL",HubDestination.MainMission,.20f,.28f)};
            Gate("PARQUE DO MOCAMBO",HubDestination.Mocambo,.86f,.28f);
            Gate("TRABALHO / ENTREGA",HubDestination.CourierStart,.47f,.28f);
            Gate("PADARIA / RECEBER ENTREGA",HubDestination.CourierFinish,.20f,.635f);
            Gate("ENSAIO / FOTO DA PRAÇA",HubDestination.StreetPhoto,.85f,.635f).gameObject.AddComponent<HubPhotoJob>();
            Gate("CÂMERA PERDIDA",HubDestination.LostCamera,.56f,.78f);
            var merchant=Npc("Henzo Ratão",3,.56f,.28f,-1,new[]{"Munição, capacitores e melhorias. Pode chegar."},false);
            var shop=merchant.gameObject.AddComponent<EquipmentShop>();shop.portrait=Portrait("Henzo");UnityEngine.Object.DestroyImmediate(merchant.GetComponent<StoryInteractable>());
            var civilians=Slice(Import(Art+"/Campaign/pedestrians.png"),4,4,2.05f,"Pedestre",true);
            var sidewalk=new[]{new Vector2(.08f,.27f),new Vector2(.29f,.27f),new Vector2(.29f,.42f),new Vector2(.08f,.42f)};
            for(int i=0;i<20;i++)
            {
                int district=i%4;Vector2 offset=new Vector2(district%2*.39f,district/2*.35f);var route=new Vector2[4];for(int j=0;j<4;j++)route[j]=Free(W(sidewalk[j].x+offset.x,sidewalk[j].y+offset.y));
                var go=new GameObject("Morador / "+i);go.layer=13;go.transform.position=route[i%4];var sr=Visual("Morador",civilians[(i%4)*4],go.transform.position,1000,go.transform);
                var rb=go.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.freezeRotation=true;rb.interpolation=RigidbodyInterpolation2D.Interpolate;go.AddComponent<CircleCollider2D>().radius=.22f;
                var walker=go.AddComponent<StreetWalker>();walker.route=route;walker.visual=sr;walker.speed=1.1f+(i%5)*.14f;walker.frames=new Sprite[4];Array.Copy(civilians,(i%4)*4,walker.frames,0,4);
                Visual("Sombra do pedestre",shadow,go.transform.position,-2000,go.transform);
            }
            foreach(var uv in new[]{new Vector2(.15f,.35f),new Vector2(.49f,.70f),new Vector2(.70f,.53f)})
            {
                var root=Vehicle(0,W(uv.x,uv.y),"Gol / veículo dirigível");root.gameObject.layer=8;
                var rb=root.gameObject.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.freezeRotation=true;rb.linearDamping=2;rb.mass=5;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
                root.gameObject.AddComponent<BoxCollider2D>().size=new Vector2(1.55f,3.4f);
                var car=root.gameObject.AddComponent<DriveableCar>();car.frames=new Sprite[4];Array.Copy(bank.vehicles,car.frames,4);car.visual=root.GetComponentInChildren<SpriteRenderer>();
                var door=new GameObject("Porta / entrar");door.transform.SetParent(root,false);door.transform.localPosition=Vector3.right*1.3f;door.layer=10;door.AddComponent<CircleCollider2D>().isTrigger=true;door.AddComponent<VehicleDoor>().car=car;
            }
            var hud=new GameObject("HUD do bairro").AddComponent<GameHud>();hud.input=player.GetComponent<PlayerInputReader>();hud.motor=player.GetComponent<PlayerMotor>();hud.interaction=player.GetComponent<Interaction>();hud.level=level;
            new GameObject("Navegação").AddComponent<GridNavigator>();new GameObject("Áudio do bairro").AddComponent<OfficeAudio>();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/Scenes/25_Bairro.unity");
        }
    }
}
