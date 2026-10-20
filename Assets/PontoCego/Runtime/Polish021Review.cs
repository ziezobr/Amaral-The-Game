using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace PontoCego
{
    /// <summary>Opt-in native 1080p review. Activated only through --ponto-world-capture --polish-review; saves are disabled.</summary>
    public static class Polish021Review
    {
        static IEnumerator Shot(string folder,string name){yield return new WaitForSeconds(.7f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSeconds(.6f);}
        public static IEnumerator Run(string folder)
        {
            var menu=UnityEngine.Object.FindAnyObjectByType<MainMenu>();if(menu){menu.OpenDifficulty(null);yield return Shot(folder,"00_Dificuldade_antes_de_iniciar");}
            GameSession.NewCampaign();yield return new WaitForSeconds(2);
            if(GameSession.Current.currency!=50)throw new InvalidOperationException("New game must start with R$50");Debug.Log("POLISH021_NEW_GAME_R50_OK");var world=WorldHub.Instance;world.enabled=false;var player=UnityEngine.Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();
            var clock=WorldEnvironment.DayNightCycle.Instance;clock.frozen=true;clock.SetHour(12);WorldWeather2D.Instance.automaticWeather=false;WorldWeather2D.Instance.SetWeather(WeatherKind.Clear,true);
            foreach(var traffic in UnityEngine.Object.FindObjectsByType<CityTraffic>())traffic.gameObject.SetActive(false);
            player.Body.position=new Vector2(-10,-30);player.transform.position=player.Body.position;Physics2D.SyncTransforms();
            yield return Shot(folder,"01_HUD_objetivos_minimapa_1080p");
            yield return MovementReview(folder,player,input);
            var rescue=world.Find("21_Adryan").template.GetComponentsInChildren<GameplaySequence>(true).First(s=>s.actor&&s.actor.name.StartsWith("Adryan"));
            var child=UnityEngine.Object.Instantiate(rescue.actor.gameObject,player.transform.position+Vector3.right*1.5f,Quaternion.identity);child.name="Adryan criança / verificação";child.SetActive(true);if(!child.GetComponent<CivilianFollower>())child.AddComponent<CivilianFollower>();
            var cf=child.GetComponent<CivilianFollower>();if(cf)cf.enabled=false; child.transform.position=player.transform.position+Vector3.right*2; var cv=child.GetComponent<ReferenceActorAnimation>().visual; Debug.Log("CHILD_DEBUG "+cv.sprite.rect+" tex "+cv.sprite.texture.width+"x"+cv.sprite.texture.height+" mat "+cv.sharedMaterial.shader.name+" mode "+cv.drawMode+" mask "+cv.maskInteraction+" bounds "+cv.bounds); yield return Shot(folder,"02_Adryan_crianca_ao_lado_Amaral");
            var unique=new Dictionary<EnemyArchetype,EnemyAgent>();foreach(var m in world.missions)foreach(var e in m.template.GetComponentsInChildren<EnemyAgent>(true)){var v=e.GetComponent<EnemyVariety>();if(v&&!unique.ContainsKey(v.archetype))unique[v.archetype]=e;}
            var demonstration=new List<GameObject>();foreach(var pair in unique){var go=UnityEngine.Object.Instantiate(pair.Value.gameObject,new Vector2(-15f+(int)pair.Key*2,-29f),Quaternion.identity);go.SetActive(true);demonstration.Add(go);go.GetComponent<EnemyAgent>().enabled=false;go.GetComponent<EnemyVariety>().enabled=false;go.GetComponent<Rigidbody2D>().simulated=false;}
            yield return Shot(folder,"03_Seis_tipos_de_inimigos");foreach(var go in demonstration)UnityEngine.Object.Destroy(go);
            var animation=player.GetComponent<ReferenceActorAnimation>();var seen=new HashSet<Sprite>();input.Mobile(Vector2.right,Vector2.down,false,false,false,false);
            float end=Time.time+1;while(Time.time<end){seen.Add(animation.visual.sprite);yield return null;}input.Mobile(Vector2.zero,Vector2.down,false,false,false,false);if(seen.Count<2)throw new InvalidOperationException("Walking frames did not advance");Debug.Log("POLISH021_WALK_FRAMES / "+seen.Count);
            GameSession.Current.currency=150;var shop=UnityEngine.Object.FindAnyObjectByType<EquipmentShop>();shop.Interact();UnityEngine.Object.FindAnyObjectByType<GameHud>().InspectShopProduct(10);yield return Shot(folder,"04_Henzo_descricao_Monster_branco");shop.Buy(10);yield return Shot(folder,"05_Melhoria_stamina_comprada");shop.Close();
            player.Body.position=Mateus.ApproachPosition;player.transform.position=player.Body.position;GameSession.Current.collected.Add("gold:start:0");yield return Shot(folder,"06_Mateus_missao_secundaria");
            player.Body.position=world.Find("01_Riberal").entry+Vector2.down*1.8f;player.transform.position=player.Body.position;yield return Shot(folder,"07_Porta_Riberal_alinhada");
            player.Body.position=world.Find("03_Casa").center+new Vector2(15.5f,-18.5f);player.transform.position=player.Body.position;yield return Shot(folder,"09_Cemiterio_ao_lado_casa");
            var questTarget=UnityEngine.Object.FindObjectsByType<GoldenQuestPickup>().First(t=>t.chain==0&&t.step==0);
            player.Body.position=(Vector2)questTarget.transform.position+Vector2.down;player.transform.position=player.Body.position;UnityEngine.Object.FindAnyObjectByType<CityNavigation>().FollowMateus=true;
            yield return Shot(folder,"15_Mateus_acesso_objetivo");
            GameSession.Current.hasSidearm=true;GameSession.Current.magazine=2;var inventory=player.GetComponent<AmmoInventory>();inventory.Reload();yield return Shot(folder,"16_Recarga_visual");
            GameSession.Current.collected.Add("weapon:xuxa");var equipment=player.GetComponent<PlayerEquipment2D>();equipment.Select(PlayerEquipment.PinkCamera);input.Mobile(Vector2.zero,Vector2.down,false,false,false,false);yield return Shot(folder,"17_Amaral_segurando_camera_rosa");
            equipment.Select(PlayerEquipment.Fists);input.Mobile(Vector2.zero,Vector2.down,false,false,false,true);yield return new WaitForSeconds(.14f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"18_Maos_clique_esquerdo.png"));yield return new WaitForSeconds(.5f);input.Mobile(Vector2.zero,Vector2.down,false,false,false,false);
            world.Navigate("01_Riberal");player.Body.position=world.Find("01_Riberal").entry;player.transform.position=player.Body.position;world.TryBegin("01_Riberal");yield return Shot(folder,"19_Introducao_cinematica_missao");player.GetComponent<StoryDialogue>().DismissForValidation();world.Complete("02_Gala");
            MapWaypoint.Set(new Vector2(60,-22));var phone=player.GetComponent<PlayerPhone>();phone.OpenMapEditor();yield return Shot(folder,"20_Mapa_ampliado_waypoint");TaxiService.Instance.SelectWaypoint();phone.OpenApp(PhoneApp.Uber);yield return Shot(folder,"21_Uber_destino_waypoint");phone.Close();
            Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSeconds(1);yield return Shot(folder,"08_Minimapa_720p");
            Debug.Log("POLISH021_NATIVE_REVIEW_OK");yield return new WaitForSeconds(.5f);
        }
        static IEnumerator MovementReview(string folder,PlayerMotor player,PlayerInputReader input)
        {
            var animation=player.GetComponent<ReferenceActorAnimation>();var parent=animation.visual.transform.parent;var frames=new HashSet<Sprite>();
            for(int d=0;d<8;d++)
            {
                float angle=d*Mathf.PI/4;Vector2 direction=new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));
                player.Body.position=new Vector2(-10,-30);player.transform.position=player.Body.position;
                input.Mobile(direction,direction,false,false,false,false);float until=Time.time+.65f;
                while(Time.time<until){var sprite=animation.visual.sprite;if(!sprite||sprite.bounds.size.y<1.1f||sprite.bounds.size.y>2.6f)throw new InvalidOperationException("Invalid body frame in direction "+d);frames.Add(sprite);yield return null;}
            }
            input.Mobile(Vector2.right,Vector2.right,true,false,false,false);yield return Shot(folder,"12_Amaral_correndo");
            input.Mobile(Vector2.right,Vector2.right,false,true,false,false);yield return new WaitForSeconds(.08f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"13_Amaral_esquiva.png"));yield return new WaitForSeconds(.5f);
            input.Mobile(Vector2.zero,Vector2.right,false,false,false,false,true);yield return new WaitForSeconds(.15f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"14_Amaral_ataque.png"));yield return new WaitForSeconds(.5f);
            input.Mobile(Vector2.zero,Vector2.down,false,false,false,false);player.Body.position=new Vector2(-10,-30);player.transform.position=player.Body.position;
            if(animation.visual.transform.parent!=parent)throw new InvalidOperationException("Actor hierarchy changed and broke Animator bindings");
            if(frames.Count<16)throw new InvalidOperationException("Not enough directional walking frames: "+frames.Count);
            Debug.Log("SPRITE022_EIGHT_DIRECTIONS_OK / "+frames.Count+" / hierarchy preserved");
        }
    }
}
