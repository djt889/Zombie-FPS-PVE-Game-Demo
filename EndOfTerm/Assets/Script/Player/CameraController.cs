using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float clampAngle = 60f;

    private float yRotation = 0f;

    // 在Inspector中拖拽引用
    private void Awake()
    {
        if (!inputHandler)
        {
            inputHandler = FindObjectOfType<InputHandler>();
        }
    }

    private void Start()
    {
        CursorState(false);
    }

    private void Update()
    {
        HandleLookInput();
    }
    public void CursorState(bool iscursor = false)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = iscursor;
    }

    // 视角输入
    private void HandleLookInput()
    {
        Vector2 lookInput = inputHandler.LookInput;

        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * -mouseX);
    }
}