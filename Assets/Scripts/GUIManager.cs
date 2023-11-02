using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// -----------------------------------------------------------------------------------------------------

public class GUIManager : MonoBehaviour
{
    // Only one of these so we can make it a singleton, also make sure we can't set this
    public static GUIManager guiManager { get; private set; }
    
    [Space(10)]
    
    [Header("Tutorial Logic")]
    public bool firstTimeStart = true;
    public bool moved = false;
    public bool sprayed = false;
    public bool nearElevator = false;

    [Space(10)]

    [Header("Tutorial")]
    public TextMeshProUGUI sprayThis;
    public TextMeshProUGUI sprayThisLowerText;
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI onwards;
    public GameObject keys;
    public GameObject keysElevator;

    [Header("UI Controllers")]
    public FadeInOut tutorialGroup;
    public FadeInOut introGroup;
    public FadeInOut pauseGroup;
    public FadeInOut settingsGroup;
    public FadeInOut endGroup;

    [Space(10)]

    public bool gameStarted = false;

    public float timeSinceGameStart = 0.0f;
    public float glhfTimer = 0;
    public float temperatureTimer = 0.0f;

    // -----------------------------------------------------------------------------------------------------

    void Awake()
    {
        // Set our singleton reference, we do this here for good reasons
        guiManager = this;
    }

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Fade out elements by default
        sprayThis.CrossFadeAlpha(0, 0, true);
        sprayThisLowerText.CrossFadeAlpha(0, 0, true);
        temperatureText.CrossFadeAlpha(0, 0, true);
        onwards.CrossFadeAlpha(0, 0, true);
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        switch (LevelManager.levelManager.simulationMode)
        {
            // Skip this shit if editing
            case SimulationMode.EditingMode:
                firstTimeStart = false;

                keys.SetActive(false);
                keysElevator.SetActive(false);

                sprayThis.CrossFadeAlpha(0, 0, true);
                sprayThisLowerText.CrossFadeAlpha(0, 0, true);
                temperatureText.CrossFadeAlpha(0, 0, true);
                onwards.CrossFadeAlpha(0, 0, true);
                break;
        }

        switch (gameStarted)
        {
            // Only do these check if the game has not started yet
            case false:
                switch (Input.anyKeyDown)
                {
                    // If any input is detected start the game
                    case true:
                        SceneManager.sceneManager.ResumeGame(); 
                        IntroFadeOut();
                        gameStarted = true;
                        break;
                }
                break;
            
            case true:
                timeSinceGameStart += Time.deltaTime;
                break;
        }
        
        switch (introGroup.currentAlpha > 0.05f)
        {
            case true:
                introGroup.canvasGroup.gameObject.SetActive(true);
                break;

            case false:
                introGroup.canvasGroup.gameObject.SetActive(false);
                break;
        }
        
        switch (pauseGroup.currentAlpha > 0.05f)
        {
            case true:
                pauseGroup.canvasGroup.gameObject.SetActive(true);
                break;

            case false:
                pauseGroup.canvasGroup.gameObject.SetActive(false);
                break;
        }

        switch (endGroup.currentAlpha > 0.05f)
        {
            case true:
                endGroup.canvasGroup.gameObject.SetActive(true);
                break;

            case false:
                endGroup.canvasGroup.gameObject.SetActive(false);
                break;
        }

        switch (settingsGroup.currentAlpha > 0.05f)
        {
            case true:
                settingsGroup.canvasGroup.gameObject.SetActive(true);
                break;

            case false:
                settingsGroup.canvasGroup.gameObject.SetActive(false);
                break;
        }

        switch (firstTimeStart && Time.timeSinceLevelLoad > 1.0f)
        {
            // If this the not first time we started and time is smaller than 1.0
            case false:
                tutorialGroup.gameObject.SetActive(false);
                return;
        }

        switch (SceneManager.sceneManager.gamePaused)
        {
            // Game paused
            case true:
                sprayThis.gameObject.SetActive(false);
                return;
        }

        switch (moved)
        {
            // Disable movement tutorial
            case true:
                keys.SetActive(false);
                break;
        }

        switch (nearElevator)
        {
            // Show elevator controls
            case true:
                keysElevator.SetActive(true);
                break;

            // Hide elevator controls
            case false:
                keysElevator.SetActive(false);
                break;
        }

        switch (sprayed)
        {
            case true:
                sprayThis.CrossFadeAlpha(0, 0.25f, true);
                sprayThisLowerText.CrossFadeAlpha(0.0f, 0.25f, true);
                break;

            // Has not spayed yet
            case false:
                sprayThis.CrossFadeAlpha(1, 0.25f, true);
                sprayThisLowerText.CrossFadeAlpha(1.0f, 0.25f, true);
                return;
        }

        switch (temperatureTimer > 2.5f)
        {
            case true:
                temperatureText.CrossFadeAlpha(0, 0.25f, true);
                break;

            // Keep counting
            case false:
                temperatureText.CrossFadeAlpha(1, 0.25f, true);
                temperatureTimer += Time.deltaTime;
                return;
        }

        // Terminate tutorial
        onwards.CrossFadeAlpha(1, 0.25f, true);

        glhfTimer += Time.deltaTime;

        switch (glhfTimer > 2.5f)
        {
            case true:
                onwards.CrossFadeAlpha(0, 0.25f, true);
                firstTimeStart = false;
                break;
        }
    }

    // -----------------------------------------------------------------------------------------------------

    public void IntroFadeOut()
    {
        introGroup.FadeTo(0.0f, 4.0f);
    }
}