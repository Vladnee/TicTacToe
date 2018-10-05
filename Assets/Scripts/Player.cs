using System.Collections;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    public class Player : NetworkBehaviour
    {
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            StartCoroutine(_delayedRegistration());
        }

        // we need to wait until GameController been awaked
        private IEnumerator _delayedRegistration()
        {
            while (GameController.Instance == null)
            {
                yield return null;
            }
            GameController.Instance.RegisterLocalPlayer(this);
            CmdReqisterPlayerOnServer();
        }

        [ClientRpc]  // create and setup play board 
        public void RpcClientSetupBoard(string mark)
        {
            if (isLocalPlayer)
            {
                GameView.Instance.SetupBoard(ClientInput);
            }
            GameView.Instance.SetMark(isLocalPlayer, mark);
        }

        [Command]
        public void CmdReqisterPlayerOnServer()
        {
            GameController.Instance.RegisterPlayerOnServer(this);
        }

        // method invoke when player click on board button
        public void ClientInput(int index)
        {
            CmdClientInput(index);
        }

        [Command]
        public void CmdClientInput(int index)
        {
            GameController.Instance.ClientInput(this, index);
        }
    }
}