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
    // give headache effect
    [SerializeField] private AudioSource mainEars; 
    [SerializeField] private AudioClip headachePing;
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

    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;
    private InputSystem input;

    // Cached Global ID Strings for extreme runtime efficiency in VR
    //private int sphereCenterID;
    //private int sphereRadiusID;

    private bool isColdState = false;

    private float glitchTimer = 0f;
    private bool isGlitching = false;

    //private float waveRadius = 0f;
    //private bool isWaveExpanding = false;

    private Dictionary<Light, float> warmLightBaselines = new Dictionary<Light, float>();
    private Dictionary<Light, float> coldLightBaselines = new Dictionary<Light, float>();

    public static EnvironmentChange instance;

    private void Awake()
    {
        if(instance == null)
            instance = this;

        //sphereCenterID = Shader.PropertyToID("_SphereCenter");
        //sphereRadiusID = Shader.PropertyToID("_SphereRad");

        input = new InputSystem();
        input.Interaction.Enable();

        input.Interaction.RoomShift.started += ctx => {
            TriggerDimensionShift();
            StartHeadacheEffect();
        };
    }

    private void Start()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
            globalVolume.profile.TryGet(out lensDistortion);
            globalVolume.profile.TryGet(out colorAdjustments);
        }

        // Establish original boot baseline visibility states
        SetGroupState(warmGameObjs, true);
        SetGroupState(coldGameObjs, false);

        //save the values so no fuck ups
        CacheLightBaselines(warmLights, warmLightBaselines);
        CacheLightBaselines(coldLights, coldLightBaselines);

        // Clear out global shader memory values on boot
        //Shader.SetGlobalVector(sphereCenterID, sphereCenter);
        //Shader.SetGlobalFloat(sphereRadiusID, 0f);

        //if (waveCollider != null) waveCollider.radius = 0f;
    }

    private void Update()
    {
        if (isGlitching)
        {
            AnimateHeadacheEffect();
        }

        //if (isWaveExpanding)
        //{
        //    AnimateWaveExpansion();
        //}
    }
    #region shifting effects
    public void TriggerDimensionShift()
    {
        //isWaveExpanding = true;
        //waveRadius = 0f;

        // Play sound cues accompanying the physical rip
        if (mainEars != null && headachePing != null)
        {
            mainEars.PlayOneShot(headachePing);
        }

        // Trigger the visual burst of mist particles inside VFX Graph
        //if (roomVFX != null)
        //{
        //    roomVFX.SendEvent("SpawnEvent");
        //}

        // Swap lighting states and toggle GameObjects instantly
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
            l.intensity = isColdState ? baseline * coldIntensityMult : baseline;
        }

        if (warmVolume != null)
        {
            warmVolume.weight = isColdState ? 0 : 1;//Mathf.Lerp(1f, 0f, progress) : Mathf.Lerp(0f, 1f, progress);
        }
        if (coldVolume != null)
        {
            coldVolume.weight = isColdState ? 1 : 0;//Mathf.Lerp(0f, 1f, progress) : Mathf.Lerp(1f, 0f, progress);
        }
    }

    //private void AnimateWaveExpansion()
    //{
    //    if (waveRadius < maxRadius)
    //    {
    //        waveRadius += expansionSpeed * Time.deltaTime;

    //        float progress = Mathf.Clamp01(waveRadius / maxRadius);

    //        // --- FIXED: VOLUME ATMOSPHERE WEIGHT LERPING LIVE ---
    //        if (warmVolume != null)
    //        {
    //            warmVolume.weight = isColdState ? 0 : 1;//Mathf.Lerp(1f, 0f, progress) : Mathf.Lerp(0f, 1f, progress);
    //        }
    //        if (coldVolume != null)
    //        {
    //            coldVolume.weight = isColdState ? 1 : 0;//Mathf.Lerp(0f, 1f, progress) : Mathf.Lerp(1f, 0f, progress);
    //        }

    //        // Sync physical trigger physics engine bounds
    //        if (waveCollider != null)
    //        {
    //            waveCollider.radius = waveRadius / Mathf.Max(transform.lossyScale.x, 0.001f);
    //        }

    //        // Sync Particle system nodes
    //        if (roomVFX != null)
    //        {
    //            roomVFX.SetVector3("SphereCenter", sphereCenter);
    //            roomVFX.SetFloat("SphereRadius", waveRadius);
    //        }

    //        // Send global values to your customized Shader Graph materials
    //        Shader.SetGlobalVector(sphereCenterID, sphereCenter);
    //        Shader.SetGlobalFloat(sphereRadiusID, waveRadius);
    //    }
    //    else
    //    {
    //        // Reset wave attributes once max size is achieved
    //        isWaveExpanding = false;
    //    }
    //}
    #endregion

    #region headache effect
    public void StartHeadacheEffect()
    {
        if (isGlitching || chromaticAberration == null) return;

        isGlitching = true;
        glitchTimer = 0f;
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
        }
        else if (glitchTimer <= totalDuration)
        {
            // Phase 2: Smoothly settle back down to standard focus clarity
            float t = (glitchTimer - startupDur) / fadeDur;
            chromaticAberration.intensity.value = Mathf.Lerp(peakChrome, minChrome, t);
            lensDistortion.intensity.value = Mathf.Lerp(peakLensDistortion, minLensDistortion, t);
            colorAdjustments.contrast.value = Mathf.Lerp(peakContrast, minContrast, t);
            colorAdjustments.saturation.value = Mathf.Lerp(peakSaturation, minSaturation, t);
        }
        else
        {
            // Reset and clean up tracking parameters
            chromaticAberration.intensity.value = minChrome;
            lensDistortion.intensity.value = minLensDistortion;
            colorAdjustments.contrast.value = minContrast;
            colorAdjustments.saturation.value = minSaturation;
            isGlitching = false;
        }
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
}
