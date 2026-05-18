using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using CS.AudioToolkit;
using TMPro;
public class Player_UIManager : MonoBehaviour
{
    public static Player_UIManager Instance { get; private set; }
    
    [SerializeField] private GameObject graphics;
    [SerializeField] private GameObject probes;
    [SerializeField] private Toggle graphicsToggle;

    [Header("Player UI")]
    [SerializeField] private GameObject playerUIPanel;
    [SerializeField] private TMP_Text objText;
    [SerializeField] private TMP_Text interactTxt;
    [SerializeField] private TMP_Text dialogText;
    [SerializeField] private Slider sanityBar;
    [SerializeField] private TMP_Text sanityText;

    [Header("Pause UI")]
    [SerializeField] private GameObject pauseGameOverUIPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [Header("Game Over UI")]
    [SerializeField] private TMP_Text pauseGameOverText;
    [SerializeField] private Button respawnButton;

    [Header("Settings UI")]
    [SerializeField] private GameObject settingsUIPanel;
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Button windowedButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Misc. Buttons")]
    [SerializeField] private Button backButton;

    [Header("Fade in to Game scene setup")]
    [SerializeField] private CanvasGroup mainUI;
    [SerializeField] private CanvasGroup fadeImg;
    
    private bool disableGraphics;


    private List<Button> buttonsArr = new List<Button>();

    private FullScreenMode desiredScreenMode = FullScreenMode.FullScreenWindow;

    void Start() {
        //singleton
        Instance = this;

        //Play Audio
        AudioController.Play("HoloJam5T5FDBGMLoop");
        ShowInteractText(false);
        ApplyScreen();

        fadeImg.alpha = 0;

        // Button Callbacks - Pause UI

        //resumeButton.onClick.AddListener(() => PauseManager.Instance.PauseResumeGame());
        settingsButton.onClick.AddListener(() => ToggleMenu(settingsUIPanel.name, true, false));
        mainMenuButton.onClick.AddListener(() => {
            Time.timeScale = 1;
            mainUI.alpha = 0;

            foreach (Button b in buttonsArr) {
                b.gameObject.SetActive(false);
            }

            LeanTween.alphaCanvas(fadeImg, 1, 2f).setOnComplete(() => {
                Debug.Log("Fade complete, loading scene..."); // Debug log to confirm animation completion
                SceneManager.LoadScene(0);
            });
        });
        quitButton.onClick.AddListener(Application.Quit);

        // Button Callbacks - Settings
        fullscreenButton.onClick.AddListener(() => { desiredScreenMode = FullScreenMode.FullScreenWindow; });
        windowedButton.onClick.AddListener(() => { desiredScreenMode = FullScreenMode.Windowed; });
        applyButton.onClick.AddListener(ApplyScreen);

        // Back button
        backButton.onClick.AddListener(() => ToggleMenu(pauseGameOverUIPanel.name, false, true));

        // Initialize sliders
        musicVolumeSlider.value = AudioController.GetCategoryVolume("BGM");
        sfxVolumeSlider.value = AudioController.GetCategoryVolume("SFX");

        if (PlayerPrefs_Sounds.Instance != null) {
            musicVolumeSlider.onValueChanged.AddListener(PlayerPrefs_Sounds.Instance.SetBGMVol);
            sfxVolumeSlider.onValueChanged.AddListener(PlayerPrefs_Sounds.Instance.SetSFXVol);
        }

        //add all buttons to list
        buttonsArr.AddRange(new Button[] {
            resumeButton, settingsButton, mainMenuButton, quitButton,
            fullscreenButton, windowedButton, applyButton, backButton
        });

        // Add hover sound and click sound to each button in buttonsArr
        foreach (Button button in buttonsArr) {
            AddHoverSound(button);
            button.onClick.AddListener(AddConfirmSound);
        }
        respawnButton.onClick.AddListener(RespawnPlayer);
        respawnButton.gameObject.SetActive(false);
        pauseGameOverText.text = "Pause";
        
        graphicsToggle.onValueChanged.AddListener(ToggleGraphics);
        this.disableGraphics = MainMenu_UIManager.disableGrahics;
        graphicsToggle.isOn = disableGraphics;
        ToggleGraphics(disableGraphics);
    }

    // TODO: REMOVE THIS WHEN FINISHED, JUST USED TO TEST KILLING THE PLAYER
    public void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            ShowEndGameScreen();
        }
    }

    public void ShowInteractText(bool show)
    {
        interactTxt.gameObject.SetActive(show);
    }

    private void ToggleMenu(string panelName, bool showBackButton, bool canUnpause)
    {
        // Deactivate all panels
        pauseGameOverUIPanel.SetActive(panelName.Equals(pauseGameOverUIPanel.name));
        settingsUIPanel.SetActive(panelName.Equals(settingsUIPanel.name));

        //PauseManager.Instance.SetCanUnpause(canUnpause);

        backButton.gameObject.SetActive(showBackButton);
    }

    private void ApplyScreen()
    {
        Screen.fullScreenMode = desiredScreenMode;
    }

    #region sanity bar UI
    public void SetMaxSanity(float maxSan)
    {
        sanityBar.maxValue = maxSan;
        sanityBar.value = maxSan;
        sanityText.text = $"Sanity: ({maxSan}/{maxSan})";
    }
    public void SetCurrSanity(float sanity)
    {
        sanityBar.value = sanity;
        sanityText.text = $"Sanity: ({sanity}/{sanityBar.maxValue})";
        if (sanity <= 0) {
            // Game over
            KillPlayer();
        }
    }
    #endregion
    
    public void KillPlayer() {
        //PauseManager.Instance.PauseResumeGame();
        resumeButton.gameObject.SetActive(false);
        pauseGameOverText.text = "Game Over";
        respawnButton.gameObject.SetActive(true);
        ToggleMenu(pauseGameOverUIPanel.name, false, false);
    }

    #region setSounds
    private void AddHoverSound(Button button)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener((eventData) => AudioController.Play("hover"));
        trigger.triggers.Add(entry);
    }
    
    private void RespawnPlayer()
    {
        //PauseManager.Instance.PauseResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void AddConfirmSound()
    {
        AudioController.Play("confirm");
    }
    #endregion
    
    private void ShowEndGameScreen()
    {
        //PauseManager.Instance.PauseResumeGame();
        pauseGameOverText.text = "You Win!";
        resumeButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        
    }
    
    private void ToggleGraphics(bool disable) {
        if (disable) {
            graphics.SetActive(false);
            probes.SetActive(false);
        } else {
            graphics.SetActive(true);
            probes.SetActive(true);
        }
    }
}
