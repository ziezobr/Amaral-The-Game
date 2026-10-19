using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.Cinemachine;
namespace PontoCego.Tests
{
    public sealed class ContinuousWorldTests
    {
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;GameSession.ExploreNeighborhood();yield return null;yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){GameSession.Current.openNeighborhood=false;GameSession.Current.worldMission="";GameSession.Current.worldPositionValid=false;yield return null;}
        [UnityTest]public IEnumerator PhysicalTriggerKeepsTheSamePlayerCameraAndLoadedScene()
        {
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();var camera=Object.FindAnyObjectByType<CinemachineCamera>();var scene=SceneManager.GetActiveScene();
            Vector2 before=player.Body.position;world.Navigate("01_Riberal");Assert.That(world.TryBegin("01_Riberal"),Is.False);
            Assert.That(player.Body.position,Is.EqualTo(before));var entry=world.Find("01_Riberal").entry;
            player.Body.position=entry+Vector2.up*2;player.transform.position=player.Body.position;
            player.GetComponent<PlayerInputReader>().Mobile(Vector2.down,Vector2.down,false,false,false,false);
            for(int i=0;i<35&&world.Active==null;i++)yield return new WaitForFixedUpdate();
            Assert.That(world.Active,Is.Not.Null);Assert.That(world.Active.id,Is.EqualTo("01_Riberal"));
            Assert.That(Object.FindAnyObjectByType<PlayerMotor>(),Is.SameAs(player));Assert.That(Object.FindAnyObjectByType<CinemachineCamera>(),Is.SameAs(camera));
            Assert.That(SceneManager.GetActiveScene().handle,Is.EqualTo(scene.handle));Assert.That(Object.FindObjectsByType<Camera>().Length,Is.EqualTo(1));
            player.GetComponent<StoryDialogue>().DismissForValidation();Vector2 at=player.Body.position;GameSession.Travel("02_Gala");
            // Check the transition synchronously: the held movement input may legitimately move the player on later frames.
            Assert.That(Vector2.Distance(at,player.Body.position),Is.LessThan(.2f));yield return null;
            Assert.That(WorldHub.Instance,Is.SameAs(world));Assert.That(world.Active,Is.Null);
            Assert.That(Object.FindObjectsByType<PlayerMotor>().Length,Is.EqualTo(1));
        }
        [UnityTest]public IEnumerator EveryDistrictEntranceConnectsToTheCityThroughRealCollisions()
        {
            var world=WorldHub.Instance;Physics2D.SyncTransforms();var rect=world.bounds;const float cell=.8f;
            int width=Mathf.CeilToInt(rect.width/cell),height=Mathf.CeilToInt(rect.height/cell);var open=new bool[width,height];var visited=new bool[width,height];
            Vector2 Point(int x,int y)=>rect.min+new Vector2((x+.5f)*cell,(y+.5f)*cell);
            for(int x=0;x<width;x++)for(int y=0;y<height;y++)open[x,y]=!Physics2D.OverlapCircle(Point(x,y),.29f,LayerMask.GetMask("World"));
            Vector2 player=Object.FindAnyObjectByType<PlayerMotor>().transform.position;int sx=Mathf.FloorToInt((player.x-rect.xMin)/cell),sy=Mathf.FloorToInt((player.y-rect.yMin)/cell);
            var queue=new Queue<Vector2Int>();queue.Enqueue(new Vector2Int(sx,sy));visited[sx,sy]=true;
            while(queue.Count>0){var at=queue.Dequeue();foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var n=at+d;if(n.x<0||n.y<0||n.x>=width||n.y>=height||visited[n.x,n.y]||!open[n.x,n.y])continue;visited[n.x,n.y]=true;queue.Enqueue(n);}}
            var checkedCenters=new HashSet<Vector2>();foreach(var mission in world.missions)
            {
                Assert.That(Physics2D.OverlapCircle(mission.entry,.29f,LayerMask.GetMask("World")),Is.Null,mission.id+" blocked trigger");
                if(!checkedCenters.Add(mission.center))continue;bool reachable=false;
                for(int x=0;x<width&&!reachable;x++)for(int y=0;y<height;y++)if(visited[x,y]&&Vector2.Distance(Point(x,y),mission.entry)<1.2f){reachable=true;break;}
                Assert.That(reachable,Is.True,"Disconnected district: "+mission.id);
            }
            yield return null;
        }
        [UnityTest]public IEnumerator WorldCoordinatesAndMissionStepAreSavedSeparately()
        {
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();world.Navigate("24_Mocambo");player.Body.position=world.Find("24_Mocambo").entry;player.transform.position=player.Body.position;
            Assert.That(world.TryBegin("24_Mocambo"),Is.True);yield return null;world.Active.level.RestoreStep(2);world.SaveCheckpoint(world.Active.level);
            Assert.That(GameSession.Current.checkpointScene,Is.EqualTo(WorldHub.SceneName));Assert.That(GameSession.Current.worldMission,Is.EqualTo("24_Mocambo"));Assert.That(GameSession.Current.worldStep,Is.EqualTo(2));
            Vector2 at=player.Body.position;yield return SceneManager.LoadSceneAsync(WorldHub.SceneName);yield return null;
            Assert.That(WorldHub.Instance.Active.id,Is.EqualTo("24_Mocambo"));Assert.That(WorldHub.Instance.Active.level.Step,Is.EqualTo(2));
            Assert.That(Vector2.Distance(at,Object.FindAnyObjectByType<PlayerMotor>().Body.position),Is.LessThan(.2f));
        }
        [UnityTest,Timeout(90000)]public IEnumerator EveryMissionKeepsReachableObjectivesInItsWorldDistrict()
        {
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();var input=player.GetComponent<PlayerInputReader>();var navigation=Object.FindAnyObjectByType<GridNavigator>();GameSession.Current.pcAssembled=true;
            foreach(var mission in world.missions)
            {
                player.Body.position=mission.entry+Vector2.down*3;player.transform.position=player.Body.position;yield return null;
                world.Navigate(mission.id);player.Body.position=mission.entry;player.transform.position=mission.entry;
                Assert.That(world.TryBegin(mission.id),Is.True,mission.id);yield return null;
                player.GetComponent<StoryDialogue>().DismissForValidation();input.SetGameplayLocked(true);Physics2D.SyncTransforms();
                foreach(var target in mission.level.objectiveTargets)
                {
                    var path=navigation.Path(mission.entry,target.position);float range=target.GetComponent<PhotoTarget>()?4.2f:1.6f;
                    Assert.That(Vector2.Distance(mission.entry,target.position)<range||path.Count>0&&Vector2.Distance(path[path.Count-1],target.position)<range,Is.True,mission.id+" / "+target.name);
                }
                input.SetGameplayLocked(false);world.Complete(mission.next);yield return null;
                Assert.That(Object.FindObjectsByType<PlayerMotor>().Length,Is.EqualTo(1));
            }
        }
        [UnityTest]public IEnumerator ReplayingAMissionRecreatesItsActorsWithoutDuplicatingTheWorld()
        {
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();int geometry=Object.FindObjectsByType<BoxCollider2D>().Length;
            for(int run=0;run<2;run++)
            {
                var mission=world.Find("24_Mocambo");player.Body.position=mission.entry+Vector2.down*3;player.transform.position=player.Body.position;yield return null;
                world.Navigate(mission.id);player.Body.position=mission.entry;player.transform.position=player.Body.position;Assert.That(world.TryBegin(mission.id),Is.True);
                yield return null;world.Active.level.RestoreStep(4);yield return null;yield return null;
                var enemy=world.Active.level.GetComponent<CombatMission>().rules[4].enemies[0];Assert.That(enemy.GetComponent<Health>().Current,Is.EqualTo(enemy.GetComponent<Health>().maximum));
                enemy.GetComponent<Health>().Damage(10000,CombatTeam.Player);GroundWarning.Spawn(player.transform.position,2,5,20);
                world.Complete("07_Distribuidora");yield return null;
                Assert.That(Object.FindObjectsByType<BoxCollider2D>().Length,Is.EqualTo(geometry));Assert.That(Object.FindObjectsByType<GroundWarning>().Length,Is.Zero);
            }
        }
        [UnityTest]public IEnumerator NorthernDistrictsCannotSortThePlayerBehindTheMap()
        {
            var world=WorldHub.Instance;var player=Object.FindAnyObjectByType<PlayerMotor>();
            player.Body.position=new Vector2(0,65);player.transform.position=player.Body.position;yield return null;
            Assert.That(player.GetComponent<DirectionalAnimation>().bodySprite.sortingOrder,Is.GreaterThan(-5000));
            Assert.That(WorldSort.At(world.bounds.yMax),Is.GreaterThan(-5000));Assert.That(WorldSort.At(world.bounds.yMin),Is.LessThan(32767));
            var camera=Object.FindAnyObjectByType<CinemachineConfiner2D>();Assert.That(camera.BoundingShape2D.bounds.Contains(new Vector3(0,65,0)),Is.True);
        }
    }
}
