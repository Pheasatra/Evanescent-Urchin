using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroScreen : MonoBehaviour
{
    public Vector2 tiling;

    [Space(10)]
    /*
    public float parallaxAmount = 8.0f;
    public float scrollParallax = 1.0f;
    */
    [Tooltip("Controls the amount of edging past the camera view, for clean parallax")]
    public float extraEdgeSize = 1.1f;

    public RectTransform rectTransform;
    public Image image;
    
    private Vector2 materialOffset;
    private Vector2 materialScale;
    private Vector2 seedOffset;
    private Vector2 cameraSize;

    private float screenSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Randomise an offset so the stars will be different each startup
        seedOffset = new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

        //scrollParallax = scrollParallax + Camera.main.orthographicSize;

        materialOffset = image.material.mainTextureOffset;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Find the largest size, so we will always fit the screen correctly, also add some extra juice to the edges
        screenSize = Mathf.Max(Screen.width, Screen.height);
        rectTransform.sizeDelta = new Vector2(screenSize, screenSize) * extraEdgeSize;
        /*
        materialOffset = Camera.main.transform.position / parallaxAmount;

        // Set the scale of the texturew
        materialScale = new Vector2(Camera.main.orthographicSize / scrollParallax, Camera.main.orthographicSize / scrollParallax);
        //mr.material.mainTextureScale = materialScale;
        */
        // Set the texture offset using our current offset plus the seed
        image.material.mainTextureScale = tiling;/*
        image.material.mainTextureOffset = materialOffset + seedOffset - (materialScale / 2);
        */
        //spriteRenderer.material.SetVector("_TextureOffset", materialOffset + seedOffset - (materialScale / 2));
        //spriteRenderer.material.SetVector("_TextureScale", materialScale);
    }
}
