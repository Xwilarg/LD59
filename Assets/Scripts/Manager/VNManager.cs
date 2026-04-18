using Ink.Runtime;
using LD59.VN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Sketch.VN
{
    [Serializable]
    public class CharacterImageOverlay
    {
        public string Tag;
        public Image Image;
        public bool IsSet { set; get; }
    }

    public class VNManager : MonoBehaviour
    {
        public static VNManager Instance { private set; get; }

        [SerializeField, Tooltip("Text that will show your visual novel story")]
        private TextDisplay _display;

        private Story _story;

        [Header("Displayed sprite")]
        [SerializeField, Tooltip("Where the image of the character will be shown")]
        private Image _characterImage;

        [Header("Interface")]
        [SerializeField, Tooltip("Object that contains all the others visual novel components")]
        private GameObject _container;

        [SerializeField, Tooltip("Pannel around the name text")]
        private GameObject _namePanel;

        [SerializeField, Tooltip("Text that show the name of the character")]
        private TMP_Text _nameText;

        private bool _isStoryOngoing;

        private Action _onDone;

        private void Awake()
        {
            Instance = this;

            if (_container == null) // If user didn't give a container, we just use the one of the text instead
            {
                _container = _display.gameObject;
            }
            _container.SetActive(false);
        }

        public bool IsStoryOngoing => _isStoryOngoing;

        /// <summary>
        /// Start showing a story to the player
        /// </summary>
        /// <param name="asset">Compiled Ink file containing the story</param>
        /// <param name="updateVariables">Method taking a VariablesState as parameter, allow to update the variables within the Ink file</param>
        /// <param name="onDone">Called once the story is done being read</param>
        /// <param name="onTags">Called upon an unknown tag is found, first parameter is the tag name and second is its value, function expect to return if the tag was treated or not</param>
        public void ShowStory(TextAsset asset, Action<VariablesState> updateVariables = null, Action onDone = null, Func<string, string, bool> onTags = null)
        {
            _story = new(asset.text);
            updateVariables?.Invoke(_story.variablesState);
            _onDone = onDone;

            _container.SetActive(true);

            _isStoryOngoing = true;
            DisplayStory(_story.Continue());
        }

        private void DisplayStory(string text)
        {
            _container.SetActive(true);

            if (_nameText != null)
            {
                _namePanel?.SetActive(false);
                _nameText.text = string.Empty;
            }

            foreach (var tag in _story.currentTags)
            {
                var s = tag.Split(' ');
                var content = string.Join(' ', s.Skip(1));
                switch (s[0])
                {
                    case "speaker":
                        break;

                    default:
                        throw new System.NotImplementedException($"Unknown tag {s[0]}");
                }
            }
            _display.ToDisplay = text;
        }

        /// <summary>
        /// Display the next dialogue if available
        /// </summary>
        public void DisplayNextDialogue()
        {
            if (!_container.activeInHierarchy)
            {
                return;
            }
            if (!_display.IsDisplayDone)
            {
                // We are slowly displaying a text, force the whole display
                _display.ForceDisplay();
            }
            else if (_story.currentChoices.Any())
            {
                // Waiting for the user to input a choice
            }
            else if (_story.canContinue && // There is text left to write
                !_story.currentChoices.Any()) // We are not currently in a choice
            {
                DisplayStory(_story.Continue());
            }
            else if (IsStoryOngoing)
            {
                _container.SetActive(false);
                _isStoryOngoing = false;
                _onDone?.Invoke();
            }
        }
    }
}