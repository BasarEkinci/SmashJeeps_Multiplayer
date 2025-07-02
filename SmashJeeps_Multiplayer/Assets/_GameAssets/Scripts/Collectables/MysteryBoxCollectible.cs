using _GameAssets.Scripts.Helpers;
using Unity.Netcode;
using UnityEngine;
namespace _GameAssets.Scripts.Collectables
{
    public class MysteryBoxCollectible : NetworkBehaviour,ICollectible
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Collider boxCollider;

        [Header("Settings")] 
        [SerializeField] private float respawnTime;
        
        public void Collect()
        {
            CollectRpc();
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void CollectRpc()
        {
            AnimateCollectible();
            Invoke(nameof(Respawn),respawnTime);
        }

        private void AnimateCollectible()
        {
            animator.SetTrigger(Consts.BoxAnimations.IS_COLLECTED);
            boxCollider.enabled = false;
        }

        private void Respawn()
        {
            animator.SetTrigger(Consts.BoxAnimations.IS_RESPAWNED);
            boxCollider.enabled = true;
        }
    }
}