using LD59.Map;
using LD59.SO;
using UnityEngine;

namespace LD59.Manager
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance { private set; get; }

        [SerializeField]
        private Sprite _straight, _turn, _crossing, _tCrossing, _platform;

        [SerializeField]
        private Sprite _normalWagon, _commercialWagon, _highSpeedWagon;

        [SerializeField]
        private Sprite _lightOn, _lightOff;
        public Sprite LightOn => _lightOn;
        public Sprite LightOff => _lightOff;

        public RotatedSprite[] All { private set; get; }

        private void Awake()
        {
            Instance = this;

            All = new[]
            {
                GetHorizontal(),
                GetVertical(),
                GetTurnDownRight(),
                GetTurnUpRight(),
                GetTurnUpLeft(),
                GetTurnDownLeft(),
                GetCrossingAll(),
                GetCrossingTDownLeftRight(),
                GetCrossingTDownUpRight(),
                GetCrossingTUpLeftRight(),
                GetCrossingTDownUpLeft()
            };
        }

        public Sprite GetWagonSprite(TrainType type)
        {
            return type switch
            {
                TrainType.Normal => _normalWagon,
                TrainType.Commercial => _commercialWagon,
                TrainType.HighSpeed => _highSpeedWagon,
                _ => throw new System.NotImplementedException()
            };
        }

        public RotatedSprite GetHorizontal() => new() { Sprite = _straight, Rotation = 90f, Exits = Exit.Left | Exit.Right };
        public RotatedSprite GetVertical() => new() { Sprite = _straight, Rotation = 0f, Exits = Exit.Up | Exit.Down };
        public RotatedSprite GetHorizontalPlatform() => new() { Sprite = _platform, Rotation = 90f, Exits = Exit.Left | Exit.Right };
        public RotatedSprite GetVerticalPlatform() => new() { Sprite = _platform, Rotation = 0f, Exits = Exit.Up | Exit.Down };

        public RotatedSprite GetTurnDownRight() => new() { Sprite = _turn, Rotation = 0f, Exits = Exit.Down | Exit.Right };
        public RotatedSprite GetTurnUpRight() => new() { Sprite = _turn, Rotation = 90f, Exits = Exit.Up | Exit.Right };
        public RotatedSprite GetTurnUpLeft() => new() { Sprite = _turn, Rotation = 180f, Exits = Exit.Up | Exit.Left };
        public RotatedSprite GetTurnDownLeft() => new() { Sprite = _turn, Rotation = 270f, Exits = Exit.Down | Exit.Left };

        public RotatedSprite GetCrossingAll() => new() { Sprite = _crossing, Rotation = 0f, Exits = Exit.Up | Exit.Down | Exit.Left | Exit.Right };
        public RotatedSprite GetCrossingTDownLeftRight() => new() { Sprite = _tCrossing, Rotation = 0f, Exits = Exit.Down | Exit.Left | Exit.Right };
        public RotatedSprite GetCrossingTDownUpRight() => new() { Sprite = _tCrossing, Rotation = 90f, Exits = Exit.Up | Exit.Down | Exit.Right };
        public RotatedSprite GetCrossingTUpLeftRight() => new() { Sprite = _tCrossing, Rotation = 180f, Exits = Exit.Up | Exit.Left | Exit.Right };
        public RotatedSprite GetCrossingTDownUpLeft() => new() { Sprite = _tCrossing, Rotation = 270f, Exits = Exit.Up | Exit.Down | Exit.Left };
    }

    public record RotatedSprite
    {
        public Sprite Sprite;
        public float Rotation;
        public Exit Exits;
    }
}
