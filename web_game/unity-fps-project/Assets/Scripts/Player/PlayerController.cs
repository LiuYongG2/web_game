using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8.5f;
    public float crouchSpeed = 2.5f;
    public float slideSpeed = 12f;
    public float slideDuration = 0.6f;
    public float jumpForce = 7f;
    public float gravity = 20f;

    [Header("Heights")]
    public float standHeight = 2f;
    public float crouchHeight = 1.2f;
    public float slideCameraOffset = -0.4f;

    [Header("References")]
    public CharacterController controller;
    public Transform cameraHolder;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSliding;
    private bool isSprinting;
    private float slideTimer;
    private float originalCameraY;
    private float headBobTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null) controller = gameObject.AddComponent<CharacterController>();
        controller.height = standHeight;
        controller.center = Vector3.up * (standHeight / 2f);
        originalCameraY = cameraHolder != null ? cameraHolder.localPosition.y : 1.6f;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift) && v > 0 && !isCrouching;

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isSprinting && isGrounded && !isSliding && moveDir.magnitude > 0.1f)
            {
                StartSlide();
            }
            else if (!isSliding)
            {
                isCrouching = !isCrouching;
                UpdateCrouch();
            }
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                isSliding = false;
                isCrouching = Input.GetKey(KeyCode.C);
                UpdateCrouch();
            }
        }

        float currentSpeed = isSliding ? slideSpeed
            : isCrouching ? crouchSpeed
            : isSprinting ? sprintSpeed
            : walkSpeed;

        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = jumpForce;
            if (isCrouching)
            {
                isCrouching = false;
                isSliding = false;
                UpdateCrouch();
            }
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        ApplyHeadBob(moveDir.magnitude > 0.1f && isGrounded);
        ApplyCameraCrouch();
    }

    void StartSlide()
    {
        isSliding = true;
        isCrouching = true;
        slideTimer = slideDuration;
        UpdateCrouch();
    }

    void UpdateCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = targetHeight;
        controller.center = Vector3.up * (targetHeight / 2f);
    }

    void ApplyHeadBob(bool moving)
    {
        if (cameraHolder == null) return;
        if (moving)
        {
            headBobTimer += Time.deltaTime * (isSprinting ? 12f : 8f);
            float bobAmount = isSprinting ? 0.05f : 0.03f;
            float bobY = Mathf.Sin(headBobTimer) * bobAmount;
            float bobX = Mathf.Cos(headBobTimer * 0.5f) * bobAmount * 0.5f;
            cameraHolder.localPosition = new Vector3(bobX, originalCameraY + bobY, 0);
        }
        else
        {
            cameraHolder.localPosition = Vector3.Lerp(
                cameraHolder.localPosition,
                new Vector3(0, originalCameraY, 0),
                Time.deltaTime * 5f
            );
        }
    }

    void ApplyCameraCrouch()
    {
        if (cameraHolder == null) return;
        float targetY = isCrouching ? originalCameraY + slideCameraOffset : originalCameraY;
        float currentY = cameraHolder.localPosition.y;
        cameraHolder.localPosition = new Vector3(
            cameraHolder.localPosition.x,
            Mathf.Lerp(currentY, targetY, Time.deltaTime * 10f),
            0
        );
    }

    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;
    public bool IsSliding => isSliding;
    public bool IsGrounded => isGrounded;
}
