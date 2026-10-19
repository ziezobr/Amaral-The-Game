using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PontoCego
{
    public enum MissionRuleKind{Manual,Reach,Fight}
    [Serializable]public sealed class MissionRule
    {
        public MissionRuleKind kind;
        public Transform target;
        public float radius=1.6f;
        public EnemyAgent[] enemies;
        public int reward=30;
        [TextArea]public string arrivalMessage;
    }
    public sealed class CombatMission:MonoBehaviour
    {
        public MissionRule[] rules;
        public string nextScene;
        public int stageNumber;
        public bool completed;
        public string LiveEncounterLabel{get;private set;}="";
        LevelProgress level;PlayerInputReader input;int activeStep=-1;
        void Start(){level=GetComponent<LevelProgress>();input=FindAnyObjectByType<PlayerInputReader>();}
        void Update()
        {
            if(!level||!input||completed||input.Paused||input.GameplayLocked)return;
            if(level.Step>=rules.Length)return;
            var rule=rules[level.Step];
            if(activeStep!=level.Step)
            {
                activeStep=level.Step;
                if(rule.enemies!=null)foreach(var e in rule.enemies)if(e)e.gameObject.SetActive(true);
                if(!string.IsNullOrEmpty(rule.arrivalMessage))FindAnyObjectByType<Interaction>().Say(rule.arrivalMessage);
            }
            if(rule.kind==MissionRuleKind.Reach&&rule.target&&Vector2.Distance(input.transform.position,rule.target.position)<rule.radius)CompleteRule(rule);
            if(rule.kind==MissionRuleKind.Fight)
            {
                int alive=0;if(rule.enemies!=null)foreach(var e in rule.enemies)if(e&&e.Alive)alive++;
                LiveEncounterLabel=alive>0?"HOSTIS  "+alive:"";
                if(alive==0)CompleteRule(rule);
            }
            else LiveEncounterLabel="";
        }
        void CompleteRule(MissionRule rule)
        {
            GameSession.RewardOnce((WorldHub.Instance&&WorldHub.Instance.Active!=null?WorldHub.Instance.Active.id:SceneManager.GetActiveScene().name)+":objective:"+level.Step,rule.reward);
            LiveEncounterLabel="";level.Advance();
            var health=input.GetComponent<Health>();if(health)health.Heal(8);
        }
        public void NotifyKill(EnemyAgent enemy){}
        public void CompleteStage()
        {
            if(completed)return;completed=true;GameSession.Current.completedStages++;
            if(!string.IsNullOrEmpty(nextScene))GameSession.Travel(nextScene);else FindAnyObjectByType<CampaignEnding>().Begin();
        }
    }
}
