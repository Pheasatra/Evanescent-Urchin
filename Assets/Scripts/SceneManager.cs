using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------------------------------------------------------------------------------

public class SceneManager : MonoBehaviour
{
    // Only one of these so we can make it a singleton, also make sure we can't set this
    public static SceneManager sceneManager { get; private set; }

    [Header("References")]
    public Texture2D cursor;

    [Header("Button events")]
    public ButtonAllClick resumeButton;
    public ButtonAllClick settingsButton;
    public ButtonAllClick closeSettingsButton;

    [Space(10)]

    public bool endSoundDone = false;
    public bool gamePaused = false;
    public bool settingsOpen = false;
    public float currentTimescale = 0;
    public float timeSincePause = 0;

    public bool updatePauseNext = false;
    private float pauseTimeTarget = 1.0f;

    // -----------------------------------------------------------------------------------------------------

    void Awake()
    {
        // Set our singleton reference, we do this here for good reasons
        sceneManager = this;
    }

    // -----------------------------------------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Set the cursor origin to its centre. (default is upper left corner)
        Vector2 cursorOffset = new Vector2(cursor.width / 2, cursor.height / 2);
        Cursor.SetCursor(cursor, cursorOffset, CursorMode.Auto);
    }

    // -----------------------------------------------------------------------------------------------------

    // Update is called once per frame
    void Update()
    {
        //cursorObject.transform.position = LevelManager.levelManager.pixelMousePosition + LevelManager.levelManager.pixelOffset * 2;

        timeSincePause += Time.unscaledDeltaTime;

        switch (Input.GetKeyDown(KeyCode.Escape)/* && manager.playerAlive == true*/)
        {
            case true:
                timeSincePause = 0;
                updatePauseNext = true;

                switch (gamePaused)
                {
                    case true:
                        gamePaused = false;
                        break;

                    case false:
                        gamePaused = true;
                        break;
                }
                break;
        }

        switch (updatePauseNext)
        {
            // If the we have decided to update the pause state
            case true:
                switch (gamePaused)
                {
                    case true:
                        PauseGame(true);
                        break;

                    case false:
                        ResumeGame();
                        break;
                }

                updatePauseNext = false;
                break;
        }
        /*
        switch (Vector2.Distance(PlayerController.playerController.playerObject.transform.position, Manager.manager.endPosition.transform.position) < Manager.manager.gameEndDistance)
        {
            case true:
                ToggleEnd();
                break;
        }*/

        // Use time dilation magic for a smooth time change
        currentTimescale = Mathf.Lerp(currentTimescale, pauseTimeTarget, timeSincePause / 4);
        Time.timeScale = currentTimescale;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    // -----------------------------------------------------------------------------------------------------
    
    public void RestartGame()
    {/*
        ResumeGame();

        // Make us respawn at the beginning
        PlayerController.playerController.lastCheckpoint = null;

        GUIManager.guiManager.endGroup.FadeTo(0.0f, 4.0f);
        Manager.manager.smoothCamera.CameraShake(1.0f, 10);

        PlayerController.playerController.Respawn();*/
    }
    
    // -----------------------------------------------------------------------------------------------------

    public void PauseGame(bool showScreen)
    {
        // Fade in
        switch (showScreen)
        {
            case true:
                GUIManager.guiManager.pauseGroup.FadeTo(1.0f, 4.0f);
                break;
        }

        pauseTimeTarget = 0.0f;
    }

    // -----------------------------------------------------------------------------------------------------

    public void ResumeGame()
    {
        // Fade out
        GUIManager.guiManager.pauseGroup.FadeTo(0.0f, 4.0f);
        pauseTimeTarget = 1.0f;

        // Also close the setting menu if that was open
        CloseSettings();
    }

    // -----------------------------------------------------------------------------------------------------

    public void OpenSettings()
    {
        switch (gamePaused)
        {
            // Settings will only open if the game is paused
            case true:
                GUIManager.guiManager.settingsGroup.FadeTo(1.0f, 4.0f);
                settingsOpen = true;
                break;
        }
    }

    // -----------------------------------------------------------------------------------------------------

    public void CloseSettings()
    {
        // Settings will always close when asked
        GUIManager.guiManager.settingsGroup.FadeTo(0.0f, 4.0f);
        settingsOpen = false;
    }

    // -----------------------------------------------------------------------------------------------------

    // So we can hook buttons up easily
    public void PlayUISelect()
    {
        //AudioManager.audioManager.PlayOneShot("Select");
    }

    // -----------------------------------------------------------------------------------------------------

    public void ToggleEnd()
    {
        PauseGame(false);

        // Fade in
        GUIManager.guiManager.endGroup.FadeTo(1.0f, 4.0f);

        switch (endSoundDone)
        {
            // Play the sound once
            case false:
                //AudioManager.audioManager.PlayOneShot("GameWin");
                endSoundDone = true;
                break;
        }
    }

    // -----------------------------------------------------------------------------------------------------

    public void ExitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}