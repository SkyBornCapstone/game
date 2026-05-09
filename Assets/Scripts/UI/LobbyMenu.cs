using PurrNet;
using PurrNet.Transports;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LobbyMenu : NetworkBehaviour
    {
        public TextMeshProUGUI joinCodeText;
        public TextMeshProUGUI playerCountText;
        public Button startGameButton;

        private NetworkManager _networkManager;
        private PurrTransport _purrTransport;

        private void OnEnable()
        {
            _networkManager = NetworkManager.main;

            _purrTransport = _networkManager.transport as PurrTransport;

            if (_purrTransport == null)
            {
                Debug.LogError("Transport is not set to PurrTransport! This script requires it for Room Codes.");
            }

            _networkManager.onPlayerJoined += OnPlayerJoined;
            _networkManager.onPlayerLeft += OnPlayerLeft;

            startGameButton.interactable = _networkManager.isHost;
        }

        private void OnDisable()
        {
            _networkManager.onPlayerJoined -= OnPlayerJoined;
            _networkManager.onPlayerLeft -= OnPlayerLeft;
        }

        private void OnPlayerLeft(PlayerID player, bool asServer)
        {
            playerCountText.text = $"Player Count: {_networkManager.players.Count}";
        }

        private void OnPlayerJoined(PlayerID player, bool isReconnect, bool asServer)
        {
            playerCountText.text = $"Player Count: {_networkManager.players.Count}";
        }

        public void UpdateText()
        {
            joinCodeText.text = $"Room Code: {_purrTransport.roomName}";
            playerCountText.text = $"Player Count: {_networkManager.players.Count}";
        }

        public void LeaveLobby()
        {
            if (isHost)
            {
                _networkManager.StopServer();
            }

            _networkManager.StopClient();
        }

        public void StartGame()
        {
            networkManager.sceneModule.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}