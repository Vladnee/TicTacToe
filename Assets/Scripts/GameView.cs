using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GameView : MonoBehaviour
    {
        public static GameView Instance;
        [SerializeField] private CanvasGroup _boardPanel;

        [SerializeField] private BoardButton _prefabBoardButton;
        [SerializeField] private Text _gameTitleText;

        [SerializeField] private Text _localCounterWins;
        [SerializeField] private Text _opponentCounterWins;

        [SerializeField] private Text _localMark;
        [SerializeField] private Text _opponentMark;

        private readonly List<BoardButton> _listBoardButtons = new List<BoardButton>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // fill the board with buttons with input hook
        public void SetupBoard(Action<int> action)
        {
            for (int i = 0; i < 9; i++)
            {
                BoardButton button = Instantiate(_prefabBoardButton, _boardPanel.transform);
                _listBoardButtons.Add(button);
                int index = i;
                button.AssignAction(() => action(index));
            }
        }

        public void ClearBoard()
        {
            foreach (var button in _listBoardButtons)
            {
                button.Clear();
            }
        }

        public void SetCountWins(bool isLocalPlayer, int count)
        {
            if (isLocalPlayer)
            {
                _localCounterWins.text = string.Format("Wins: {0}", count);
            }
            else
            {
                _opponentCounterWins.text = string.Format("Wins: {0}", count);
            }
        }

        public void SetMark(bool isLocalPlayer, string mark)
        {
            if (isLocalPlayer)
            {
                _localMark.text = mark;
            }
            else
            {
                _opponentMark.text = mark;
            }
        }

        public void SetBoardInteractable(bool interactable)
        {
            _boardPanel.interactable = interactable;
            _gameTitleText.text = interactable ? "You Move" : "Opponent Move";
        }

        public void DrawMark(int cellIndex, string playerMark)
        {
            _listBoardButtons[cellIndex].SetMark(playerMark);
        }

        public void SetWinner(int[] cells)
        {
            SetBoardInteractable(false);
            foreach (int index in cells)
                _listBoardButtons[index].SetColor(Color.green);
            _gameTitleText.text = "You Win :)";
        }

        public void SetLoser(int[] cells)
        {
            SetBoardInteractable(false);
            foreach (int index in cells)
                _listBoardButtons[index].SetColor(Color.red);
            _gameTitleText.text = "You Lose :(";
        }

        public void SetNoWinner()
        {
            SetBoardInteractable(false);
            _gameTitleText.text = "The Draw ;)";
        }
    }
}