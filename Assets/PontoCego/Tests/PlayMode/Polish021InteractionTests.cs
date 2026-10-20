using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PontoCego.Tests
{
    public sealed class Polish021InteractionTests
    {
        [UnitySetUp]public IEnumerator Setup(){GameSession.ExploreNeighborhood("01_Riberal");yield return null;yield return null;}
        [UnityTest]public IEnumerator PlayerStopsAtWallButPassesRiberalDoor()
        {
            var world=WorldHub.Instance;world.enabled=false;var player=Object.FindAnyObjectByType<PlayerMotor>();player.enabled=false;
            var contract=Object.FindObjectsByType<SceneryFootprint2D>().First(c=>c.transform.parent.name.Contains("AGÊNCIA RIBERAL"));var rect=contract.groundBounds;
            player.Body.position=new Vector2(rect.xMin-.9f,rect.center.y);Physics2D.SyncTransforms();
            for(int i=0;i<35;i++){player.Body.linearVelocity=Vector2.right*4;yield return new WaitForFixedUpdate();}
            Assert.That(player.Body.position.x,Is.LessThan(rect.xMin+.1f),"Player crossed a solid wall");
            Vector2 gate=contract.entrances[0];player.Body.position=gate+Vector2.down*.9f;Physics2D.SyncTransforms();
            for(int i=0;i<40;i++){player.Body.linearVelocity=Vector2.up*3;yield return new WaitForFixedUpdate();}
            Assert.That(player.Body.position.y,Is.GreaterThan(gate.y+.5f),"Player cannot pass the visible doorway");player.Body.linearVelocity=Vector2.zero;player.enabled=true;world.enabled=true;
        }
        [UnityTest]public IEnumerator MateusPhotoAdvancesOnlyThroughCamera()
        {
            GameSession.Current.collected.Add("gold:start:0");GameSession.Current.collected.Add("gold:0:0");yield return null;yield return null;
            var task=Object.FindObjectsByType<GoldenQuestPickup>().First(x=>x.chain==0&&x.step==1);var player=Object.FindAnyObjectByType<PlayerMotor>();
            player.enabled=false;player.Body.position=(Vector2)task.transform.position+Vector2.down*2;player.transform.position=player.Body.position;player.GetComponent<PlayerInputReader>().Mobile(Vector2.zero,Vector2.up,false,false,false,false);Physics2D.SyncTransforms();yield return null;
            task.Interact();Assert.That(GoldenWeaponQuest.Step(0),Is.EqualTo(1),"Photo errand completed by pressing E");
            var wall=new GameObject("Photo line of sight blocker");wall.layer=LayerMask.NameToLayer("World");wall.transform.position=player.transform.position+Vector3.up;wall.AddComponent<BoxCollider2D>().size=new Vector2(2,.15f);Physics2D.SyncTransforms();
            Assert.That(task.TryPhotograph(player.GetComponent<PlayerAim>()),Is.False,"Must not photograph through a wall");Object.Destroy(wall);yield return null;Physics2D.SyncTransforms();
            Assert.That(task.TryPhotograph(player.GetComponent<PlayerAim>()),Is.True);Assert.That(GoldenWeaponQuest.Step(0),Is.EqualTo(2));yield return null;
        }
        [UnityTest]public IEnumerator InvestigationCancelsWhenPlayerWalksAway()
        {
            GameSession.Current.collected.Add("gold:start:0");var player=Object.FindAnyObjectByType<PlayerMotor>();player.enabled=false;
            var go=new GameObject("Investigation cancellation test");go.transform.position=player.transform.position;var task=go.AddComponent<GoldenQuestPickup>();task.chain=0;task.step=0;task.Interact();yield return new WaitForSeconds(.35f);
            player.Body.position+=Vector2.right;player.transform.position=player.Body.position;yield return null;Assert.That(GoldenWeaponQuest.Step(0),Is.Zero);Assert.That(task.WorkProgress,Is.Zero);Object.Destroy(go);
        }
    }
}
