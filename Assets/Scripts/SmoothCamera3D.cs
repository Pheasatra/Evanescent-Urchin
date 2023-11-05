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

    float currentSensitivityX = 0;
    float currentSensitivityY = 0;

    [Space(10)]

    public float minimumY = -60f;
    public float maximumY = 60f;

    float nextRotationX = 0f;
    float nextRotationY = 0f;

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
    public float totalShake;
    public float totalIntensity;

    [Space(10)]

    private List<Shake> shakes = new List<Shake>();

    [Space(10)]

    private Vector3 desiredPosition;
    private Quaternion currentRotation;

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Set our initial rotations so we can rotate things in the inspector
        nextRotationX = transform.localEulerAngles.x;
        nextRotationY = transform.localEulerAngles.y;
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

    /// <summary> Based on key input, Zooms the camera by changing FOV and sensitivity </summary>
    private void ManageFarZoom()
    {
        switch (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Set FOV to far 
            case true:
                // Halving the FOV doubles the magnification
                currentFov = baseFov / magnification;

                currentSensitivityX = sensitivityX / magnification;
                currentSensitivityY = sensitivityY / magnification;
                break;

            // Set FOV to normal 
            case false:
                currentFov = baseFov;

                currentSensitivityX = sensitivityX;
                currentSensitivityY = sensitivityY;
                break;
        }

        // Make the zoom berry smooth
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, currentFov, zoomStiffness * Time.deltaTime);
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Based on mousepad inputs, rotates the camera smoothly </summary>
    private void ManageRotation()
    {
        // Get mouse inputs multiplied by sensitivity
        nextRotationX += Input.GetAxis("Mouse X") * currentSensitivityX;
        nextRotationY += Input.GetAxis("Mouse Y") * currentSensitivityY;

        // Clamp Y so we don't go australian in this bitch
        nextRotationY = Mathf.Clamp(nextRotationY, minimumY, maximumY);

        // Produces in-between rotations for when there is no input this frame (Low keyboard update rate) and when input fully stops (Smoothes camera at start and destination)
        // We can also use localEulerAngles here, but Quanterions reduce the risk of gimbal lock
        transform.rotation = Quaternion.Lerp(Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0), Quaternion.Euler(-nextRotationY, nextRotationX, 0), rotationStiffness * Time.deltaTime);

        // Less useful / functional alternatives down here
        //transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-nextRotationY, nextRotationX, 0), rotationStiffness * Time.deltaTime);
    }

    // -----------------------------------------------------------------------------------------------------

    /// <summary> Sums all of the shakes added to the camera, Removes completed ones and adds the final summed shake for this frame </summary>
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

    /// <summary> Adds a shake to the camera of a specific length and intensity </summary>
    public void CameraShake(float duration, float intensity)
    {
        shakes.Add(new Shake(duration * Manager.manager.screenShakeMultiplyer, intensity * Manager.manager.screenShakeMultiplyer));
    }
}

// -----------------------------------------------------------------------------------------------------

/// <summary> A shake, this holds the remaining duration and the intensity of a shake </summary>
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