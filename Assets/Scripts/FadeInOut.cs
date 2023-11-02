using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// -----------------------------------------------------------------------------------------------------

[System.Serializable]
public class FadeInOut : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    [Space(10)]

    public float desiredAlpha = 0.0f;
    public float currentAlpha = 0.0f;
    public float timeMultiplyer = 1.0f;

    // -----------------------------------------------------------------------------------------------------

    private void Start()
    {
        desiredAlpha = canvasGroup.alpha;
        currentAlpha = canvasGroup.alpha;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentAlpha == desiredAlpha)
        {
            // If these values are not exactly the same
            case false:
                currentAlpha = Mathf.MoveTowards(currentAlpha, desiredAlpha, timeMultiplyer * Time.unscaledDeltaTime);

                SetValues();
                break;
        }
    }

    // -----------------------------------------------------------------------------------------------------

    public void FadeTo(float desiredAlpha, float speed)
    {
        this.desiredAlpha = desiredAlpha;
        timeMultiplyer = speed;
    }

    // -----------------------------------------------------------------------------------------------------

    public void SetValues()
    {
        canvasGroup.alpha = currentAlpha;
    }
}