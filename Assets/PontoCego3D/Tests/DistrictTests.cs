using System.Collections;using System.Linq;using NUnit.Framework;using UnityEngine;using UnityEngine.AI;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;using UnityEngine.SceneManagement;using UnityEngine.TestTools;
namespace PontoCego3D.Tests
{
    public sealed class DistrictTests
    {
        ThirdPersonMotor hero;OpenDistrict world;Gamepad pad;
        [UnitySetUp]public IEnumerator Setup(){Time.timeScale=1;yield return SceneManager.LoadSceneAsync("CentroAberto");yield return null;hero=Object.FindAnyObjectByType<ThirdPersonMotor>();world=Object.FindAnyObjectByType<OpenDistrict>();world.Capturing=true;hero.orbit.Manual=true;pad=InputSystem.AddDevice<Gamepad>();}
        [UnityTearDown]public IEnumerator Cleanup(){if(pad!=null)InputSystem.RemoveDevice(pad);Time.timeScale=1;yield return null;}
        [UnityTest]public IEnumerator PedestriansHaveNavigationAndActuallyWalk()
        {
            var agents=Object.FindObjectsByType<StreetPedestrian>().Where(p=>!p.hostile).ToArray();Assert.That(agents.Length,Is.EqualTo(36));
            Assert.That(agents.All(p=>p.GetComponent<NavMeshAgent>().isOnNavMesh),Is.True,"A pedestrian failed to reach the baked NavMesh.");
            var before=agents.Select(a=>a.transform.position).ToArray();yield return new WaitForSeconds(3);
            Assert.That(agents.Where((a,i)=>Vector3.Distance(a.transform.position,before[i])>.5f).Count(),Is.GreaterThan(20));
        }
        [UnityTest]public IEnumerator LocomotionUsesSkinningAndAnimatedBones()
        {
            Assert.That(hero.animator.isHuman,Is.True);Assert.That(hero.GetComponentsInChildren<SkinnedMeshRenderer>().Length,Is.GreaterThan(2));
            var leg=hero.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);hero.animator.Play("Locomotion");hero.animator.SetFloat("Speed",3.6f);hero.Locked=true;hero.enabled=false;
            yield return new WaitForSeconds(.2f);var before=leg.localRotation;float excursion=0;
            for(int i=0;i<12;i++){yield return new WaitForSeconds(.08f);excursion=Mathf.Max(excursion,Quaternion.Angle(before,leg.localRotation));}
            Assert.That(excursion,Is.GreaterThan(15),"The rigged leg must articulate across the locomotion cycle.");
        }
        [UnityTest]public IEnumerator MovementIsRelativeToCameraAndNormalizesDiagonals()
        {
            hero.Warp(new Vector3(0,.2f,-20));hero.orbit.yaw=0;InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.up});yield return new WaitForSeconds(.5f);float a=hero.Velocity.magnitude;
            hero.Warp(new Vector3(0,.2f,-20));InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(1,1)});yield return new WaitForSeconds(.5f);Assert.That(hero.Velocity.magnitude,Is.EqualTo(a).Within(.05f));
            hero.orbit.yaw=90;InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.up});yield return new WaitForSeconds(.5f);Assert.That(hero.Velocity.x,Is.GreaterThan(3));
        }
        [UnityTest]public IEnumerator AttacksRespectRangeWallsAndDodgeProtection()
        {
            hero.Warp(new Vector3(63,.2f,13));hero.visual.rotation=Quaternion.identity;hero.Locked=true;
            var enemy=world.encounter[0];enemy.SetActive(true);yield return null;enemy.GetComponent<StreetPedestrian>().enabled=false;var agent=enemy.GetComponent<NavMeshAgent>();agent.enabled=false;enemy.transform.position=hero.transform.position+Vector3.forward*1.3f;Physics.SyncTransforms();
            var hp=enemy.GetComponent<CombatActor>();float before=hp.Health;hero.GetComponent<CombatActor>().StartMelee("Jab",20);yield return new WaitForSeconds(.3f);Assert.That(hp.Health,Is.LessThan(before));
            yield return new WaitForSeconds(.5f);var wall=new GameObject("Wall test");wall.layer=8;wall.transform.position=hero.transform.position+new Vector3(0,1,.6f);wall.AddComponent<BoxCollider>().size=new Vector3(4,3,.12f);Physics.SyncTransforms();before=hp.Health;
            hero.GetComponent<CombatActor>().StartMelee("Cross",20);yield return new WaitForSeconds(.3f);Assert.That(hp.Health,Is.EqualTo(before));Object.Destroy(wall);
            var own=hero.GetComponent<CombatActor>();own.Protect(1);Assert.That(own.Damage(20,hp,Vector3.back),Is.False);
        }
        [UnityTest]public IEnumerator CiviliansFleeFromCombat()
        {
            var npc=Object.FindObjectsByType<StreetPedestrian>().First(p=>!p.hostile);npc.Flee(npc.transform.position+Vector3.left*2);Assert.That(npc.Fleeing,Is.True);Assert.That(npc.GetComponent<NavMeshAgent>().speed,Is.GreaterThan(4));yield return null;
        }
        [UnityTest]public IEnumerator CameraAvoidsWallsBehindPlayer()
        {
            hero.Warp(new Vector3(0,.2f,-20));hero.orbit.yaw=0;hero.orbit.pitch=0;hero.orbit.Snap();hero.Locked=true;
            var wall=new GameObject("Camera blocker");wall.layer=8;wall.transform.position=hero.transform.position+new Vector3(0,1.4f,-2);wall.AddComponent<BoxCollider>().size=new Vector3(4,4,.2f);Physics.SyncTransforms();yield return new WaitForSeconds(.2f);
            Assert.That(Vector3.Distance(hero.orbit.cameraRig.transform.position,hero.transform.position+Vector3.up*1.45f),Is.LessThan(2.1f));Object.Destroy(wall);
        }
    }
}
