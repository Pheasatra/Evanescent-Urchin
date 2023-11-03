using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class PlayerController : MonoBehaviour
{
    // Only one of these so we can make it a singleton, also make sure we can't set this
    public static PlayerController playerController { get; private set; }

    [Header("References")]
    public GameObject playerObject;
    public Rigidbody playerRigidbody;
    
    [Header("Movement Variables")]
    public float engineThrust = 400;
    public float rollTorque = 400;

    [Space(10)]

    public float airViscosity = 0.1f;
    public float waterViscosity = 1.0f;
    public float landViscosity = 50.0f;

    [Space(10)]

    public float airDensity = 0.1f;
    public float waterDensity = 1.0f;
    public float landDensity = 10.0f;

    // -----------------------------------------------------------------------------------------------------

    void Awake()
    {
        // Set our singleton reference, we do this here for good reasons
        playerController = this;
    }

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    // -----------------------------------------------------------------------------------------------------

    public void Movement()
    {
        float currentThrust = engineThrust * Time.deltaTime;
        //playerRigidbody.drag = averageViscosity;

        // --- Forward Back ---

        switch (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // Forward movement
            case true:
                playerRigidbody.AddForce(currentThrust * Vector3.forward, ForceMode.Force);
                break;
        }

        switch (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // Backwards movement
            case true:
                playerRigidbody.AddForce(currentThrust * Vector3.back, ForceMode.Force);
                break;
        }

        // --- Left Right ---

        switch (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            // Left strafe
            case true:
                playerRigidbody.AddForce(currentThrust * Vector3.left, ForceMode.Force);
                break;
        }

        switch (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            // Right strafe
            case true:
                playerRigidbody.AddForce(currentThrust * Vector3.right, ForceMode.Force);
                break;
        }

        // --- Roll Left Right ---

        switch (Input.GetKeyDown(KeyCode.Q))
        {
            // Roll Left
            case true:
                playerRigidbody.AddTorque(rollTorque * Vector3.left, ForceMode.Force);
                break;
        }

        switch (Input.GetKeyDown(KeyCode.E))
        {
            // Roll Right
            case true:
                playerRigidbody.AddForce(rollTorque * Vector3.right, ForceMode.Force);
                break;
        }
    }

    // -----------------------------------------------------------------------------------------------------

    public void Die()
    {
        //AudioManager.audioManager.PlayOneShot("GameOver");

        playerObject.SetActive(false);

        //GUIManager.guiManager.endGroup.FadeTo(0.0f, 4.0f);
        //Manager.manager.BubbleExplosion(transform.position, 200, 50, 1);


        //Manager.manager.smoothCamera.CameraShake(2.0f, 10);

        // Respawn after 2 seconds
        Invoke(nameof(Respawn), 3.0f);
    }

    // -----------------------------------------------------------------------------------------------------

    public void Respawn()
    {
        //AudioManager.audioManager.PlayOneShot("Respawn");

        playerObject.SetActive(true);

        //Manager.manager.BubbleExplosion(playerObject.transform.position, 50, 25, 1);
    }
}