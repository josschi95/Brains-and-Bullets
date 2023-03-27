using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Unity.Netcode;

public class PlayerLocomotion : NetworkBehaviour
{
    private PlayerInput playerInput;
    private CharacterController controller;
    private Animator anim;

    #region - Movement -
    [SerializeField] private float crawlSpeed = 1f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float sprintSpeed = 6f;

    private float locomotionSmoothing = 0.1f;
    private float movementSpeed;
    private float animSpeedParam;

    private Vector2 moveInput;
    private Vector3 movement;

    private Stance stance = Stance.Standing;
    private bool isWalking = false;
    private bool isSprinting = false;
    private Coroutine sprintCoroutine;
    #endregion

    #region - Rotation -
    [SerializeField] private Transform cameraRoot;
    private Vector2 lookInput;
    private Vector2 turn;

    private float lookSensitivity = 25f;

    [SerializeField] private float minY = -55, maxY = 75;
    private float mouseX, mouseY;
    #endregion

    private void Start()
    {
        if (!IsOwner) return;

        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        playerInput.actions["Walk"].performed += i => ToggleWalk();
        playerInput.actions["Sprint"].performed += i => OnSprint();
        playerInput.actions["Change Stance"].performed += context =>
        {
            if (context.interaction is HoldInteraction)
            {
                OnProne();
            }
            else if (context.interaction is PressInteraction)
            {
                OnCrouch();
            }
        };

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!IsOwner) return;

        playerInput.actions["Walk"].performed -= i => ToggleWalk();
        playerInput.actions["Sprint"].performed -= i => OnSprint();
        playerInput.actions["Change Stance"].performed -= context =>
        {
            if (context.interaction is HoldInteraction)
            {
                OnProne();
            }
            else if (context.interaction is PressInteraction)
            {
                OnCrouch();
            }
        };
    }

    private void Update()
    {
        GetInput();
        ControlSpeed();
        MovePlayer();
        SetAnimatorParameters();
    }

    private void GetInput()
    {
        if (!IsOwner) return;

        lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        movement = transform.forward * moveInput.y + transform.right * moveInput.x;
        movement.y = 0;

        mouseX = lookInput.x * Time.deltaTime;
        mouseY = lookInput.y * Time.deltaTime;

        turn.x += mouseX * lookSensitivity;
        turn.y -= mouseY * lookSensitivity;
        turn.y = Mathf.Clamp(turn.y, minY, maxY);
    }

    public void SetLookSensitivity(float sensitivity)
    {
        if (!IsOwner) return;

        lookSensitivity = sensitivity;
    }

    private void ControlSpeed()
    {
        if (!IsOwner) return;

        switch (stance)
        {
            case Stance.Standing:
                if (isWalking)
                {
                    movementSpeed = walkSpeed;
                    animSpeedParam = 1;
                }
                else if (isSprinting)
                {
                    movementSpeed = sprintSpeed;
                    animSpeedParam = 3;
                }
                else
                {
                    movementSpeed = runSpeed;
                    animSpeedParam = 2;
                }
                break;
            case Stance.Crouching:
                movementSpeed = crouchSpeed;
                animSpeedParam = 1;
                break;
            case Stance.Prone:
                movementSpeed = crawlSpeed;
                animSpeedParam = 1;
                break;
        }

        if (movement.x == 0 && movement.z == 0) animSpeedParam = 0;
    }

    private void MovePlayer()
    {
        if (!IsOwner) return;

        controller.Move(movement * movementSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, turn.x, 0);
        cameraRoot.transform.rotation = Quaternion.Euler(turn.y, turn.x, 0);
    }

    private void SetAnimatorParameters()
    {
        if (!IsOwner) return;

        anim.SetFloat("speed", animSpeedParam, locomotionSmoothing, Time.deltaTime);
        anim.SetFloat("horizontal", moveInput.x, locomotionSmoothing, Time.deltaTime);
        anim.SetFloat("vertical", moveInput.y, locomotionSmoothing, Time.deltaTime);
    }

    private void ToggleWalk()
    {
        if (!IsOwner) return;

        if (stance != Stance.Standing) return;

        isSprinting = false;
        isWalking = !isWalking;
    }

    private void OnSprint()
    {
        if (!IsOwner) return;

        if (stance != Stance.Standing) return;

        isWalking = false;
        isSprinting = !isSprinting;

        if (sprintCoroutine != null) StopCoroutine(sprintCoroutine);
        if (isSprinting) sprintCoroutine = StartCoroutine(SprintCoroutine());
    }

    private void OnJump()
    {
        if (!IsOwner) return;


    }

    private void OnCrouch() //press
    {
        if (!IsOwner) return;

        Debug.Log("OnCrouch");

        if (stance == Stance.Crouching)
        {
            //stand
        }
        else
        {
            //crouch
        }        
    }

    private void OnProne() //hold
    {
        if (!IsOwner) return;

        Debug.Log("OnProne");

        if (stance == Stance.Prone)
        {
            //stand
        }
        else
        {
            //prone
        }
    }

    private IEnumerator SprintCoroutine()
    {
        if (!IsOwner) yield break;

        while (isSprinting)
        {
            if (moveInput.y < 1) isSprinting = false;
            if (moveInput.x != 0) isSprinting = false;
            if (stance != Stance.Standing) isSprinting = false;
            yield return null;
        }
    }
}

public enum Stance { Standing, Crouching, Prone }
