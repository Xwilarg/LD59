using LD59.Manager;
using UnityEngine;

namespace LD59.SO
{
    [CreateAssetMenu(menuName = "ScriptableObject/StoryLevelInfo", fileName = "StoryLevelInfo")]
    public class StoryLevelInfo : ScriptableObject
    {
        public TextAsset Intro;
        public StoryTrain[] Trains;

        [Header("Story")]
        public bool FreeJunctions;
        public bool FreeSignals;
    }

    [System.Serializable]
    public record StoryTrain
    {
        public Station From;
        public Station To;
        public float DepartureTime;
        public TrainType Type;
    }

    public enum TrainType
    {
        Normal,
        Commercial,
        HighSpeed
    }
}