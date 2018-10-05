using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class BoardButton : MonoBehaviour
    {
        [SerializeField] private Text _markText;

        [SerializeField] private Image _image;

        [SerializeField] private Button _button;

        public void Clear()
        {
            _image.color = Color.white;
            _markText.text = "";
        }

        public void AssignAction(UnityAction action)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(action);
        }

        public void SetMark(string mark)
        {
            _markText.text = mark;
        }

        public void SetColor(Color clr)
        {
            _image.color = clr;
        }

        public bool CheckIsMark(string mark)
        {
            return _markText.text == mark;
        }
    }
}