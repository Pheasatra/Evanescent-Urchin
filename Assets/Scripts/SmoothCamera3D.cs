using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class SmoothCamera3D : MonoBehaviour
{
    [Header("References")]
    public Transform target;

    [Header("Positioning")]
    [Tooltip("Adds a bias, prevents the camera from violating it's restraining order on cameraTarget")]
    public Vector3 baseOffset = new Vector3(0, 0, -10);
    private Vector3 shakeOffset;

    [Header("Scroll")]
    [Tooltip("Mutiply the current size by this")]
    public float scrollMultiplyer = 1.0f;

    [Space(10)]

    public float minCameraScroll = 10;
    public float maxCameraScroll = 1000;

    [Header("Rotation")]
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    [Space(10)]

    public float minimumX = -360F;
    public float maximumX = 360F;

    [Space(10)]

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationY = 0F;

    [Header("Movement")]
    [Tooltip("The lower the variable smoothness is the higher the actual smoothness is")]
    public float movementSmoothness = 10.0f;
    public float scrollSmoothness = 10.0f;

    [Header("Screen Shake")]
    public List<Shake> shakes = new List<Shake>();

    [Space(10)]

    public float totalShake;
    public float totalIntensity;

    [Space(10)]

    private float scrollingIncrement;
    private float newOffset;
    private Vector3 desiredPosition;

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Set newOffset to our baseOffset.z so that it has the inital value
        newOffset = baseOffset.z;
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
        ManageScroll();
        ManageRotation();

        // Scroll Lerp
        baseOffset.z = Mathf.Lerp(baseOffset.z, newOffset, scrollSmoothness * Time.deltaTime);

        desiredPosition = target.position + shakeOffset + baseOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, movementSmoothness * Time.deltaTime);
    }

    // -----------------------------------------------------------------------------------------------------

    public void ManageScroll()
    {
        // Ensures that the camera scroll scales with the size of the camera.
        scrollingIncrement = Mathf.Sqrt(Mathf.Abs(baseOffset.z)) * scrollMultiplyer;

        switch (Input.GetAxis("Mouse ScrollWheel") > 0 && baseOffset.z > minCameraScroll)
        {
            // Zoom in
            case true:
                newOffset = baseOffset.z + scrollingIncrement;
                break;
        }

        // Get mouse scrollwheel backwards 
        switch (Input.GetAxis("Mouse ScrollWheel") < 0 && baseOffset.z < maxCameraScroll)
        {
            case true:
                newOffset = baseOffset.z - scrollingIncrement;
                break;
        }

        // When a computer does not have a mouse.
        switch (Input.GetKey(KeyCode.Minus) && baseOffset.z > minCameraScroll)
        {
            case true:
                // Scroll camera inwards
                newOffset = baseOffset.z - scrollingIncrement / 3;
                break;
        }

        switch (Input.GetKey(KeyCode.Equals) && baseOffset.z < maxCameraScroll)
        {
            case true:
                // Scrolling Backwards
                newOffset = baseOffset.z + scrollingIncrement / 3;
                break;
        }

        // Clamp the new offset between our limit
        newOffset = Mathf.Clamp(newOffset, minCameraScroll, maxCameraScroll);
    }    
    
    // -----------------------------------------------------------------------------------------------------

    public void ManageRotation()
    {
        float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
    }

    // -----------------------------------------------------------------------------------------------------

    public void ManageShakes()
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