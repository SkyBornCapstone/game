using PurrNet;
using PurrNet.Transports;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public TMP_InputField joinCodeInput;
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
        }


        public void HostGame()
        {
            if (_purrTransport == null) return;

            string newRoomCode = GenerateRandomCode(4);
            _purrTransport.roomName = newRoomCode;
            _networkManager.StartHost();
            Debug.Log($"Started Host with Room Code: {newRoomCode}");
        }

        public void JoinGame()
        {
            if (_purrTransport == null) return;

            string codeToJoin = joinCodeInput.text.ToUpper();
            if (string.IsNullOrEmpty(codeToJoin)) return;

            _purrTransport.roomName = codeToJoin;
            _networkManager.StartClient();
            Debug.Log($"Attempting to join Room: {codeToJoin}");
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game");
            Application.Quit();
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHJKMNPQRSTWXYZ23456789";
            char[] stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[Random.Range(0, chars.Length)];
            }

            return new string(stringChars);
        }
    }
}