using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class SmoothCamera3D : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform target;

    [Header("Positioning")]
    private Vector3 shakeOffset;

    [Header("Rotation")]
    public float sensitivityX = 15f;
    public float sensitivityY = 15f;

    [Space(10)]

    public float minimumX = -360f;
    public float maximumX = 360f;

    [Space(10)]

    public float minimumY = -60f;
    public float maximumY = 60f;

    float rotationY = 0f;

    [Header("Zoom")]
    public float baseFov = 60.0f;
    public float magnification = 2.0f;
    private float currentFov;

    [Header("Movement Smoothness")]
    [Tooltip("How stiff our movement is")]
    public float movementStiffness = 10.0f;

    [Tooltip("How stiff our fov zooming is")]
    public float zoomStiffness = 10.0f;

    [Tooltip("How stiff our fov zooming is")]
    public float rotationStiffness = 10.0f;

    [Header("Screen Shake")]
    public List<Shake> shakes = new List<Shake>();

    [Space(10)]

    public float totalShake;
    public float totalIntensity;

    [Space(10)]

    private Vector3 desiredPosition;
    private Quaternion currentRotation;

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        switch (SceneManager.sceneManager.gamePaused)
        {
            // Unlock cursor when in menus
            case true:
                Cursor.lockState = CursorLockMode.None;
                break;

            // Lock cursor when in game view
            case false:
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }

        ManageShakes();
        ManageRotation();
        ManageFarZoom();

        desiredPosition = target.position + shakeOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, movementStiffness * Time.deltaTime);
    }

    // -----------------------------------------------------------------------------------------------------

    private void ManageFarZoom()
    {
        switch (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Set FOV to far 
            case true:
                // Halving the FOV doubles the magnification
                currentFov = baseFov / magnification;
                break;

            // Set FOV to normal 
            case false:
                currentFov = baseFov;
                break;
        }

        // Make the zoom berry smooth
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, currentFov, zoomStiffness * Time.deltaTime);
    }

    // -----------------------------------------------------------------------------------------------------

    private void ManageRotation()
    {
        float rotationX = transform.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

        //transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-rotationY, rotationX, 0), rotationStiffness * Time.deltaTime);
    }

    // -----------------------------------------------------------------------------------------------------

    private void ManageShakes()
    {
        totalShake = 0.0f;
        totalIntensity = 0.0f;

        // For all shakes
        for (int x = 0; x < shakes.Count; x++)
        {
            switch (shakes[x].duration > 0.0f)
            {
                // If needing to shake
                case true:
                    shakes[x].duration -= Time.deltaTime;
                    totalShake += shakes[x].duration;
                    totalIntensity += shakes[x].intensity;
                    break;

                // If not needing to shake
                case false:
                    shakes.Remove(shakes[x]);
                    x--; // We are removing something from a list so we need to roll back
                    break;
            }
        }

        // Get a random direction in a circle, then increase it by the shake amount
        shakeOffset = totalIntensity * totalShake * Manager.manager.screenShakeMultiplyer * Random.insideUnitSphere;
    }

    // -----------------------------------------------------------------------------------------------------

    public void CameraShake(float duration, float intensity)
    {
        shakes.Add(new Shake(duration * Manager.manager.screenShakeMultiplyer, intensity * Manager.manager.screenShakeMultiplyer));
    }
}

// -----------------------------------------------------------------------------------------------------

public class Shake
{
    public float duration;
    public float intensity;

    public Shake(float duration, float intensity)
    {
        this.duration = duration;
        this.intensity = intensity;
    }
}