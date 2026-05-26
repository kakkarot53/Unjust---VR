using System.Collections;
using UnityEngine;
using TMPro;
using static Oculus.Interaction.Context;
using UnityEngine.UI;

public class InteractionInfoManager : MonoBehaviour
{
    [Header("Info Base Data")]
    [SerializeField] private Transform infoPar;
    [SerializeField] private CanvasGroup canvasAlpha;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject iconPrefab;

    [Header("Info Icon Data")]
    public Sprite button_X;
    public Sprite button_Y;
    public Sprite button_A;
    public Sprite button_B;
    public Sprite trigger_Left;
    public Sprite trigger_Right;
    public Sprite grip_Left;
    public Sprite grip_Right;
    public Sprite joystick_Left;
    public Sprite joystick_Right;

    public static InteractionInfoManager instance;
    private Coroutine currentFadeRoutine;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        SetAlpha(0);
        ClearAllSpawnedElements();
    }

    public void AddText(string text)
    {
        ResetActiveDisplayState();

        GameObject textFab = Instantiate(textPrefab, infoPar);
        if (textFab.TryGetComponent<TMP_Text>(out TMP_Text _Text))
        {
            _Text.text = text;
        }
    }

    public void AddSprite(string iconKeyword)
    {
        ResetActiveDisplayState();

        Sprite targetSprite = GetSpriteFromKeyword(iconKeyword);
        if (targetSprite == null)
        {
            Debug.LogWarning($"InteractionInfoManager: No sprite found matching keyword '{iconKeyword}'");
            return;
        }

        GameObject iconFab = Instantiate(iconPrefab, infoPar);
        if (iconFab.TryGetComponent<Image>(out Image img))
        {
            img.sprite = targetSprite;
        }
    }

    //change input according to needs
    public void RequestInfoDisappear(float waitTime, float fadeDuration)
    {
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
        }

        if (infoPar.childCount <= 0)
            return;

        currentFadeRoutine = StartCoroutine(FadeOutText(waitTime, fadeDuration));
    }

    private void ResetActiveDisplayState()
    {
        // Stop any active fading so the window stays fully illuminated while adding new instructions
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
        }
        SetAlpha(1f);
    }

    public void ClearAllSpawnedElements()
    {
        foreach (Transform child in infoPar)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator FadeOutText(float wait, float duration)
    {
        yield return new WaitForSeconds(wait); // delay

        float startAlpha = 1;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0, time / duration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0); //make sure its 0
        ClearAllSpawnedElements(); // Wipe clean AFTER the overlay is entirely invisible
        currentFadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        canvasAlpha.alpha = alpha;
    }

    private Sprite GetSpriteFromKeyword(string keyword)
    {
        return keyword.ToLower().Replace(" ", "") switch
        {
            "x" => button_X,
            "y" => button_Y,
            "a" => button_A,
            "b" => button_B,
            "ltrigger" => trigger_Left,
            "rtrigger" => trigger_Right,
            "lgrip" => grip_Left,
            "rgrip" => grip_Right,
            "lstick" => joystick_Left,
            "rstick" => joystick_Right,
            _ => null
        };
    }
}
