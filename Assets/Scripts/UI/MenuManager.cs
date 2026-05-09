using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuManager : MonoBehaviour
    {
        public Button hostButton;
        public Button joinButton;
        public MainMenu mainMenu;
        public LobbyMenu lobbyMenu;

        private NetworkManager _networkManager;
        private PurrTransport _purrTransport;

        private void Start()
        {
            _networkManager = NetworkManager.main;

            _purrTransport = _networkManager.transport as PurrTransport;

            if (_purrTransport == null)
            {
                Debug.LogError("Transport is not set to PurrTransport! This script requires it for Room Codes.");
            }

            _purrTransport.onConnectionState += OnConnectionState;
        }

        private void OnDestroy()
        {
            _purrTransport.onConnectionState -= OnConnectionState;
        }

        private void OnConnectionState(ConnectionState state, bool asServer)
        {
            if (asServer) return;
            switch (state)
            {
                case ConnectionState.Connecting:
                    joinButton.interactable = false;
                    hostButton.interactable = false;
                    break;
                case ConnectionState.Connected:
                    lobbyMenu.gameObject.SetActive(true);
                    mainMenu.gameObject.SetActive(false);
                    lobbyMenu.UpdateText();
                    break;
                case ConnectionState.Disconnected:
                    lobbyMenu.gameObject.SetActive(false);
                    mainMenu.gameObject.SetActive(true);
                    joinButton.interactable = true;
                    hostButton.interactable = true;
                    break;
                case ConnectionState.Disconnecting:
                    break;
            }
        }
    }
}