using UnityEngine;

namespace LD59.Manager
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance { private set; get; }

        [SerializeField]
        private Sprite _straight, _turn, _crossing, _tCrossing;

        private void Awake()
        {
            Instance = this;
        }

        public RotatedSprite GetHorizontal() => new() { Sprite = _straight, Rotation = 90f };
        public RotatedSprite GetVertical() => new() { Sprite = _straight, Rotation = 0f };

        public RotatedSprite GetTurnDownRight() => new() { Sprite = _turn, Rotation = 0f };
        public RotatedSprite GetTurnUpRight() => new() { Sprite = _turn, Rotation = 90f };
        public RotatedSprite GetTurnUpLeft() => new() { Sprite = _turn, Rotation = 180f };
        public RotatedSprite GetTurnDownLeft() => new() { Sprite = _turn, Rotation = 270f };

        public RotatedSprite GetCrossingAll() => new() { Sprite = _crossing, Rotation = 0f };
        public RotatedSprite GetCrossingTDownLeftRight() => new() { Sprite = _tCrossing, Rotation = 0f };
        public RotatedSprite GetCrossingTDownUpRight() => new() { Sprite = _tCrossing, Rotation = 90f };
        public RotatedSprite GetCrossingTUpLeftRight() => new() { Sprite = _tCrossing, Rotation = 180f };
        public RotatedSprite GetCrossingTDownUpLeft() => new() { Sprite = _tCrossing, Rotation = 270f };
    }

    public record RotatedSprite
    {
        public Sprite Sprite;
        public float Rotation;
    }
}
