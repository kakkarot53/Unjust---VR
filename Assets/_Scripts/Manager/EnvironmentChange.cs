using CS.AudioToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
public class EnvironmentChange : MonoBehaviour
{
    //mainly just for classroom
    [Header("Warm Items")]
    [SerializeField] private GameObject[] warmGameObjs;
    [SerializeField] private Light[] warmLights;
    [SerializeField] private float warmTemperature;
    [SerializeField] private float warmIntensityMult = 5;
    [SerializeField] private Volume warmVolume; 

    [Header("Cold Items")]
    [SerializeField] private GameObject[] coldGameObjs;
    [SerializeField] private Light[] coldLights;
    [SerializeField] private float coldTemperature;
    [SerializeField] private float coldIntensityMult = 2;
    [SerializeField] private Volume coldVolume;  

    //[Header("VFX Graph Integration")]
    //[SerializeField] private VisualEffect roomVFX;        // Your custom DissolveFX graph instance
    //[SerializeField] private SphereCollider waveCollider; // The expanding trigger zone component
    //[SerializeField] private float expansionSpeed = 2f;  // How fast the wave expands across the room
    //[SerializeField] private float maxRadius = 15f;       // Fits the size footprint of your classroom
    //[SerializeField] private Vector3 sphereCenter;

    [Header("Headache Effect")]
    [SerializeField] private Volume globalVolume; //for chromatic abberation to kinda blur 
    [Header("Headache Volume Effects")]
    [SerializeField] private float startupDur = 0.5f;           // How fast the screen distorts 
    [SerializeField] private float fadeDur = 1.2f;              // How fast the screen recovers
    [SerializeField] private float minChrome = .15f;            // Chromatic Abberation Bounds
    [SerializeField] private float peakChrome = 1.0f;           
    [SerializeField] private float minLensDistortion = -0.2f;   // Lens Distortion bounds
    [SerializeField] private float peakLensDistortion = -0.4f; 
    [SerializeField] private float minContrast = 0f;            // Contrast bounds
    [SerializeField] private float peakContrast = 60f;          
    [SerializeField] private float minSaturation = -30f;        // Saturation bounds
    [SerializeField] private float peakSaturation = -70f;
    [SerializeField] private float minVignette = .25f;        // Saturation bounds
    [SerializeField] private float peakVignette = .5f;
    //[SerializeField] private float minFocalLen = 1f;        // Depth of Field bounds
    //[SerializeField] private float peakFocalLen = 25f;

    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private ShadowsMidtonesHighlights shadows;
    //private DepthOfField depthOfField;

    private Coroutine singleGaspRoutine;

    private InputSystem input;

    public bool isColdState = false;

    private float glitchTimer = 0f;
    private bool isGlitching = false;

    //private float waveRadius = 0f;
    //private bool isWaveExpanding = false;

    private Dictionary<Light, float> warmLightBaselines = new Dictionary<Light, float>();
    private Dictionary<Light, float> coldLightBaselines = new Dictionary<Light, float>();

    public static EnvironmentChange instance;
    private UnjustGameManager m_Game;
    private void Awake()
    {
        if(instance == null)
            instance = this;

        input = new InputSystem();
        input.Interaction.Enable();

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
            globalVolume.profile.TryGet(out lensDistortion);
            globalVolume.profile.TryGet(out colorAdjustments);
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out shadows);

