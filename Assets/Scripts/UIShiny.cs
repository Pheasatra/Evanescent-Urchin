using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------

public class UIShiny : MonoBehaviour
{
    public Sprite originalSprite;
    public Sprite activeSprite;

    [Space(10)]

    public bool isImage = false;

    public Image image;
    public SpriteRenderer rend;

    [Space(10)]

    public float shadingPower = 0.5f;
    public float variation = 0.05f;

    [Space(10)]

    public Color baseColour = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    [Space(10)]

    public float xOffset;
    public float yOffset;

    // Start is called before the first frame update
    void Start()
    {
        // Reset
        activeSprite.texture.SetPixels(originalSprite.texture.GetPixels());

        xOffset = Random.Range(-99999, 99999);
        yOffset = Random.Range(-99999, 99999);

        PixelShader();
    }

    //-----------------------------------------------------------------------

    public void PixelShader()
    {
        // Iterate through it's pixels
        for (int x = 0; x < activeSprite.texture.width; x++)
        {
            for (int y = 0; y < activeSprite.texture.height; y++)
            {
                Color currentColour = originalSprite.texture.GetPixel(x, y);

                switch (currentColour.a > 0.05f)
                {
                    // If the alpha is larger than 0.05
                    case true:
                        // Calculate the sine shading on each axis
                        float shading = Mathf.Sin(x + xOffset) * shadingPower;
                        shading += Mathf.Sin(y + yOffset) * shadingPower;

                        // Calculate the varience value
                        float varience = Random.Range(-variation, variation);

                        activeSprite.texture.SetPixel(x, y, baseColour + new Color(shading, shading, shading, 0) + new Color(varience, varience, varience, 0));
                        break;

                }
            }
        }

        switch (isImage)
        {
            case true:
                image.sprite = activeSprite;
                break;

            case false:
                rend.sprite = activeSprite;
                break;
        }

        activeSprite.texture.Apply();
    }
}