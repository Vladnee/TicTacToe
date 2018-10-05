using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class GameController : NetworkBehaviour
    {
        public const string MarkTic = "X";
        public const string MarkTac = "O";

        public static GameController Instance;

        [HideInInspector]
        public Player LocalPlayer;

        private string[] _board = new string[9];

        private NetworkInstanceId _lastMoveNetId;
        private NetworkInstanceId _startMoveNetId;

        private readonly List<Player> _players = new List<Player>();

        private readonly Dictionary<NetworkInstanceId, string> _playerIdToMark = new Dictionary<NetworkInstanceId, string>();
        private readonly Dictionary<NetworkInstanceId, int> _playerIdToCountWins = new Dictionary<NetworkInstanceId, int>();

        private void Awake()
        {
            Instance = this;
        }

        // this method called from the player when the client become ready to start the game
        public void RegisterPlayerOnServer(Player player)
        {
            _players.Add(player);
            if (_players.Count == 2)
            {
                InitGame(_players);
            }
        }

        // register local player to have a posibility to easy get the local player link
        public void RegisterLocalPlayer(Player player)
        {
            LocalPlayer = player;
        }

        public void RestartGame()
        {
            _board = new string[9];
            _lastMoveNetId = _startMoveNetId;
            _startMoveNetId = _players.First(x => x.netId != _startMoveNetId).netId;
            RpcClearBoard();
            RpcChangeTurn(_lastMoveNetId);
        }

        // randomly choose the players marks and start the game 
        public void InitGame(List<Player> _players)
        {
            _playerIdToCountWins.Add(_players[0].netId, 0);
            _playerIdToCountWins.Add(_players[1].netId, 0);

            bool first = Random.Range(0, 2) == 0;

            _playerIdToMark.Add(_players[0].netId, first ? MarkTic : MarkTac);
            _playerIdToMark.Add(_players[1].netId, first ? MarkTac : MarkTic);

            _lastMoveNetId = first ? _players[1].netId : _players[0].netId;
            _startMoveNetId = first ? _players[0].netId : _players[1].netId;

            RpcChangeTurn(_lastMoveNetId);

            _players.ForEach(x =>
                x.RpcClientSetupBoard(_playerIdToMark[x.netId]));
        }

        // the hook from the player object when the real player choose his move
        public void ClientInput(Player player, int index)
        {
            if (_lastMoveNetId != player.netId)
            {
                if (_board[index] == null)
                {
                    _board[index] = _playerIdToMark[player.netId];

                    RpcDrawBoardMark(index, _board[index]);

                    int[] cells;
                    if (_checkBoardForWin(player.netId, out cells))
                    {
                        _playerIdToCountWins[player.netId]++;
                        RpcSetBoardWinner(player.netId, _playerIdToCountWins[player.netId], cells);
                        StartCoroutine(_restartGame());
                    }
                    else if (!_board.Any(string.IsNullOrEmpty))
                    {
                        RpcSetBoardNoWinner();
                        StartCoroutine(_restartGame());
                    }
                    else
                    {
                        _lastMoveNetId = player.netId;
                        RpcChangeTurn(_lastMoveNetId);
                    }
                }
            }
        }

        private IEnumerator _restartGame()
        {
            yield return new WaitForSeconds(1);
            RestartGame();
        }

        [ClientRpc] // change the input posibility
        public void RpcChangeTurn(NetworkInstanceId lastMove)
        {
            GameView.Instance.SetBoardInteractable(lastMove != LocalPlayer.netId);
        }

        [ClientRpc]
        public void RpcClearBoard()
        {
            GameView.Instance.ClearBoard();
        }

        [ClientRpc] // called on each player move to draw a mark
        public void RpcDrawBoardMark(int index, string mark)
        {
            GameView.Instance.DrawMark(index, mark);
        }

        [ClientRpc] // hook to visualize the winner
        public void RpcSetBoardWinner(NetworkInstanceId playerId, int countWins, int[] cells)
        {
            if (playerId == LocalPlayer.netId)
            {
                GameView.Instance.SetWinner(cells);
                GameView.Instance.SetCountWins(true, countWins);
            }
            else
            {
                GameView.Instance.SetLoser(cells);
                GameView.Instance.SetCountWins(false, countWins);
            }
        }

        [ClientRpc] // hook when there is no winner
        public void RpcSetBoardNoWinner()
        {
            GameView.Instance.SetNoWinner();
        }

        // in case that we have static board size, i've count all posible variants.
        // board indexes:
        // 0 1 2 
        // 3 4 5 
        // 6 7 8
        private readonly int[][] _variants = new int[][]
        {
            // rows
            new int[] {0,1,2},
            new int[] {3,4,5},
            new int[] {6,7,8},
            // columns
            new int[] {0,3,6},
            new int[] {1,4,7},
            new int[] {2,5,8},
            // diagonals
            new int[] {0,4,8},
            new int[] {2,4,6},
        };

        private bool _checkBoardForWin(NetworkInstanceId netId, out int[] cells)
        {
            string mark = _playerIdToMark[netId];

            foreach (var variant in _variants)
            {
                if (variant.All(x => _board[x] == mark))
                {
                    cells = variant;
                    return true;
                }
            }
            cells = default(int[]);
            return false;

        }


    }
}
