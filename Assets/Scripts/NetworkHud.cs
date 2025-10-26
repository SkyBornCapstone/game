using System;
using FishNet;
using UnityEngine;

public class NetworkHud : MonoBehaviour
{
    private void OnGUI()
    {
        if (InstanceFinder.IsServerStarted)
        {
            if (GUILayout.Button("Stop Server")) InstanceFinder.ServerManager.StopConnection(true);
        }
        else
        {
            if (GUILayout.Button("Start Server")) InstanceFinder.ServerManager.StartConnection();
        }

        if (InstanceFinder.IsClientStarted)
        {
            if (GUILayout.Button("Stop Client")) InstanceFinder.ClientManager.StopConnection();
        }
        else
        {
            if (GUILayout.Button("Start Client")) InstanceFinder.ClientManager.StartConnection();
        }
    }
}
