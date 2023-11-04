using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// -----------------------------------------------------------------------------------------------------

public class Scroller : MonoBehaviour
{
    [Header("Positioning")]
    [Tooltip("Adds a bias, prevents the camera from violating it's restraining order on cameraTarget")]
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Scroll")]
    [Tooltip("Mutiply the scroll increment by this")]
    public float scrollMultiplyer = 1.0f;

    [Tooltip("How stiff the scrolling will be")]
    public float scrollStiffness = 10.0f;

    [Space(10)]

    public float minCameraScroll = 10;
    public float maxCameraScroll = 1000;

    private float scrollingIncrement;
    private float newOffset;

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Set newOffset to our baseOffset.z so that it has the inital value
        newOffset = offset.z;
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void LateUpdate()
    {
        ManageScroll();

        // Scroll Lerp, we double lerp this so the camera does not jerk back each middle mouse roll
        offset.z = Mathf.Lerp(offset.z, newOffset, scrollStiffness * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, offset, scrollStiffness * Time.deltaTime);
    }

    // -----------------------------------------------------------------------------------------------------

    public void ManageScroll()
    {
        // Ensures that the camera scroll scales with the size of the camera.
        scrollingIncrement = Mathf.Sqrt(Mathf.Abs(offset.z)) * scrollMultiplyer;

        switch (Input.GetAxis("Mouse ScrollWheel") > 0 && offset.z < minCameraScroll)
        {
            // Zoom in
            case true:
                newOffset = offset.z + scrollingIncrement;
                break;
        }

        // Get mouse scrollwheel backwards 
        switch (Input.GetAxis("Mouse ScrollWheel") < 0 && offset.z >= maxCameraScroll)
        {
            case true:
                newOffset = offset.z - scrollingIncrement;
                break;
        }
        
        // When a computer does not have a mouse.
        switch (Input.GetKey(KeyCode.Minus) && offset.z < minCameraScroll)
        {
            case true:
                // Scroll camera inwards
                newOffset = offset.z + scrollingIncrement * Time.deltaTime * 100;
                break;
        }

        switch (Input.GetKey(KeyCode.Equals) && offset.z >= maxCameraScroll)
        {
            case true:
                // Scrolling Backwards
                newOffset = offset.z - scrollingIncrement * Time.deltaTime * 100;
                break;
        }
        
        // Clamp the new offset between our limit
        newOffset = Mathf.Clamp(newOffset, maxCameraScroll, minCameraScroll);
    }
}