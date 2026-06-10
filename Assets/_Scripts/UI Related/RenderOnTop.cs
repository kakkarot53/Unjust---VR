using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RenderOnTop : MonoBehaviour
{
    [SerializeField] private Material onTopMaterial; // your RenderLastMat

    void Start()
    {
        // Handle regular UI Images and RawImages
        foreach (var image in GetComponentsInChildren<Image>(true))
            image.material = onTopMaterial;

        foreach (var raw in GetComponentsInChildren<RawImage>(true))
            raw.material = onTopMaterial;

        // Handle TMPro separately — must use fontMaterial
        foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            tmp.fontMaterial.SetInt("_ZTestMode", 8); // Always
            tmp.UpdateMeshPadding();
        }
    }
}
