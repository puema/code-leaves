using UnityEngine;

public class MovingManager : MonoBehaviour
{
    public float RotationSpeed = 1.0f;
    public float TranslationSpeed = .3f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isDragging;
    private Vector3 mouseDragOrigin;


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
            return;
        }

        // ----==== Modifier Keys ====---- //
        HandleModifierKeys();

        // ----==== Mouse ====---- //
        HandleMouse();

        // ----==== Arrow Keys ====---- //
        HandleArrowKeys();

        // ----==== WASD Keys ====---- //
        HandleWASDKeys();
    }

    private void HandleModifierKeys()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            RotationSpeed = 2 * RotationSpeed;
            TranslationSpeed = 2 * TranslationSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RotationSpeed = RotationSpeed / 2;
            TranslationSpeed = TranslationSpeed / 2;
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            mouseDragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseDragOrigin);

            transform.RotateAround(transform.position, transform.right, -pos.y * RotationSpeed);
            transform.RotateAround(transform.position, Vector3.up, pos.x * RotationSpeed);
        }
    }

    private void HandleArrowKeys()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * RotationSpeed);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.RotateAround(transform.position, Vector3.up, -Time.deltaTime * RotationSpeed);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.RotateAround(transform.position, transform.right, -Time.deltaTime * RotationSpeed);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.RotateAround(transform.position, transform.right, Time.deltaTime * RotationSpeed);
        }
    }

    private void HandleWASDKeys()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(TranslationSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(-TranslationSpeed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(0, 0, -TranslationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(0, 0, TranslationSpeed * Time.deltaTime);
        }
    }
}