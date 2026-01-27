using UnityEngine;

namespace Ship.ShipControllers
{
    public class ShipInputManager : MonoBehaviour
    {
        public static ShipInputManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public float GetControlInput()
        {
            // Returns -1 for Q, +1 for E, 0 for neither
            float input = 0f;
            
            if (Input.GetKey(KeyCode.E))
                input = 1f;
            else if (Input.GetKey(KeyCode.Q))
                input = -1f;
                
            return input;
        }
    }
}