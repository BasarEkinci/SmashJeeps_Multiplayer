using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace _GameAssets.Scripts.Player
{
    public class PlayerNetworkController : NetworkBehaviour
    {
        [SerializeField] private CinemachineCamera cam;

        public override void OnNetworkSpawn()
        {
            cam.gameObject.SetActive(IsOwner);
        }
    }
}
