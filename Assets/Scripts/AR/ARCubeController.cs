using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class ARCubeController : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 60f;   // degrees per second
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Colors")]
    [SerializeField] private Color[] colors = {
        Color.red, Color.green, Color.blue, Color.yellow, Color.cyan
    };

    [Header("Pinch Scale")]
    [SerializeField] private float maxScaleMultiplier = 3f;
    [SerializeField] private float pinchSensitivity = 0.01f; // scale change per pixel of pinch delta

    private Camera arCamera;
    private Renderer cubeRenderer;
    private bool isRotating = false;
    private int colorIndex = 0;

    // scale state
    private Vector3 originalScale;
    private float currentScaleFactor = 1f; // 1 = original, up to maxScaleMultiplier
    private float previousPinchDistance;
    private bool isPinching = false;

    // one-finger drag state
    private bool isDragging = false;
    private float cameraDistance;
    private Vector3 grabOffset;

    private void Awake()
    {
        cubeRenderer = GetComponent<Renderer>();
        arCamera = Camera.main;

        originalScale = transform.localScale;

        // Apply the first color right away so colorIndex 0 is visible from the start.
        ApplyColor(colorIndex);
    }

    private void OnEnable()  => EnhancedTouchSupport.Enable();
    private void OnDisable() => EnhancedTouchSupport.Disable();

    private void Update()
    {
        if (isRotating)
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);

        // Pinch (2 fingers) takes priority over single-finger drag.
        if (Touch.activeTouches.Count >= 2)
        {
            isDragging = false; // cancel any one-finger drag in progress
            HandlePinchScale();
        }
        else
        {
            isPinching = false;
            HandleDrag();
        }
    }

    // ---------- Public methods wired to the UI buttons ----------

    public void ToggleRotation() => isRotating = !isRotating;

    public void NextColor()
    {
        if (colors.Length == 0) return;

        colorIndex = (colorIndex + 1) % colors.Length; // loops back to the first color
        ApplyColor(colorIndex);
    }

    public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Internal helpers ----------

    private void ApplyColor(int index)
    {
        if (cubeRenderer.material.HasProperty("_BaseColor"))
            cubeRenderer.material.SetColor("_BaseColor", colors[index]);
        else
            cubeRenderer.material.color = colors[index];
    }

    // ---------- Two-finger pinch scale ----------

    private void HandlePinchScale()
    {
        Touch fingerA = Touch.activeTouches[0];
        Touch fingerB = Touch.activeTouches[1];

        float currentPinchDistance = Vector2.Distance(
            fingerA.screenPosition, fingerB.screenPosition);

        if (!isPinching)
        {
            // first frame of the pinch: just record the baseline, don't scale yet
            isPinching = true;
            previousPinchDistance = currentPinchDistance;
            return;
        }

        float pinchDelta = currentPinchDistance - previousPinchDistance;
        currentScaleFactor += pinchDelta * pinchSensitivity;
        currentScaleFactor = Mathf.Clamp(currentScaleFactor, 1f, maxScaleMultiplier);

        transform.localScale = originalScale * currentScaleFactor;

        previousPinchDistance = currentPinchDistance;
    }

    // ---------- One-finger drag ----------

    private void HandleDrag()
    {
        if (arCamera == null) return;

        if (Touch.activeTouches.Count == 0)
        {
            isDragging = false;
            return;
        }

        Touch finger = Touch.activeTouches[0];   // first finger only

        switch (finger.phase)
        {
            case TouchPhase.Began:
                if (IsOverUI(finger)) return;

                Ray ray = arCamera.ScreenPointToRay(finger.screenPosition);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
                {
                    isDragging = true;
                    cameraDistance = Vector3.Distance(
                        arCamera.transform.position, transform.position);
                    grabOffset = transform.position - ScreenToWorld(finger.screenPosition);
                }
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isDragging)
                    transform.position = ScreenToWorld(finger.screenPosition) + grabOffset;
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isDragging = false;
                break;
        }
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        return arCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, cameraDistance));
    }

    private bool IsOverUI(Touch finger)
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject(finger.finger.index);
    }
}