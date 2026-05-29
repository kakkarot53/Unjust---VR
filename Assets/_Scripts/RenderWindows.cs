using UnityEngine;

public class RenderWindows : MonoBehaviour
{
    public RenderTexture cubeTargetTexture;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (cam != null && cubeTargetTexture != null)
        {
            // This forces the camera to render all 6 faces of the cube dynamically
            cam.RenderToCubemap(cubeTargetTexture);
        }
    }
}
