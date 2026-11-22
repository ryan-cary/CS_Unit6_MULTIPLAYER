using UnityEngine;
using Unity.Netcode;

public class Landmine : NetworkBehaviour
{
    [SerializeField] private float radius = 0.125f;
    [SerializeField] private int damage = 25;

    [SerializeField] private float beepTimer = 2;
    private float _timer;

    [SerializeField] private GameObject explosion_VFX;

    private void FixedUpdate()
    {
        if (!IsServer || !IsSpawned)
            return;

        _timer += Time.fixedDeltaTime;

        if (_timer >= beepTimer)
        {
            //Beep();
        }

        // - Server sided collision detection
        if (Physics.SphereCast(transform.position, radius, transform.up, out RaycastHit hit))
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
        }
    }

    private void TriggerExplosion()
    {
        if (!IsServer) return;

        PlayExplosionRpc(transform.position);
    }

    /*
    private void Beep()
    {
        if (!IsServer) return;

        PlayBeepRpc(transform.position);
    }
    */

    [Rpc(SendTo.Everyone)]
    private void PlayExplosionRpc(Vector3 position)
    {
        Destroy(Instantiate(explosion_VFX, position, Quaternion.identity), 5);
    }
}
