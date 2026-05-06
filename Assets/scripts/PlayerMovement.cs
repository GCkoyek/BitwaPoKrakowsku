using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
public class RigidbodyFPS : NetworkBehaviour
{
    [Header("Ruch")]
    public float speed = 5f;
    public float jumpForce = 5f;

    [Header("Kamera")]
    public Transform playerCamera;
    public float mouseSensitivity = 100f;

    [Header("Synchronizacja")]
    public float lerpRate = 12f;

    [Header("Widoczność (opcjonalne)")]
    public GameObject firstPersonObjects;
    public GameObject thirdPersonModel;

    private Rigidbody rb;
    private float xRotation = 0f;

    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Owner);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"[RigidbodyFPS] OnNetworkSpawn {name} IsOwner={IsOwner}, activeSelf={gameObject.activeSelf}, activeInHierarchy={gameObject.activeInHierarchy}");

        // Dodatkowa bezpieczna aktywacja (chyba że skrypt nie wykona się bo obiekt disabled — RelayManager i tak wymusi z zewnątrz)
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning($"{name} był disabled przy spawnie – próbuję włączyć tutaj.");
            gameObject.SetActive(true);
        }

        if (!IsOwner)
        {
            rb.isKinematic = true;

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);

            if (firstPersonObjects != null)
                firstPersonObjects.SetActive(false);

            if (thirdPersonModel != null)
                thirdPersonModel.SetActive(true);

            transform.position = netPosition.Value;
            transform.rotation = netRotation.Value;
        }
        else
        {
            rb.isKinematic = false;

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);

            if (firstPersonObjects != null)
                firstPersonObjects.SetActive(true);

            if (thirdPersonModel != null)
                thirdPersonModel.SetActive(false);

            if (playerCamera == null)
                Debug.LogWarning("RigidbodyFPS: playerCamera nie przypisana!");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            netPosition.Value = transform.position;
            netRotation.Value = transform.rotation;
        }

        Debug.Log($"[RigidbodyFPS] Post-setup {name} IsOwner={IsOwner}, activeSelf={gameObject.activeSelf}, activeInHierarchy={gameObject.activeInHierarchy}");
    }

    void Update()
    {
        if (IsOwner)
        {
            RotateCamera();
            HandleJumpInput();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, netPosition.Value, Time.deltaTime * lerpRate);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRotation.Value, Time.deltaTime * lerpRate);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        MovePlayer();

        netPosition.Value = transform.position;
        netRotation.Value = transform.rotation;
    }

    private void RotateCamera()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, mouseX, 0f));
    }

    private void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 velocity = move * speed;
        velocity.y = rb.velocity.y;
        rb.velocity = velocity;
    }

    private void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
