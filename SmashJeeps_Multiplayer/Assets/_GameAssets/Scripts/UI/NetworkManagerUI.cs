using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.UI
{
    public class NetworkManagerUI : MonoBehaviour
    {
        [SerializeField] private Button startHostButton;
        [SerializeField] private Button startClientButton;

        private void Awake()
        {
            startHostButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
                Hide();
            });
            startClientButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
                Hide();
            });
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
