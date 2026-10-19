using System;
using UnityEngine;
namespace PontoCego
{
    [Serializable]public sealed class CityMission
    {
        public string id,next;public GameObject actors;public LevelProgress level;
        public Vector2 center,size,entry;public WorldMissionTrigger trigger;
        public bool road;
        [NonSerialized]public GameObject template;
        [NonSerialized]public LevelProgress templateLevel;
    }
}
