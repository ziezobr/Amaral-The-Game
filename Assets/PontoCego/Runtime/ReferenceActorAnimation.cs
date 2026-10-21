using UnityEngine;
namespace PontoCego
{
    [DefaultExecutionOrder(40)]public sealed class ReferenceActorAnimation:MonoBehaviour
    {
        public SpriteRenderer visual;public Sprite[] idle,walk,run,idleCycle;public bool[] mirroredWalkDirections;Rigidbody2D body;PlayerMotor player;EnemyAgent enemy;CombatPlayer combat;AllyAgent ally;Vector2 facing=Vector2.down;Vector3 previous;float phase,idleTime,cadence,settle;bool wasMoving;int displayedDirection;
        public float GaitCycle{get;private set;}
        public static float FramesPerSecond(float speed,bool sprint,int count)=>Mathf.Clamp(speed*(sprint?.36f:.34f),sprint?1.8f:1.1f,sprint?2.5f:1.65f)*(count==3?4:Mathf.Max(1,count));
        public static int StableDirection(Vector2 direction,int previous){float angle=previous*Mathf.PI/4;Vector2 old=new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));return direction.sqrMagnitude<.001f||Vector2.Dot(direction.normalized,old)>Mathf.Cos(29*Mathf.Deg2Rad)?previous:DirectionIndex(direction);}
        public static int DirectionIndex(Vector2 direction)=>(Mathf.RoundToInt(Mathf.Atan2(-direction.x,-direction.y)/(Mathf.PI/4))+8)%8;
        public static int CycleFrame(float phase,int frames){if(frames<=1)return 0;int tick=Mathf.FloorToInt(phase);if(frames==3){int step=tick%4;return step==3?1:step;}return tick%frames;}
        void Start(){if(!GetComponent<DirectionalPose24>())gameObject.AddComponent<DirectionalPose24>();body=GetComponent<Rigidbody2D>();player=GetComponent<PlayerMotor>();enemy=GetComponent<EnemyAgent>();combat=GetComponent<CombatPlayer>();ally=GetComponent<AllyAgent>();previous=transform.position;phase=Mathf.Abs(transform.position.x+transform.position.y)%1;}
        void LateUpdate()
        {
            if(!visual||idle==null||idle.Length<8)return;if(enemy&&!enemy.Alive)return;
            var martial=GetComponent<HigorMartialArts>();if(martial&&martial.AttackVisualActive){NormalizeHeight();return;}
            Vector2 measured=(transform.position-previous)/Mathf.Max(.001f,Time.deltaTime);previous=transform.position;
            if(ally&&ally.AttackVisualActive){NormalizeHeight();return;}
            if(combat&&Time.time-combat.AttackStarted<.30f){NormalizeHeight();return;}
            if(enemy){var b=enemy.GetComponent<BossAbilities>();if(enemy.WindupProgress>0||b&&(b.Charging||b.Dashing)){NormalizeHeight();return;}}
            Vector2 velocity=body?body.linearVelocity:measured;if(velocity.sqrMagnitude>.04f)facing=velocity.normalized;else if(player)facing=player.Model.Facing;
            displayedDirection=StableDirection(facing,displayedDirection);int dir=displayedDirection;float speed=velocity.magnitude;
            bool moving=speed>.14f;bool sprint=player?player.Model.Sprinting:speed>3.3f;
            var frames=sprint&&run!=null&&run.Length>=24?run:walk;int count=frames==null?0:frames.Length/8;
            if(wasMoving&&!moving)settle=.10f;wasMoving=moving;settle=Mathf.Max(0,settle-Time.deltaTime);
            cadence=Mathf.Lerp(cadence,moving?FramesPerSecond(speed,sprint,count):0,1-Mathf.Exp(-12*Time.deltaTime));
            if(moving)phase+=Time.deltaTime*cadence;idleTime=moving?0:idleTime+Time.deltaTime;GaitCycle=Mathf.Repeat(phase/(count==3?4:Mathf.Max(1,count)),1);
            visual.flipX=(moving||settle>0)&&mirroredWalkDirections!=null&&mirroredWalkDirections.Length>dir&&mirroredWalkDirections[dir];
            if((moving||settle>0)&&count>0)visual.sprite=frames[dir*count+CycleFrame(phase,count)];
            else if(idleCycle!=null&&idleCycle.Length>=24)visual.sprite=idleCycle[dir*3+(idleTime%4.8f>4.35f?2:idleTime%4.8f>3.9f?1:0)];
            else visual.sprite=idle[dir];
            NormalizeHeight();visual.sortingOrder=WorldSort.At(transform.position.y);
        }
        void NormalizeHeight()
        {
            // Reference sheets mix different drawing sizes; locomotion and strikes share one height.
            if(visual.sprite&&idle[0])visual.transform.localScale=Vector3.one*(idle[0].bounds.size.y/Mathf.Max(.01f,visual.sprite.bounds.size.y));
        }
    }
}