            //globalVolume.profile.TryGet(out depthOfField);
        }

        input.Interaction.RoomShift.started += ctx => {
            AudioController.Play("ping");
            StartEyeCloseEffect(.8f, 2f, .5f, 1f, 1f, -1);
            
            //TriggerDimensionShift();
            //StartHeadacheEffect();
        };
    }

    private void Start()
    {
        m_Game = UnjustGameManager.instance;

        // Establish original boot baseline visibility states
        SetGroupState(warmGameObjs, true);
        SetGroupState(coldGameObjs, false);

        //save the values so no fuck ups
        CacheLightBaselines(warmLights, warmLightBaselines);
        CacheLightBaselines(coldLights, coldLightBaselines);

        m_Game.OnRoomChange += ChangePost;
        m_Game.OnRoomChange += ChangeAmbience;
        //just in case
        AudioController.Play("BGM_Classroom_Warm", 1f, 0, 0);
    }

    private void Update()
    {
        if (isGlitching)
        {
            AnimateHeadacheEffect();
        }

    }
    #region shifting effects
    public void TriggerDimensionShift()
    {
        //initially didnt use the audio system so thats why it looks weird af
        TemperatureShift();
    }

    private void TemperatureShift()
    {
        isColdState = !isColdState;

        // Execute the literal GameObject swap right at the peak moment of the trigger framework
        if (warmGameObjs != null && coldGameObjs != null)
        {
            foreach (GameObject obj in warmGameObjs)
            {
                if (obj != null) obj.SetActive(!isColdState);
            }
            foreach (GameObject obj in coldGameObjs)
            {
                if (obj != null) obj.SetActive(isColdState);
            }
        }

        // Apply lighting changes safely by referencing original baselines
        foreach (Light l in warmLights)
        {
            if (l == null) continue;
            l.colorTemperature = isColdState ? coldTemperature : warmTemperature;

            float baseline = warmLightBaselines.ContainsKey(l) ? warmLightBaselines[l] : l.intensity;
            l.intensity = isColdState ? baseline / warmIntensityMult : baseline;
        }

        foreach (Light l in coldLights)
        {
            if (l == null) continue;

            float baseline = coldLightBaselines.ContainsKey(l) ? coldLightBaselines[l] : l.intensity;
            l.intensity = isColdState ? baseline / coldIntensityMult : baseline;
        }

        if (warmVolume != null)
        {
            warmVolume.weight = isColdState ? 0 : 1;//Mathf.Lerp(1f, 0f, progress) : Mathf.Lerp(0f, 1f, progress);
        }
        if (coldVolume != null)
        {
            coldVolume.weight = isColdState ? 1 : 0;//Mathf.Lerp(0f, 1f, progress) : Mathf.Lerp(1f, 0f, progress);
        }

        ChangeAmbience(1);
    }

    #endregion

    #region headache effect
    public void StartHeadacheEffect()
    {
        if (isGlitching || chromaticAberration == null) return;

        isGlitching = true;
        glitchTimer = 0f;

        AudioController.Play("ping");
    }

    private void AnimateHeadacheEffect()
    {
        glitchTimer += Time.deltaTime;
        float totalDuration = startupDur + fadeDur;

        if (glitchTimer <= startupDur)
        {
            // Phase 1: Rapidly scale up the screen aberration lines
            float t = glitchTimer / startupDur;
            chromaticAberration.intensity.value = Mathf.Lerp(minChrome, peakChrome, t);
            lensDistortion.intensity.value = Mathf.Lerp(minLensDistortion, peakLensDistortion, t);
            colorAdjustments.contrast.value = Mathf.Lerp(minContrast, peakContrast, t);
            colorAdjustments.saturation.value = Mathf.Lerp(minSaturation, peakSaturation, t);
            vignette.intensity.value = Mathf.Lerp(minVignette, peakVignette, t);
            //depthOfField.focalLength.value = Mathf.Lerp(minFocalLen, peakFocalLen, t);
        }
        else if (glitchTimer <= totalDuration)
        {
            // Phase 2: Smoothly settle back down to standard focus clarity
            float t = (glitchTimer - startupDur) / fadeDur;
            chromaticAberration.intensity.value = Mathf.Lerp(peakChrome, minChrome, t);
            lensDistortion.intensity.value = Mathf.Lerp(peakLensDistortion, minLensDistortion, t);
            colorAdjustments.contrast.value = Mathf.Lerp(peakContrast, minContrast, t);
            colorAdjustments.saturation.value = Mathf.Lerp(peakSaturation, minSaturation, t);
            vignette.intensity.value = Mathf.Lerp(peakVignette, minVignette, t);
            //depthOfField.focalLength.value = Mathf.Lerp(peakFocalLen, minFocalLen, t);


        }
        else
        {
            // Reset and clean up tracking parameters
            chromaticAberration.intensity.value = minChrome;
            lensDistortion.intensity.value = minLensDistortion;
            colorAdjustments.contrast.value = minContrast;
            colorAdjustments.saturation.value = minSaturation;
            vignette.intensity.value = minVignette;
            //depthOfField.focalLength.value = minFocalLen;

            isGlitching = false;
        }
    }
    #endregion

    #region Flashbang
    /// fadeOutDuration => How fast the screen go white
    /// fadeInDuration  => How fast the screen returns to normal 
    /// maxPostValue    => The peak exposure value for the blinding effect
    /// onFlashPeak     => The method block to execute while the screen is fully obscured
    public void PlayTransitionFlash(float fadeOutDuration, float fadeInDuration, float maxPostValue, Action onFlashPeak)
    {
        AudioController.Play("ping");

        if (colorAdjustments == null)
        {
            Debug.LogError("[EnvironmentChange] Cannot execute flash transition: ColorAdjustments component reference is missing!");
            // Fallback: If post processing is broken, execute the action immediately so the player isn't softlocked!
            onFlashPeak?.Invoke();
            return;
        }

        StartCoroutine(FlashTransitionRoutine(fadeOutDuration, fadeInDuration, maxPostValue, onFlashPeak));
    }

    private IEnumerator FlashTransitionRoutine(float fadeOut, float fadeIn, float peakExposure, Action onFlashPeak)
    {
        float elapsed = 0f;
        float startExposure = colorAdjustments.postExposure.value;

        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOut;
            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, peakExposure, t);
            yield return null;
        }
        colorAdjustments.postExposure.value = peakExposure;

        onFlashPeak?.Invoke();

        yield return new WaitForSeconds(0.1f);

        elapsed = 0f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeIn;
            colorAdjustments.postExposure.value = Mathf.Lerp(peakExposure, startExposure, t);
            yield return null;
        }
        colorAdjustments.postExposure.value = startExposure;
    }
    #endregion

    #region Eye close Fade Out

    /// <summary>
    /// Effect 1: Closes eyes completely using distinct close/open speeds and custom intensity bounds.
    /// </summary>
    /// <param name="closingDuration">How long it takes to blink shut to black.</param>
    /// <param name="closedDuration">How long it closes the eyes </param>
    /// <param name="openDuration">How long it takes to blink open back to baseline.</param>
    /// <param name="maxIntensity">Target tightness of the vignette (1.0f is pure black).</param>
    /// <param name="maxSmoothness">Target blurriness of the eyelid edge.</param>
    /// <param name="targetRoomID">Pass -1 to skip room transition, or a valid index to warp.</param>
    public void StartEyeCloseEffect(float closingDuration, float closedDuration, float openDuration, float maxIntensity, float maxSmoothness, int targetRoomID)
    {
        if (vignette == null)
        {
            if (targetRoomID >= 0) UnjustGameManager.instance.RequestChangeRoom(targetRoomID, true);
            return;
        }

        StartCoroutine(EyeCloseRoutine(closingDuration, closedDuration, openDuration, maxIntensity, maxSmoothness, targetRoomID));
    }

    private IEnumerator EyeCloseRoutine(float closingDur, float closedDur, float openDur, float peakIntens, float peakSmooth, int targetRoomID)
    {
        float elapsed = 0f;
        float startIntensity = vignette.intensity.value;
        float startSmoothness = vignette.smoothness.value;

        // 1. Eyelids clamping down shut
        while (elapsed < closingDur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / closingDur;

            vignette.intensity.value = Mathf.Lerp(startIntensity, peakIntens, t);
            vignette.smoothness.value = Mathf.Lerp(startSmoothness, peakSmooth, t);
            yield return null;
        }

        // Snap precisely to the requested peak parameters
        vignette.intensity.value = peakIntens;
        vignette.smoothness.value = peakSmooth;

        shadows.active = true;
        yield return new WaitForSeconds(closedDur *.75f);

        // 2. Perform the critical data warp mid-blackout frame
        if (targetRoomID >= 0)
        {
            UnjustGameManager.instance.RequestChangeRoom(targetRoomID, true);
        }

        // Settle buffer padding frame gap
        yield return new WaitForSeconds(closedDur * .25f);
        shadows.active = false;

        // 3. Eyelids fluttering back open to default room visibility parameters
        elapsed = 0f;
        while (elapsed < openDur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openDur;

            vignette.intensity.value = Mathf.Lerp(peakIntens, minVignette, t);
            vignette.smoothness.value = Mathf.Lerp(peakSmooth, 0.35f, t);
            yield return null;
        }

        vignette.intensity.value = minVignette;
        vignette.smoothness.value = 0.35f;
    }
    #endregion

    #region hard breathing
    public void ExecuteSingleGaspPulse(float peakTime, float holdTime, float fadeTime, float maxIntens, float maxSmooth)
    {
        if (vignette == null) return;

        if (singleGaspRoutine != null) StopCoroutine(singleGaspRoutine); //just in case accidentally piled up so clear prev and play a new one

        singleGaspRoutine = StartCoroutine(SingleGaspRoutine(peakTime, holdTime, fadeTime, maxIntens, maxSmooth));
    }

    private IEnumerator SingleGaspRoutine(float peakTime, float holdTime, float fadeTime, float targetIntens, float targetSmooth)
    {
        float elapsed = 0f;

        // --- CONTRACT IN: Sharp, sudden panic grab ---
        while (elapsed < peakTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / peakTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            vignette.intensity.value = Mathf.Lerp(minVignette, targetIntens, smoothT);
            vignette.smoothness.value = Mathf.Lerp(0.2f, targetSmooth, smoothT);
            yield return null;
        }
        yield return new WaitForSeconds(holdTime);

        elapsed = 0f;
        // --- RELAX OUT: Sinking slowly back to the room baseline ---
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            vignette.intensity.value = Mathf.Lerp(targetIntens, minVignette, smoothT);
            vignette.smoothness.value = Mathf.Lerp(targetSmooth, 0.35f, smoothT);
            yield return null;
        }

        // Lock safely back down to base clear room vision configurations
        vignette.intensity.value = minVignette;
        vignette.smoothness.value = 0.35f;
        singleGaspRoutine = null;
    }
    #endregion

    private void CacheLightBaselines(Light[] lights, Dictionary<Light, float> targetDictionary)
    {
        if (lights == null) return;
        foreach (Light l in lights)
        {
            if (l != null && !targetDictionary.ContainsKey(l))
            {
                targetDictionary.Add(l, l.intensity);
            }
        }
    }

    private void SetGroupState(GameObject[] array, bool state)
    {
        if (array == null) return;
        foreach (GameObject obj in array)
        {
            if (obj != null) obj.SetActive(state);
        }
    }

    private void ChangePost(int _i)
    {
        switch (_i)
        {
            case 0:
                if (isColdState == true)
                    TemperatureShift();
                break;
            case 1:
                if (isColdState == true)
                    TemperatureShift();
                break;
            case 2:
                if (shadows != null) shadows.active = true;
                break;
            case 3:
                if (shadows != null) shadows.active = false;
                break;
        }
    }

    private void ChangeAmbience(int _i)
    {
        switch (_i)
        {
            case 0:
                if (AudioController.IsPlaying("BGM_Classroom_Warm")) return;

                AudioController.StopCategory("BGM", 0.5f);
                AudioController.StopCategory("SFX", 0.5f);
                AudioController.Play("BGM_Classroom_Warm", 1f, 0, 0);
                break;
            case 1:
                if (isColdState == false)
                {
                    if (AudioController.IsPlaying("BGM_Classroom_Warm")) return;

                    AudioController.StopCategory("BGM", 0.5f);
                    AudioController.Play("BGM_Classroom_Warm", 1f, 0, 0);
                }
                else
                {
                    if (AudioController.IsPlaying("Loopable creepy")) return;

                    AudioController.StopCategory("BGM", 0.5f);
                    AudioController.Play("Loopable creepy", 1f, 0, 0);
                }
                break;
            case 2:
                break;
            case 3:
                if (AudioController.IsPlaying("BGM_court")) return;

                AudioController.StopCategory("BGM", 0.5f);
                AudioController.Play("BGM_court", 1f, 0, 0);
                break;
            case 4:
                AudioController.StopCategory("BGM", 0.5f);
                break;
        }
    }
}
