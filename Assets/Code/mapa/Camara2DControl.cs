using UnityEngine;

public class Camara2DControl : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomStep = 1f;
    public float zoomMin = 2f;
    public float zoomMax = 20f;
    public float zoomScrollSpeed = 10f;

    [Header("Arrastre")]
    public float dragSpeed = 1f;
    private Vector3 lastMousePosition;

    [Header("Rotación con clic derecho")]
    public float velocidadRotacion = 2f;
    private float rotacionZActual = 0f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic)
        {
            Debug.LogError("Este script requiere una cámara ortográfica.");
            enabled = false;
        }
    }

    void Update()
    {
        // Zoom con teclas
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            cam.orthographicSize = Mathf.Max(zoomMin, cam.orthographicSize - zoomStep);
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            cam.orthographicSize = Mathf.Min(zoomMax, cam.orthographicSize + zoomStep);

        // Zoom con scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - scroll * zoomScrollSpeed,
                zoomMin,
                zoomMax
            );
        }

        // Arrastre con clic izquierdo
        if (Input.GetMouseButtonDown(0))
            lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = cam.ScreenToWorldPoint(Input.mousePosition) - cam.ScreenToWorldPoint(lastMousePosition);
            transform.position -= delta;
            lastMousePosition = Input.mousePosition;
        }

        // Rotación con clic derecho
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            rotacionZActual -= mouseX * velocidadRotacion;
            transform.rotation = Quaternion.Euler(0f, 0f, rotacionZActual);
        }
    }
}
