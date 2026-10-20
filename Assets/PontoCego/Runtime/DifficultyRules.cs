using UnityEngine;
namespace PontoCego
{
    public enum GameDifficulty { Story, Normal, Hard }
    /// <summary>One saved difficulty contract. Normal retains the authored balance.</summary>
    public static class DifficultyRules
    {
        public static GameDifficulty Current=>(GameDifficulty)Mathf.Clamp(GameSession.Current.difficulty,0,2);
        public static string Name(GameDifficulty d)=>d==GameDifficulty.Story?"HISTÓRIA":d==GameDifficulty.Hard?"DIFÍCIL":"NORMAL";
        public static float EnemyLife(GameDifficulty d)=>d==GameDifficulty.Story?.75f:d==GameDifficulty.Hard?1.35f:1;
        public static float Incoming(GameDifficulty d)=>d==GameDifficulty.Story?.6f:d==GameDifficulty.Hard?1.35f:1;
        public static float EnemyPace(GameDifficulty d)=>d==GameDifficulty.Story?.85f:d==GameDifficulty.Hard?1.12f:1;
    }
}
