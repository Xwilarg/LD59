using LD59.Manager;
using UnityEngine;

namespace LD59.SO
{
    [CreateAssetMenu(menuName = "ScriptableObject/StoryLevelInfo", fileName = "StoryLevelInfo")]
    public class StoryLevelInfo : ScriptableObject
    {
        public TextAsset Intro;
        public StoryTrain[] Trains;
    }

    [System.Serializable]
    public record StoryTrain
    {
        public Station From;
        public Station To;
        public float DepartureTime;
    }
}