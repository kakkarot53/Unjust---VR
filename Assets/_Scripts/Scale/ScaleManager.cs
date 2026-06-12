using CS.AudioToolkit;
using System;
using System.Collections;
using UnityEngine;

public class ScaleManager : MonoBehaviour
{
    [Header("Scale Cups")]
    [SerializeField] private ScaleCup defenseCup;
    [SerializeField] private ScaleCup accusationCup;

    [SerializeField] private Transform leftCupPivot;
    [SerializeField] private Transform rightCupPivot;

    [Header("Scale Beam ")]
    [SerializeField] private Transform scaleBeamPivot;
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float tiltSmoothSpeed = 4f;

    [Header("Audio Toolkit Sound IDs")]
    [SerializeField] private string creakSoundName = "metal-creak";

    [Header("Target Room Config")]
    [SerializeField] private int scalePuzzleRoomID = 3;

    private float accWeight;
    private float defWeight;

    private float currentZRotation = 0f;
    private Quaternion initBeamRot;
    private Quaternion initLeftCupRot;
    private Quaternion initRightCupRot;
    private bool puzzleSolved = false;
    private Coroutine tiltRoutine;

    private UnjustGameManager m_game;

    public Action OnPuzzleSolved;

    private void Awake()
    {
        if (scaleBeamPivot != null) initBeamRot = scaleBeamPivot.localRotation;
        if (leftCupPivot != null) initLeftCupRot = leftCupPivot.localRotation;
        if (rightCupPivot != null) initRightCupRot = rightCupPivot.localRotation;
    }
    private void Start()
    {
        m_game = UnjustGameManager.instance;

        if (m_game != null) m_game.OnRoomChange += ResetPuzzle;
        if (defenseCup != null) defenseCup.OnWeightChanged += UpdateScale;
        if (accusationCup != null) accusationCup.OnWeightChanged += UpdateScale;

        UpdateScale();
    }

    private void OnDestroy()
    {
        if (m_game != null) m_game.OnRoomChange -= ResetPuzzle;
        if (defenseCup != null) defenseCup.OnWeightChanged -= UpdateScale;
        if (accusationCup != null) accusationCup.OnWeightChanged -= UpdateScale;
    }

    private void ResetPuzzle(int roomId)
    {
        if (roomId == scalePuzzleRoomID)
        {
            StartCoroutine(DelayedResetRoutine());
        }
    }

    private void UpdateScale()
    {
        if (puzzleSolved || scaleBeamPivot == null || defenseCup == null || accusationCup == null)
            return;

        defWeight = defenseCup.TotalWeight;
        accWeight = accusationCup.TotalWeight;

        float targetZRotation = 0f;
        float netWeight = defWeight - accWeight;

        if (netWeight < 0)
        {
            // Accusation is winning (netWeight is between -3 and -1)
            float imbalanceFactor = netWeight / 3f;
            targetZRotation = imbalanceFactor * maxTiltAngle;
        }
        else if (netWeight > 0)
        {
            // Defense is winning (netWeight is between 1 and 4)
            float imbalanceFactor = netWeight / 4f;
            targetZRotation = imbalanceFactor * maxTiltAngle;
        }
        else
        {
            targetZRotation = 0f;
        }

        if (defWeight >= 4f && accWeight <= 3f)
        {
            ExecuteWinState(targetZRotation);
            return;
        }

        if (tiltRoutine != null) StopCoroutine(tiltRoutine);
        tiltRoutine = StartCoroutine(AnimateScaleMovement(targetZRotation));
    }

    private IEnumerator AnimateScaleMovement(float targetZ)
    {
        // Keep running until the Z values are mathematically close enough to snap
        while (Mathf.Abs(currentZRotation - targetZ) > 0.01f)
        {
            float previousZ = currentZRotation;

            // Perform the framing animation shift updates explicitly here
            currentZRotation = Mathf.MoveTowards(currentZRotation, targetZ, Time.deltaTime * tiltSmoothSpeed * 10f);

            scaleBeamPivot.localRotation = initBeamRot * Quaternion.Euler(0f, 0f, currentZRotation);
            leftCupPivot.localRotation = initLeftCupRot * Quaternion.Euler(0f, 0f, -currentZRotation);
            rightCupPivot.localRotation = initRightCupRot * Quaternion.Euler(0f, 0f, -currentZRotation);

            // Sound check logic bounds constraints
            if (Mathf.Abs(currentZRotation - previousZ) > 0.01f)
            {
                if (!AudioController.IsPlaying(creakSoundName))
                {
                    AudioController.Play(creakSoundName);
                }
            }

            yield return null; 
        }

        //make sure to apply the actual rotation
        currentZRotation = targetZ;
        scaleBeamPivot.localRotation = initBeamRot * Quaternion.Euler(0f, 0f, currentZRotation);
        leftCupPivot.localRotation = initLeftCupRot * Quaternion.Euler(0f, 0f, -currentZRotation);
        rightCupPivot.localRotation = initRightCupRot * Quaternion.Euler(0f, 0f, -currentZRotation);

        AudioController.Stop(creakSoundName, 0.2f);
        tiltRoutine = null;
    }

    private void ExecuteWinState(float targetZ)
    {
        puzzleSolved = true;

        if (tiltRoutine != null) StopCoroutine(tiltRoutine);

        float victoryExtraTilt = targetZ * 2f;

        Debug.Log("<color=green>[Puzzle Cleared]</color> Scale puzzle completed correctly.");

        AudioController.Stop(creakSoundName, 0.8f);
        AudioController.Play("gavel-finish");

        LeanTween.value(scaleBeamPivot.gameObject, currentZRotation, victoryExtraTilt, 3f)
            .setOnUpdate((float val) =>
            {
                currentZRotation = val;

                if (scaleBeamPivot != null)
                    scaleBeamPivot.localRotation = initBeamRot * Quaternion.Euler(0f, 0f, currentZRotation);

                if (leftCupPivot != null)
                    leftCupPivot.localRotation = initLeftCupRot * Quaternion.Euler(0f, 0f, -currentZRotation);

                if (rightCupPivot != null)
                    rightCupPivot.localRotation = initRightCupRot * Quaternion.Euler(0f, 0f, -currentZRotation);
            })
            .setOnComplete(() =>
            {
                AudioController.Play("ping");
                EnvironmentChange.instance.StartEyeCloseEffect(.8f, 2f, .5f, 1f, 1f, 4);
            });


    }

    private IEnumerator DelayedResetRoutine()
    {
        puzzleSolved = true; 

        yield return null; 

        if (defenseCup != null) defenseCup.ResetZone();
        if (accusationCup != null) accusationCup.ResetZone();

        currentZRotation = 0f;
        if (scaleBeamPivot != null) scaleBeamPivot.localRotation = initBeamRot;

        puzzleSolved = false;
        UpdateScale();

        Debug.Log("<color=lime>[Scale Reset]</color> Clean frame delay complete. All states synchronized successfully.");
    }
}
