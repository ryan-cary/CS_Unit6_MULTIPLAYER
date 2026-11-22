
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Input")]
    public InputActionReference moveAction;
    public InputActionReference shootAction;
    public InputActionReference gunRotateAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 100f;
    [SerializeField] private float turretSpeed = 120f;
    [SerializeField] private Transform turretPivot;

    [Header("Shooting")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private float fireCooldown = 0.5f;
    private float nextFireTime;

    [Header("Components")]
    [SerializeField] private GameObject cam;
    private TankHealth health;

    [Header("Network Authority")]
    public Material mat_localPlayer;

    // - Server keeps authoritative turrent angle
    private readonly NetworkVariable<float> turretAngle = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkPlayerStats stats;


    private void Awake()
    {
        health = GetComponent<TankHealth>();        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        cam.SetActive(IsOwner);
    }

    private void Start()
    {
        if (!IsSpawned || !IsOwner)
            return;

        GetComponentInChildren<MeshRenderer>().material = mat_localPlayer;
    }

    private void Update()
    {
        if (!IsSpawned)
            return;

        if (IsOwner && health.IsAlive.Value)
        {
            HandleMovement();
            HandleTurret();
            HandleShooting();
        }

        if (!IsOwner && turretPivot != null)
        {
            turretPivot.localRotation = Quaternion.Euler(0f, turretAngle.Value, 0f);
        }

        
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        float move = moveInput.y;
        float turn = moveInput.x;

        transform.position += transform.forward * move * moveSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.Self);
    }

    private void HandleTurret()
    {
        if (turretPivot == null)
            return;

        float turn = gunRotateAction.action.ReadValue<float>();

        if (Mathf.Approximately(turn, 0f)) return;

        turretPivot.Rotate(Vector3.up, turn * turretSpeed * Time.deltaTime, Space.Self);

        // - Send turret angle to the server to be replicated to its observers
        UpdateTurretRpc(turretPivot.localEulerAngles.y);
    }

    [Rpc(SendTo.Server)]
    private void UpdateTurretRpc(float angle)
    {        
        
        angle = Mathf.Repeat(angle, 360f);
        turretAngle.Value = angle;

        turretPivot.localRotation = Quaternion.Euler(0f, angle, 0f);        
    }

    private void HandleShooting()
    {
        if (shootAction.action.WasPressedThisFrame() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            FireRpc(OwnerClientId);
        }
    }

    [Rpc(SendTo.Server)]
    private void FireRpc(ulong clientId)
    {
        if (health == null || !health.IsAlive.Value)
            return;
        
        if (muzzlePoint == null || projectilePrefab == null)
            return;

        NetworkObject projectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        //projectile.Spawn();
        projectile.SpawnWithOwnership(clientId);
    }

    [Rpc(SendTo.Owner)]
    public void TeleportOwnerRpc(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }
}
