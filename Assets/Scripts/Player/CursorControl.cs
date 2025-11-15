using UnityEngine;

namespace player
{
    public class CursorControl : MonoBehaviour
    {
        void Start()
        {

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; 
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
