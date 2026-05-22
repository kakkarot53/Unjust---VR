using UnityEngine;

public class DissolveSphereScript : MonoBehaviour
{
    [Header("Material Settings")]
    public Material classroomMaterial;

    [Header("Warm Items")]
    [SerializeField] private GameObject[] warmGameObjs;

    [Header("Cold Items")]
    [SerializeField] private GameObject[] coldGameObjs;


    [Header("Transition Settings")]
    [SerializeField] private Transform startPoint;
    public float targetRadius = 25f;
    public float totalDur = 2f;
    private float growthSpeed;

    private float currentRadius = 0f;
    private bool isExpanding = false;

    private void Start()
    {
        growthSpeed = targetRadius / (2 * 30f);
    }

    void Update()
    {
        // Spacebar acts as a toggle placeholder for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerDissolve();
        }

        // Handle the continuous radius sizing
        AnimateSphereRadius();
    }

    /// <summary>
    /// Call this function from an external script to toggle the expansion on and off.
    /// </summary>
    public void TriggerDissolve()
    {
        isExpanding = !isExpanding;
    }

    private void AnimateSphereRadius()
    {
        float target = isExpanding ? targetRadius : 0f;
        currentRadius = Mathf.MoveTowards(currentRadius, target, growthSpeed * Time.fixedDeltaTime);
        if (classroomMaterial != null)
        {
            classroomMaterial.SetVector("_SphereCenter", startPoint.position);
            classroomMaterial.SetFloat("_SphereRadius", currentRadius);
        }
    }

    [ContextMenu("Set Start Point To Current Position")]
    private void SyncStartPoint()
    {
        if(startPoint==null)
            startPoint = transform;
    }
}
