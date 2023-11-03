using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class Manager : MonoBehaviour
{
    // Only one of these so we can make it a singleton, also make sure we can't set this
    public static Manager manager { get; private set; }

    [Header("Global control variables")]
    public float screenShakeMultiplyer = 2.0f;
    public float gameEndDistance = 30.0f;

    public Transform endPosition;

    //[Header("Important variables")]
    //public SmoothCamera2D smoothCamera;

    //[Header("References")]

    // -----------------------------------------------------------------------------------------------------

    void Awake()
    {
        // Set our singleton reference, we do this here for good reasons
        manager = this;
    }

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {

    }
}