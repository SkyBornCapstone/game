using PurrNet;
using UnityEngine;

namespace player
{
    public class CursorControl : NetworkBehaviour
    {
        void Start()
        {
            // if (!isOwner) return;
            
            HideCursor();
        }

        public void ShowCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void HideCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}