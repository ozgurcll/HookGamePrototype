using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerFPS : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => TestInputJump() && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canUseHeadBob = true;
    [SerializeField] private bool WillSlideOnSlopes = true;



    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;


    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 7.0f;
    [SerializeField] private float sprintSpeed = 15.0f;
    [SerializeField] private float slopeSpeed = 8f;



    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedx = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    private Vector3 hitPointNormal;
    private bool IsSliding
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }

    }

    [Header("HookShot Scripts")]

    private const float NORMAL_FOV = 60f;
    private const float HOOKSHOT_FOV = 100f;
    private Vector3 characterVelocityMomentum;
    private State state;
    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform hookShotTransform;

    private Vector3 hookShotPosition;

    
    private float hookShotSize;

    private enum State
    {
        Normal,
        HookShotThrown,
        HookShotFlyingPlayer,
    }

    private Camera playerCamera;
    private CameraFov cameraFov;
    private CharacterController characterController;
    private Vector3 moveDirection;
    private float characterVelocityY;
    private Vector2 currentInput;
    private float rotationX = 0;
    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        cameraFov = playerCamera.GetComponent<CameraFov>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        hookShotTransform.gameObject.SetActive(false); 
        state = State.Normal;
    }


    void Update()
    {
        switch (state)
        {
            default:
            case State.Normal:
                if (CanMove)
                {
                    HandleMovementInput();
                    HandleMouseLook();
                    HandleHookShotStart();

                    if (canJump)
                    {
                        HandleJump();
                    }

                    if (canUseHeadBob)
                    {
                        HandleHeadBob();
                    }

                }
                break;
            case State.HookShotThrown:
                HandleHookShotThrown();
                HandleMovementInput();
                HandleMouseLook(); 

                 if (canJump)
                    {
                        HandleJump();
                    }

                    if (canUseHeadBob)
                    {
                        HandleHeadBob();
                    }

                break;
            case State.HookShotFlyingPlayer:
                HadnleHookShotMovement();
                HandleMouseLook();
                
                break;
        }


    }


    private void HandleMovementInput()
    {
        currentInput = new Vector2((IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical")
        , (IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;

        //bu kısım sorunlu burayı nere yazcağımızı tam olarak bilmiyoruz dk 14 de felan başlıyor sanırım sonra dönceğim
        moveDirection += characterVelocityMomentum;

        if (characterVelocityMomentum.magnitude >= 0f)
        {
            float momentumDrag = 3f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
            if (characterVelocityMomentum.magnitude < .0f)
            {
                characterVelocityMomentum = Vector3.zero;
            }
        }
    }

    private void HandleJump()
    {
         if (TestInputJump()) {
                float jumpSpeed = 18f;
                characterVelocityY = jumpSpeed;
            }
        

        // Apply gravity to the velocity
        float gravityDownForce = -30f;
        characterVelocityY += gravityDownForce * Time.deltaTime;


        // Apply Y velocity to move vector
        moveDirection.y = characterVelocityY;

        // Apply momentum
        moveDirection += characterVelocityMomentum;

        // Move Character Controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Dampen momentum
        if (characterVelocityMomentum.magnitude > 0f) {
            float momentumDrag = 3f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
            if (characterVelocityMomentum.magnitude < .0f) {
                characterVelocityMomentum = Vector3.zero;
            }
        }
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedx, 0);
    }

    
    private void HandleHeadBob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
            playerCamera.transform.localPosition.x,
            defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
            playerCamera.transform.localPosition.z);
        }

    }
    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
        {
            moveDirection.y += -60 * Time.deltaTime;
        }

        if (WillSlideOnSlopes && IsSliding)
        {
            moveDirection = new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void ResetGravityEffect()
    {
        moveDirection.y = 0f;
    }
    

    private void HandleHookShotStart()
    {
        if (TestInputDownHookShot())
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit))
            {
                debugHitPointTransform.position = raycastHit.point;
                hookShotPosition = raycastHit.point;
                hookShotSize = 0f;
                hookShotTransform.gameObject.SetActive(true);
                hookShotTransform.localScale = Vector3.zero;

                state = State.HookShotThrown;
            }
        }
    }

    private void HandleHookShotThrown()
    {
        hookShotTransform.LookAt(hookShotPosition);

        float hookShotThrowSpeed =500f;
        hookShotSize += hookShotThrowSpeed * Time.deltaTime;
        hookShotTransform.localScale = new Vector3(1, 1, hookShotSize);

        if (hookShotSize >= Vector3.Distance(transform.position, hookShotPosition))
        {
            state = State.HookShotFlyingPlayer;
            cameraFov.SetCameraFov(HOOKSHOT_FOV);
        }
    }

    private void HadnleHookShotMovement()
    {
        hookShotTransform.LookAt(hookShotPosition); 
        Vector3 hookShotDir = (hookShotPosition - transform.position).normalized;

        float hookShotSpeedMin = 10f;
        float hookShotSpeedMax = 40f;
        float hookShotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookShotPosition), hookShotSpeedMin, hookShotSpeedMax);
        float hookShotSpeedMultiplier = 2f;

        characterController.Move(hookShotDir * hookShotSpeed * hookShotSpeedMultiplier * Time.deltaTime);

        float reachedHookShotPositionDistance = 2f;
        if (Vector3.Distance(transform.position, hookShotPosition) < reachedHookShotPositionDistance)
        {
            StopHookShot();
        }

        if (TestInputDownHookShot())
        {
            StopHookShot();
        }

        if (TestInputJump())
        {
            //Bu kısım sorunlu burda karakter hook esnasındayken jumpa bastığı zaman ileri atılcakdı
            float momentumExtraSpeed = 1f;
            characterVelocityMomentum = hookShotDir * hookShotSpeed * momentumExtraSpeed;
            float jumpSpeed = 80f;
            characterVelocityMomentum += Vector3.up * jumpSpeed;
            StopHookShot();
        }
    }

    private void StopHookShot()
    {
        state = State.Normal;
        ResetGravityEffect();
        hookShotTransform.gameObject.SetActive(false);
        cameraFov.SetCameraFov(NORMAL_FOV);
    }

    private bool TestInputDownHookShot()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    private bool TestInputJump()
    {
        return Input.GetKeyDown(jumpKey);
    }

}
