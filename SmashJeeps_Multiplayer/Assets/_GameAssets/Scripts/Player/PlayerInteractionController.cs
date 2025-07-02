using _GameAssets.Scripts.Collectables;
using Unity.Netcode;
using UnityEngine;

namespace _GameAssets.Scripts.Player
{
    public class PlayerInteractionController : NetworkBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (IsOwner) { return; }
            
            if (other.TryGetComponent(out ICollectible collectible))
            {
                collectible.Collect();
            }
        }
    }
}
