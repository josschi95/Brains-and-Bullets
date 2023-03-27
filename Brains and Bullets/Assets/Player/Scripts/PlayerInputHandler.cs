using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerLocomotion playerLocomotion;
    public Vector2 MoveInput { get; private set; }

    private void OnEnable()
    {
        playerInput.actions["Move"].performed += i => MoveInput = i.ReadValue<Vector2>();
        //playerInput.actions["Look"].performed += i => playerController.cameraLook = i.ReadValue<Vector2>();

    }

    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= i => MoveInput = i.ReadValue<Vector2>();
    }

}
