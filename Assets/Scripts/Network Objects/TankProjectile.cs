using Unity.Netcode;
using UnityEngine;

public class TankProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float radius = 0.125f;
    [SerializeField] private int damage = 25;
    private float remainingLife;

    [SerializeField] private GameObject explosion_VFX;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            remainingLife = lifeTime;
    }

    private void FixedUpdate()
    {
        if (!IsServer || !IsSpawned)
            return;

        remainingLife -= Time.fixedDeltaTime;

        if (remainingLife <= 0f)
        {
            NetworkObject.Despawn();
            return;
        }

        float distance = speed * Time.fixedDeltaTime;


        // - Server sided collision detection
        if (Physics.SphereCast(transform.position, radius, transform.forward, out RaycastHit hit, distance))
        {
            if (hit.collider.TryGetComponent(out TankHealth health))
            {
                health.TakeDamage(damage, OwnerClientId);
            }

            else
            {
                TankHealth parentHealth = hit.collider.GetComponentInParent<TankHealth>();

                if (parentHealth != null) {
                    parentHealth.TakeDamage(damage, OwnerClientId);
                }
            }

            TriggerExplosion();
            NetworkObject.Despawn();
            return;
        }

        // Server-sided movement
        transform.position += transform.forward * distance;
    }

    private void TriggerExplosion()
    {
        if (!IsServer) return;

        PlayExplosionRpc(transform.position);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayExplosionRpc(Vector3 position)
    {
        Destroy(Instantiate(explosion_VFX, position, Quaternion.identity), 5);
    }
}
