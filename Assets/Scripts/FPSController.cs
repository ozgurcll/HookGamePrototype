using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    public bool IsSprinting => canSprint && SprintInput();
    public bool canSprint = true;
    private bool WillSlideOnSlopes = true;
    private Animator anim;

    [Header("Key Scripts")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode hookShotKey = KeyCode.E;

    [Header("Camera Scripts")]
    private const float normal_fov = 60f;
    private const float running_fov = 90f;
    private float hookshot_fov = 120f;
    private Camera playerCamera;
    private CameraFov cameraFov;


    [Header("Mouse Look Scripts")]
    // [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField, Range(1, 10)] private float lookSpeedx = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
    // [SerializeField] float cameraVerticalAngle;
    private float rotationX;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0f;
    private float timer;


    [Header("Character Controller Scripts")]
    private CharacterController characterController;
    private Vector3 characterVelocity;
    private float characterVelocityY;
    private float moveX;
    private float moveZ;

    //CurrentInput profesyonelce kullanımı var
    private Vector2 currentInput;

    [Header("Movement Scripts")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 30f;
    [SerializeField] private float slopeSpeed = 15f;

    [Header("Jumping Scripts")]
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float gravity = -60f;

     public float doubleJumpForce = 4f;
    public int maxJumps = 2;

    private int jumpCount = 0;
    private Vector3 moveDirection = Vector3.zero;


    private Vector3 characterVelocityMomentum;

    [Header("HookShot Scripts")]

    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform hookShotTransform;
    private State state;
    private Vector3 hookShotPosition;
    private float hookShotSize;


    [Header("Effects")]

    private ParticleSystem speedLinesParticleSystem;
    private ParticleSystem hookFlyEffect;
    private ParticleSystem fallEffect;

    private enum State
    {
        Normal,
        HookShotThrown,
        HookShotFlyingPlayer,
    }

    Vector3 hitPointNormal;
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
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        speedLinesParticleSystem = transform.Find("Camera").Find("SpeedLinesParticleSystem").GetComponent<ParticleSystem>();
        hookFlyEffect = transform.Find("HookFlyEffect").Find("HookFlyEffectVFX").GetComponent<ParticleSystem>();
        fallEffect = transform.Find("FallEffect").Find("FallEffectVFX").GetComponent<ParticleSystem>();
        anim = GetComponent<Animator>();
        cameraFov = playerCamera.GetComponent<CameraFov>();
        Cursor.lockState = CursorLockMode.Locked;
        state = State.Normal;
        hookShotTransform.gameObject.SetActive(false);

    }

    private void Update()
    {
        switch (state)
        {
            default:
            case State.Normal:
                MouseLook();
                Movement();
                Jump();
                HookShotStart();
                HeadBob();
                Sliding();
                break;
            case State.HookShotThrown:
                HookshotThrow();
                MouseLook();
                Movement();
                HeadBob();

                break;
            case State.HookShotFlyingPlayer:
                MouseLook();
                HookshotMovement();
                HeadBob();
                break;
        }



    }

    private void MouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedx, 0);
    }

    private void Movement()
    {
        if (!SprintInput())
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveZ = Input.GetAxisRaw("Vertical");

            characterVelocity = transform.right * moveX * walkSpeed + transform.forward * moveZ * walkSpeed;

            characterVelocityY += gravity * Time.deltaTime;
            characterVelocity.y = characterVelocityY;

            //Momentum
            characterVelocity += characterVelocityMomentum;

            characterController.Move(characterVelocity * Time.deltaTime);

            if (characterVelocityMomentum.magnitude > 0f)
            {
                float momentumDrag = 3f;
                characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
                if (characterVelocityMomentum.magnitude < .0f)
                {
                    characterVelocityMomentum = Vector3.zero;
                }
            }
        }

        else if (SprintInput())
        {
            currentInput = new Vector2((IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical")
    , (IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));
            characterVelocityY = characterVelocity.y;
            characterVelocity = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
            characterVelocity.y = characterVelocityY;

        }

    }

    private void Sliding() //Character Controllerda verdiğimiz değer kadar yukarı çıkar
    {
        if (WillSlideOnSlopes && IsSliding)
        {
            characterVelocity = new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
        characterController.Move(characterVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (characterController.isGrounded)
        {
            characterVelocityY = 0f;

            if (JumpInput())
            {
                Debug.Log("Space basıldı");
                characterVelocityY = jumpForce;
            }
            

        }
        //FallControl();

    }

    /* private void FallControl()
     {
         if (characterVelocityY <= 0)
         {
             fallEffect.Stop();
             anim.SetBool("Fall" , false);
             Debug.Log("FALLEFFECT false");
         }
         else if(characterVelocityY >= 0 && characterVelocityY <= 1)
         {
             fallEffect.Play();
             anim.SetBool("Fall" , true);
             Debug.Log("FALL");
         }
     }*/

    private void HeadBob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(characterVelocity.x) > 0.1f || Mathf.Abs(characterVelocity.z) > 0.1f)
        {
            timer += Time.deltaTime * (walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
            playerCamera.transform.localPosition.x,
            defaultYPos + Mathf.Sin(timer) * (walkBobAmount),
            playerCamera.transform.localPosition.z);
        }
    }


    private void ResetGravityEffect()
    {
        characterVelocityY = 0f;
    }

    private void HookShotStart()
    {
        if (HookShotInput())
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

    private void HookshotThrow()
    {
        hookShotTransform.LookAt(hookShotPosition);

        float hookshotThrowSpeed = 500f;
        hookShotSize += hookshotThrowSpeed * Time.deltaTime;
        hookShotTransform.localScale = new Vector3(1, 1, hookShotSize);

        if (hookShotSize >= Vector3.Distance(transform.position, hookShotPosition))
        {
            state = State.HookShotFlyingPlayer;
            cameraFov.SetCameraFov(hookshot_fov);
            speedLinesParticleSystem.Play();
            hookFlyEffect.Play();
        }
    }


    private void HookshotMovement()
    {
        hookShotTransform.LookAt(hookShotPosition);

        Vector3 hookshotDir = (hookShotPosition - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 40f;
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookShotPosition), hookshotSpeedMin, hookshotSpeedMax);
        float hookshotSpeedMultiplier = 5f;




        // Move Character Controller
        characterController.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        float reachedHookshotPositionDistance = 1f;
        if (Vector3.Distance(transform.position, hookShotPosition) < reachedHookshotPositionDistance)
        {
            // Reached Hookshot Position
            StopHookshot();
        }

        if (HookShotInput())
        {
            // Cancel Hookshot
            StopHookshot();
        }

        if (JumpInput())
        {
            // Cancelled with Jump
            float momentumExtraSpeed = 7f;
            characterVelocityMomentum = hookshotDir * hookshotSpeed * momentumExtraSpeed;
            float jumpSpeed = 40f;
            characterVelocityMomentum += Vector3.up * jumpSpeed;
            StopHookshot();
        }

    }

    private void StopHookshot()
    {
        state = State.Normal;
        ResetGravityEffect();
        hookShotTransform.gameObject.SetActive(false);
        cameraFov.SetCameraFov(normal_fov);
        speedLinesParticleSystem.Stop();
        hookFlyEffect.Stop();
    }








    #region Input Atamaları

    private bool HookShotInput()
    {
        return Input.GetKeyDown(hookShotKey);
    }
    private bool JumpInput()
    {
        return Input.GetKeyDown(jumpKey);
    }

    private bool SprintInput()
    {
        return Input.GetKey(sprintKey);
    }

    #endregion
}
