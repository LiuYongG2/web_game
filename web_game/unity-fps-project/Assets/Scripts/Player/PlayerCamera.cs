using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Recoil")]
    public float recoilRecoverySpeed = 8f;

    [Header("References")]
    public Transform playerBody;

    private float xRotation;
    private float currentRecoilX;
    private float currentRecoilY;
    private float targetRecoilX;
    private float targetRecoilY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        targetRecoilX = Mathf.Lerp(targetRecoilX, 0, Time.deltaTime * recoilRecoverySpeed);
        targetRecoilY = Mathf.Lerp(targetRecoilY, 0, Time.deltaTime * recoilRecoverySpeed);
        currentRecoilX = Mathf.Lerp(currentRecoilX, targetRecoilX, Time.deltaTime * recoilRecoverySpeed * 2);
        currentRecoilY = Mathf.Lerp(currentRecoilY, targetRecoilY, Time.deltaTime * recoilRecoverySpeed * 2);

        xRotation -= mouseY + currentRecoilX;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * (mouseX + currentRecoilY));
    }

    public void AddRecoil(float upward, float sideways)
    {
        targetRecoilX += upward;
        targetRecoilY += Random.Range(-sideways, sideways);
    }

    public void ToggleCursor(bool visible)
    {
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }
}
