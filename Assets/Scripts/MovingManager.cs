using UnityEngine;

public class MovingManager : MonoBehaviour
{
    public float ArrowSpeed = 10;
    public float MovementSpeed = .3f;
    public float MouseSpeed = 5;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isDragging;
    private float mouseX;
    private float mouseY;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            ResetMouse();
            return;
        }

        // ----==== Modifier Keys ====---- //
        HandleModifierKeys();
        
        // ----==== Mouse ====---- //
        HandleMouse();

        // ----==== Orbit ====---- //
        HandleArrowKeys();

        // ----==== Plane Movement ====---- //
        HandleWASDKeys();

        // ----==== Up/Down ====---- //
        HandleQEKeys();
    }

    private void HandleModifierKeys()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            IncreaseSpeed(2);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            ReduceSpeed(2);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftCommand))
        {
            ReduceSpeed(2);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftCommand))
        {
            IncreaseSpeed(2);
        }
    }

    private void IncreaseSpeed(int amount)
    {
        ArrowSpeed = amount * ArrowSpeed;
        MovementSpeed = amount * MovementSpeed;
        MouseSpeed = amount * MouseSpeed;
    }

    private void ReduceSpeed(int amount)
    {
        ArrowSpeed = ArrowSpeed / amount;
        MovementSpeed = MovementSpeed / amount;
        MouseSpeed = MouseSpeed / amount;
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0)) isDragging = true;
        if (Input.GetMouseButtonUp(0)) isDragging = false;
        if (!isDragging) return;

        mouseX += Input.GetAxis("Mouse X") * MouseSpeed;
        mouseY += Input.GetAxis("Mouse Y") * MouseSpeed;

        var desiredRotation = Quaternion.Euler(-mouseY, mouseX, 0);
        var currentRotation = transform.rotation;

        transform.rotation = Quaternion.Lerp(currentRotation, desiredRotation, 1);
    }

    private void HandleArrowKeys()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * ArrowSpeed);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.RotateAround(transform.position, Vector3.up, -Time.deltaTime * ArrowSpeed);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.RotateAround(transform.position, transform.right, -Time.deltaTime * ArrowSpeed);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Debug.Log(mouseY);
            transform.RotateAround(transform.position, transform.right, Time.deltaTime * ArrowSpeed);
        }
    }

    private void HandleWASDKeys()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(MovementSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(-MovementSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(0, 0, -MovementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(0, 0, MovementSpeed * Time.deltaTime);
        }
    }

    private void HandleQEKeys()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(0, -MovementSpeed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(0, MovementSpeed * Time.deltaTime, 0);
        }
    }

    private void ResetMouse()
    {
        mouseX = 0;
        mouseY = 0;
    }
}