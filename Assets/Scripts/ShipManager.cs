using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class ShipManager : MonoBehaviour
{
    public static ShipManager shipManager { get; private set; }

    public float maxBuildDistance = 100;


    // -----------------------------------------------------------------------------------------------------

    void Awake()
    {
        // Set our singleton reference, we do this here for good reasons
        shipManager = this;
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
        CheckForBuild();
        CheckForDestroy();
    }

    // -----------------------------------------------------------------------------------------------------

    public void CheckForBuild()
    {
        switch (Input.GetMouseButtonDown(0))
        {
            // No left mouse button
            case false:
                return;
        }

        switch (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, maxBuildDistance))
        {
            // No hit
            case false:
                return;
        }

        // Build a block here
        Vector3 position = hit.normal + hit.transform.position;

        //Instantiate(blocks[selected], new Vector3((int)position.x, (int)position.y, (int)position.z), Quaternion.Euler(0, 0, 0)).transform.SetParent(ship.transform);
    }

    // -----------------------------------------------------------------------------------------------------

    public void CheckForDestroy()
    {
        switch (Input.GetMouseButtonDown(2))
        {
            // No right mouse button
            case false:
                return;
        }

        switch (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, maxBuildDistance))
        {
            // No hit
            case false:
                return;
        }

        // Destroy a block here
        //Destroy(hit.transform.gameObject);
    }
}