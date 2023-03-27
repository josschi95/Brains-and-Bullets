using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerCameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraRoot;
    [SerializeField] private Camera cam;
    [SerializeField] private CinemachineVirtualCamera playerFollowCam;
    [SerializeField] private CinemachineVirtualCamera playerAimCam;

    private Cinemachine3rdPersonFollow followCam;
    private Cinemachine3rdPersonFollow aimCam;
    private PlayerLocomotion locomotion;

    [Space]

    [SerializeField] private float normalSensitivity = 25f;
    [SerializeField] private float aimSensitivity = 10f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Instantiate(cam);
        playerFollowCam = Instantiate(playerFollowCam);
        playerAimCam = Instantiate(playerAimCam);

        playerFollowCam.Follow = playerCameraRoot;
        playerAimCam.Follow = playerCameraRoot;
        playerAimCam.enabled = false;

        followCam = playerFollowCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        aimCam = playerAimCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    private void Start()
    {
        locomotion = GetComponent<PlayerLocomotion>();

        var input = GetComponent<PlayerInput>();
        input.actions["Aim"].performed += OnAimStart;
        input.actions["Aim"].canceled += OnAimEnd;
        input.actions["Lean"].performed += i => OnLean(i.ReadValue<float>());
    }

    public override void OnDestroy()
    {
        var input = GetComponent<PlayerInput>();
        input.actions["Aim"].performed -= OnAimStart;
        input.actions["Aim"].canceled -= OnAimEnd;
        input.actions["Lean"].performed -= i => OnLean(i.ReadValue<float>());
    }

    private void OnAimStart(InputAction.CallbackContext obj)
    {
        if (!IsOwner) return;
        playerAimCam.enabled = true;
        locomotion.SetLookSensitivity(aimSensitivity);
    }

    private void OnAimEnd(InputAction.CallbackContext obj)
    {
        if (!IsOwner) return;
        playerAimCam.enabled = false;
        locomotion.SetLookSensitivity(normalSensitivity);
    }

    private void OnLean(float value)
    {
        if (!IsOwner) return;
        value = Mathf.Clamp(value, 0, 1);
        StartCoroutine(SmoothCameraSide(value));
    }

    private IEnumerator SmoothCameraSide(float value)
    {
        float t = 0, timeToMove = 0.1f;
        while(t < timeToMove)
        {
            followCam.CameraSide = Mathf.Lerp(followCam.CameraSide, value, t / timeToMove);
            aimCam.CameraSide = Mathf.Lerp(aimCam.CameraSide, value, t / timeToMove);
            t += Time.deltaTime;
            yield return null;
        }
        followCam.CameraSide = value;
        aimCam.CameraSide = value;
    }
}
