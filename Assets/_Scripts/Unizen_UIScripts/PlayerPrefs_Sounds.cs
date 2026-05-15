using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CS.AudioToolkit;
using UnityEngine.SceneManagement;
public class PlayerPrefs_Sounds : MonoBehaviour
{
    public static PlayerPrefs_Sounds Instance { get; private set; }

    //save this and try to find this everytime the scene is loaded
    private AudioController audioCont;

    private float bgmVol = 0.5f, sfxVol = 0.5f;
    private void Awake()
    {
        //set as an instance to save the audio settings from main menu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        audioCont = FindObjectOfType<AudioController>();

        if (audioCont != null)
        {
            SetBGMVol(bgmVol);
            SetSFXVol(sfxVol);
        }
    }

    public void SetBGMVol(float vol)
    {
        bgmVol = vol;
        AudioController.SetCategoryVolume("BGM", vol);
    }    
    public void SetSFXVol(float vol)
    {
        sfxVol = vol;
        AudioController.SetCategoryVolume("SFX", vol);
    }
}
