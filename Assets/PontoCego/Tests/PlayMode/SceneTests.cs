using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace PontoCego.Tests
{
    public sealed class SceneTests
    {
        static Vector2 W(float u,float v)=>new Vector2((u-.5f)*36,(.5f-v)*24);
        [UnitySetUp]public IEnumerator Setup()
        {
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("00_Laboratorio");yield return null;
            Object.FindAnyObjectByType<PlayerInputReader>().SetPaused(false);
        }
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;yield return null;}
        [UnityTest]public IEnumerator RealRigidbodyMovesAndStops()
        {
            var m=Object.FindAnyObjectByType<PlayerMotor>();var input=m.GetComponent<PlayerInputReader>();Vector2 start=m.Body.position;
            input.Mobile(Vector2.right,Vector2.up,false,false,false,false);
            for(int n=0;n<25;n++)yield return new WaitForFixedUpdate();
            Assert.That(m.Body.position.x-start.x,Is.GreaterThan(1));Assert.That(m.GetComponent<PlayerAim>().Direction.y,Is.GreaterThan(.9f));
            input.Mobile(Vector2.zero,Vector2.up,false,false,false,false);for(int n=0;n<15;n++)yield return new WaitForFixedUpdate();
            Assert.That(m.Body.linearVelocity.magnitude,Is.LessThan(.01f));
        }
        [UnityTest]public IEnumerator DodgeCannotTunnelThroughPaintedDeskCollision()
        {
            var m=Object.FindAnyObjectByType<PlayerMotor>();m.Body.position=new Vector2(-9.1f,1.3f);yield return new WaitForFixedUpdate();
            m.GetComponent<PlayerInputReader>().Mobile(Vector2.right,Vector2.up,false,true,false,false);
            for(int n=0;n<20;n++)yield return new WaitForFixedUpdate();
            Assert.That(m.Body.position.x,Is.LessThan(-8.65f));
        }
        [UnityTest]public IEnumerator GamepadMovesAndAimsIndependently()
        {
            var pad=InputSystem.AddDevice<Gamepad>();try
            {
                InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.right,rightStick=Vector2.up});
                InputSystem.Update(); // Process the queued device state before stepping catch-up physics ticks.
                for(int n=0;n<25;n++)yield return new WaitForFixedUpdate();var m=Object.FindAnyObjectByType<PlayerMotor>();
                Assert.That(m.Body.linearVelocity.x,Is.GreaterThan(2));Assert.That(m.GetComponent<PlayerAim>().Direction.y,Is.GreaterThan(.9f));
            }finally{InputSystem.RemoveDevice(pad);}
        }
        [UnityTest]public IEnumerator CameraAndAuthoredSpritesAreAssigned()
        {
            var cm=Object.FindAnyObjectByType<CinemachineCamera>();Assert.That(cm.Follow,Is.EqualTo(Object.FindAnyObjectByType<PlayerMotor>().transform));
            Assert.That(cm.GetComponent<CinemachineConfiner2D>().BoundingShape2D,Is.Not.Null);
            var visual=Object.FindAnyObjectByType<DirectionalAnimation>().bodySprite;
            Assert.That(visual.sprite.texture.width,Is.GreaterThan(500));yield return null;
        }
        [UnityTest]public IEnumerator PaintedMapKeepsItsOriginalAspectAndCollisionScale()
        {
            var background=GameObject.Find("Cenário ilustrado • direção urbana").GetComponent<SpriteRenderer>();
            Assert.That(background.sprite.texture.width,Is.EqualTo(1536));
            Assert.That(background.sprite.texture.height,Is.EqualTo(1024));
            Assert.That(background.bounds.size.x,Is.EqualTo(36).Within(.001f));
            Assert.That(background.bounds.size.y,Is.EqualTo(24).Within(.001f));
            yield return null;
        }
        [UnityTest]public IEnumerator PrologueFollowsClockBossColleaguesAndLens()
        {
            yield return SceneManager.LoadSceneAsync("01_Riberal");yield return null;
            var input=Object.FindAnyObjectByType<PlayerInputReader>();input.SetPaused(false);var talk=input.GetComponent<StoryDialogue>();talk.DismissForValidation();
            var level=Object.FindAnyObjectByType<LevelProgress>();
            StoryInteractable Item(string name){foreach(var a in Object.FindObjectsByType<StoryInteractable>())if(a.displayName==name)return a;return null;}
            Item("Sair para o evento de gala").Interact();Assert.That(level.Step,Is.EqualTo(0));
            foreach(string name in new[]{"Bater ponto","Paulo","Ribeiro","Cristyan","Pegar a lente nova"})
            {
                int before=level.Step;Item(name).Interact();Assert.That(input.GameplayLocked,Is.True);
                Assert.That(level.Step,Is.EqualTo(before),"Objective must wait until dialogue ends.");talk.DismissForValidation();Assert.That(level.Step,Is.EqualTo(before+1));
            }
            Assert.That(level.Objective,Does.Contain("gala"));
            Item("Sair para o evento de gala").Interact();talk.DismissForValidation();yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo(WorldHub.SceneName));
            input=Object.FindAnyObjectByType<PlayerInputReader>();input.SetPaused(false);var motor=input.GetComponent<PlayerMotor>();
            var world=WorldHub.Instance;world.Navigate("02_Gala");motor.Body.position=world.Find("02_Gala").entry;motor.transform.position=motor.Body.position;Assert.That(world.TryBegin("02_Gala"),Is.True);yield return null;talk=input.GetComponent<StoryDialogue>();talk.DismissForValidation();
            level=world.Active.level;Vector2 offset=world.Active.center;motor.Body.position=W(.36f,.61f)+offset;yield return new WaitForFixedUpdate();
            input.Mobile(Vector2.zero,Vector2.up,false,false,false,true);yield return null;yield return null;
            Assert.That(level.Step,Is.EqualTo(1));
            motor.Body.position=W(.72f,.477f)+offset;yield return new WaitForSeconds(input.GetComponent<PlayerAim>().FlashRemaining+.06f);
            input.Mobile(Vector2.zero,Vector2.right,false,false,false,true);yield return null;yield return null;
            Assert.That(level.Step,Is.EqualTo(2),"The accidental flash must unlock the escape.");
            StoryInteractable escape=null;foreach(var a in Object.FindObjectsByType<StoryInteractable>())if(a.endsOpening)escape=a;
            escape.Interact();input.GetComponent<StoryDialogue>().DismissForValidation();
            yield return null;Assert.That(world.Destination,Is.EqualTo("03_Casa"));motor.Body.position=world.Find("03_Casa").entry+Vector2.down*3;motor.transform.position=motor.Body.position;yield return null;motor.Body.position=world.Find("03_Casa").entry;motor.transform.position=motor.Body.position;Assert.That(world.TryBegin("03_Casa"),Is.True);yield return null;
            Assert.That(Object.FindAnyObjectByType<GameplaySequence>(),Is.Not.Null);
        }
        [UnityTest]public IEnumerator DialogueLocksMovementUntilDismissed()
        {
            var m=Object.FindAnyObjectByType<PlayerMotor>();var input=m.GetComponent<PlayerInputReader>();var talk=m.GetComponent<StoryDialogue>();
            talk.Begin("Amaral",null,new[]{"Adryan depende de mim."});Vector2 start=m.Body.position;input.Mobile(Vector2.right,Vector2.up,true,true,false,false);
            for(int i=0;i<8;i++)yield return new WaitForFixedUpdate();Assert.That(Vector2.Distance(start,m.Body.position),Is.LessThan(.01f));
            talk.Advance();Assert.That(input.GameplayLocked,Is.False);
        }
        [UnityTest]public IEnumerator PhotographyRequiresClearLineOfSight()
        {
            yield return SceneManager.LoadSceneAsync("02_Gala");yield return null;var input=Object.FindAnyObjectByType<PlayerInputReader>();input.SetPaused(false);
            var level=Object.FindAnyObjectByType<LevelProgress>();var motor=input.GetComponent<PlayerMotor>();motor.Body.position=W(.36f,.63f);
            var wall=new GameObject("Test LOS wall");wall.layer=8;wall.transform.position=new Vector3(-5.04f,-1.2f,0);wall.AddComponent<BoxCollider2D>().size=new Vector2(2,.3f);
            yield return new WaitForFixedUpdate();input.Mobile(Vector2.zero,Vector2.up,false,false,false,true);yield return null;yield return null;
            Assert.That(level.Step,Is.EqualTo(0));Object.Destroy(wall);yield return new WaitForSeconds(input.GetComponent<PlayerAim>().FlashRemaining+.06f);
            input.Mobile(Vector2.zero,Vector2.up,false,false,false,true);yield return null;yield return null;Assert.That(level.Step,Is.EqualTo(1));
        }
        [UnityTest]public IEnumerator InteractionCannotReachThroughWalls()
        {
            var motor=Object.FindAnyObjectByType<PlayerMotor>();motor.Body.position=new Vector2(-.6f,-1.92f);
            var wall=new GameObject("Test interaction wall");wall.layer=8;wall.transform.position=new Vector3(-1.2f,-1.92f,0);wall.AddComponent<BoxCollider2D>().size=new Vector2(.1f,2);
            yield return new WaitForFixedUpdate();yield return null;Assert.That(motor.GetComponent<Interaction>().Prompt,Is.Empty);
            Object.Destroy(wall);yield return new WaitForFixedUpdate();yield return null;Assert.That(motor.GetComponent<Interaction>().Prompt,Is.EqualTo("Ribeiro"));
        }
        [UnityTest]public IEnumerator EveryOfficeObjectiveIsReachableAroundFurniture()
        {
            yield return SceneManager.LoadSceneAsync("01_Riberal");yield return new WaitForFixedUpdate();
            Object.FindAnyObjectByType<PlayerInputReader>().SetPaused(false);Object.FindAnyObjectByType<StoryDialogue>().DismissForValidation();
            var level=Object.FindAnyObjectByType<LevelProgress>();const int width=60,height=40;const float cell=.6f;
            Vector2 P(int x,int y)=>new Vector2(-17.7f+x*cell,-11.7f+y*cell);
            var visited=new bool[width,height];var queue=new Queue<Vector2Int>();Vector2 start=Object.FindAnyObjectByType<PlayerMotor>().Body.position;
            int sx=Mathf.RoundToInt((start.x+17.7f)/cell),sy=Mathf.RoundToInt((start.y+11.7f)/cell);queue.Enqueue(new Vector2Int(sx,sy));visited[sx,sy]=true;
            var neighbors=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};int mask=LayerMask.GetMask("World");
            while(queue.Count>0)
            {
                var point=queue.Dequeue();foreach(var delta in neighbors)
                {
                    var n=point+delta;if(n.x<0||n.x>=width||n.y<0||n.y>=height||visited[n.x,n.y])continue;
                    if(Physics2D.OverlapCircle(P(n.x,n.y),.26f,mask))continue;visited[n.x,n.y]=true;queue.Enqueue(n);
                }
            }
            foreach(var target in level.objectiveTargets)
            {
                bool reached=false;for(int x=0;x<width;x++)for(int y=0;y<height;y++)if(visited[x,y]&&Vector2.Distance(P(x,y),target.position)<1.2f)reached=true;
                Assert.That(reached,Is.True,"Objective unreachable: "+target.name);
            }
        }
    }
}
