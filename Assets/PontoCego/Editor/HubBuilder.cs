using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        static Sprite[] civilianFrames;
        static void BuildNeighborhood()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);currentMapSize=new Vector2(64,128f/3);
            var map=Import(Art+"/Campaign/bairro.png");Visual("Bairro / cenário próprio",Patch(map,new Rect(0,0,map.width,map.height),"Bairro aberto",64),Vector2.zero,-5000);
            Block("limite norte",.5f,.01f,.99f,.02f);Block("limite sul",.5f,.99f,.99f,.02f);Block("limite oeste",.01f,.5f,.02f,.99f);Block("limite leste",.99f,.5f,.02f,.99f);
            foreach(float x in new[]{.15f,.50f,.85f})foreach(float y in new[]{.11f,.45f,.82f})Block("Edificação / quarteirão",x,y,x<.2f?.25f:x<.6f?.23f:.25f,y<.2f?.17f:y<.6f?.18f:.24f);
            var level=new GameObject("Bairro • destinos e trabalhos").AddComponent<LevelProgress>();level.title="Patos de Minas • bairro aberto";level.chapter="EXPLORAÇÃO";
            level.mapTexture=map;level.mapSize=currentMapSize;level.amaralPortrait=Portrait("Amaral");level.objectives=new[]{"Vá à agência para seguir a missão principal. Mocambo e trabalhos são opcionais."};
            var player=Player(W(.33f,.40f));CameraRig(player.transform);
            Transform Gate(string label,HubDestination kind,float u,float v)
            {
                var go=new GameObject(label);go.layer=10;go.transform.position=Free(W(u,v));go.AddComponent<CircleCollider2D>().isTrigger=true;
                var gate=go.AddComponent<HubGate>();gate.label=label;gate.destination=kind;
                AddProp(go,kind==HubDestination.CourierStart||kind==HubDestination.CourierFinish?13:kind==HubDestination.StreetPhoto?14:15,-1,1.1f);
                var labelGo=new GameObject("Placa do destino");labelGo.transform.SetParent(go.transform,false);labelGo.transform.localPosition=Vector3.up*1.2f;
                var t=labelGo.AddComponent<TextMesh>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;t.text=label;t.anchor=TextAnchor.MiddleCenter;t.fontSize=48;t.characterSize=.15f;t.GetComponent<MeshRenderer>().sortingOrder=1600;var fit=labelGo.AddComponent<WorldTextLabel>();fit.width=4.2f;fit.height=.4f;fit.visibleDistance=8;
                return go.transform;
            }
            level.objectiveTargets=new[]{Gate("MISSÃO PRINCIPAL",HubDestination.MainMission,.20f,.22f)};
            Gate("PARQUE DO MOCAMBO",HubDestination.Mocambo,.85f,.22f);
            Gate("TRABALHO / ENTREGA",HubDestination.CourierStart,.47f,.22f);
            Gate("PADARIA / RECEBER ENTREGA",HubDestination.CourierFinish,.20f,.57f);
            Gate("ENSAIO / FOTO DA PRAÇA",HubDestination.StreetPhoto,.85f,.57f).gameObject.AddComponent<HubPhotoJob>();
            Gate("CÂMERA PERDIDA",HubDestination.LostCamera,.56f,.68f);
            var merchant=Npc("Henzo Ratão",3,.56f,.22f,-1,new[]{"Munição, capacitores e melhorias. Pode chegar."},false);
            var shop=merchant.gameObject.AddComponent<EquipmentShop>();shop.portrait=Portrait("Henzo");UnityEngine.Object.DestroyImmediate(merchant.GetComponent<StoryInteractable>());
            var civilians=civilianFrames;
            var sidewalk=new[]{new Vector2(.05f,.22f),new Vector2(.285f,.22f),new Vector2(.285f,.345f),new Vector2(.05f,.345f)};
            for(int i=0;i<20;i++)
            {
                int district=i%4;Vector2 offset=new Vector2(district%2*.37f,district/2*.35f);var route=new Vector2[4];for(int j=0;j<4;j++)route[j]=Free(W(sidewalk[j].x+offset.x,sidewalk[j].y+offset.y));
                var go=new GameObject("Morador / "+i);go.layer=13;go.transform.position=route[i%4];var sr=Visual("Morador",civilians[(i%4)*4],go.transform.position,1000,go.transform);
                var rb=go.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.freezeRotation=true;rb.interpolation=RigidbodyInterpolation2D.Interpolate;go.AddComponent<CircleCollider2D>().radius=.22f;
                var walker=go.AddComponent<StreetWalker>();walker.route=route;walker.visual=sr;walker.speed=1.1f+(i%5)*.14f;walker.frames=new Sprite[4];Array.Copy(civilians,(i%4)*4,walker.frames,0,4);
                Visual("Sombra do pedestre",shadow,go.transform.position,-2000,go.transform);
            }
            foreach(var uv in new[]{new Vector2(.15f,.28f),new Vector2(.49f,.63f),new Vector2(.67f,.45f)})
            {
                var root=Vehicle(0,W(uv.x,uv.y),"Gol / veículo dirigível");root.gameObject.layer=8;
                var rb=root.gameObject.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.freezeRotation=true;rb.linearDamping=2;rb.mass=5;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
                root.gameObject.AddComponent<BoxCollider2D>().size=new Vector2(1.35f,2.9f);
                var car=root.gameObject.AddComponent<DriveableCar>();car.frames=new Sprite[4];Array.Copy(bank.vehicles,car.frames,4);car.visual=root.GetComponentInChildren<SpriteRenderer>();
                var door=new GameObject("Porta / entrar");door.transform.SetParent(root,false);door.transform.localPosition=Vector3.right*1.3f;door.layer=10;door.AddComponent<CircleCollider2D>().isTrigger=true;door.AddComponent<VehicleDoor>().car=car;
            }
            var hud=new GameObject("HUD do bairro").AddComponent<GameHud>();hud.input=player.GetComponent<PlayerInputReader>();hud.motor=player.GetComponent<PlayerMotor>();hud.interaction=player.GetComponent<Interaction>();hud.level=level;hud.art=uiArt;
            BuildUber(player.transform.position);
            new GameObject("Navegação").AddComponent<GridNavigator>();new GameObject("Áudio do bairro").AddComponent<OfficeAudio>();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/Scenes/25_Bairro.unity");
        }
    }
}
