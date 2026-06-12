using UnityEngine;
using CS.AudioToolkit;
public class ScaleHitSFX : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
            AudioController.Play("metal-hit");
    }
}
