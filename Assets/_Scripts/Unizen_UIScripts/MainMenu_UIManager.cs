using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using CS.AudioToolkit;
using Unity.VisualScripting;

public class MainMenu_UIManager : MonoBehaviour
{
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Sliders")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("MainMenu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Buttons")]
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Button windowedButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Toggle graphicsToggle;

    [Header("Misc. Buttons")]
    [SerializeField] private Button backButton;

    [Header("Fade in to Game scene setup")]
    [SerializeField] private GameObject title;
    [SerializeField] private CanvasGroup fadeImg;
    
    public static bool disableGrahics = false;


    private List<Button> buttonsArr = new List<Button>();

    private FullScreenMode desiredScreenMode = FullScreenMode.FullScreenWindow;
    private void Start()
    {
        // Setup Scene
        AudioController.Play("MainMenuBGM");
        ApplyScreen();
        fadeImg.alpha = 0;

        // Button Callbacks - Main Menu
        startButton.onClick.AddListener(() => 
        {
            title.SetActive(false);
            foreach (Button button in buttonsArr)
            {
                button.gameObject.SetActive(false);
            }
                LeanTween.alphaCanvas(fadeImg, 1, 2f).setOnComplete(() =>
            {
                SceneManager.LoadScene(1);
            });
        });
        settingsButton.onClick.AddListener(() => ToggleMenu(settingsPanel.name, true));
        creditsButton.onClick.AddListener(() => ToggleMenu(creditsPanel.name, true));
        quitButton.onClick.AddListener(Application.Quit);

        // Button Callbacks - Settings
        fullscreenButton.onClick.AddListener(() => { desiredScreenMode = FullScreenMode.FullScreenWindow; });
        windowedButton.onClick.AddListener(() => { desiredScreenMode = FullScreenMode.Windowed; });
        applyButton.onClick.AddListener(ApplyScreen);

        // Back button
        backButton.onClick.AddListener(() => ToggleMenu(mainMenuPanel.name, false));

        // Initialize sliders
        musicVolumeSlider.value = AudioController.GetCategoryVolume("BGM");
        sfxVolumeSlider.value = AudioController.GetCategoryVolume("SFX");

        musicVolumeSlider.onValueChanged.AddListener(PlayerPrefs_Sounds.Instance.SetBGMVol);
        sfxVolumeSlider.onValueChanged.AddListener(PlayerPrefs_Sounds.Instance.SetSFXVol);

        // Populate buttonsArr with all assigned buttons
        buttonsArr.AddRange(new Button[] {
            startButton, settingsButton, creditsButton, quitButton,
            fullscreenButton, windowedButton, applyButton, backButton
        });

        // Add hover sound and click sound to each button in buttonsArr
        foreach (Button button in buttonsArr)
        {
            AddHoverSound(button);
            button.onClick.AddListener(AddConfirmSound);
        }
        
        graphicsToggle.onValueChanged.AddListener(ToggleGraphics);
    }

    private void ToggleMenu(string panelName, bool showBackButton)
    {
        // Deactivate all panels
        mainMenuPanel.SetActive(panelName.Equals(mainMenuPanel.name));
        settingsPanel.SetActive(panelName.Equals(settingsPanel.name));
        creditsPanel.SetActive(panelName.Equals(creditsPanel.name));

        backButton.gameObject.SetActive(showBackButton);
    }

    private void ApplyScreen()
    {
        Screen.fullScreenMode = desiredScreenMode;
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

    private void AddConfirmSound()
    {
        AudioController.Play("confirm");
    }
    #endregion

    public void ToggleGraphics(bool toggleValue) {
        disableGrahics = toggleValue;
    }
}
