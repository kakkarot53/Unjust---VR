using UnityEngine;

public abstract class BaseInteractible : MonoBehaviour, IInteractible
{
    [Header("Layer Hover Settings")]
    [SerializeField] private LayerMask oriLayer;    // No outline effect layer
    [SerializeField] private LayerMask targetLayer; // Outline/Highlight effect layer

    protected int originalLayerIndex;
    protected int targetLayerIndex;
    protected bool isInteracting;

    protected virtual void Awake()
    {
        originalLayerIndex = LayerMaskToLayer(oriLayer);
        targetLayerIndex = LayerMaskToLayer(targetLayer);
    }

    protected virtual void Start()
    {
        gameObject.layer = originalLayerIndex;
    }

    // Automatically handles turning the outline ON
    public virtual void OnHoverEnter()
    {
        if (isInteracting) return;
        gameObject.layer = targetLayerIndex;
    }

    // Automatically handles turning the outline OFF
    public virtual void OnHoverExit()
    {
        if (isInteracting) return;
        gameObject.layer = originalLayerIndex;
    }

    // This fulfills the interface, but forces child classes to handle the specifics
    public abstract void Interact();

    public virtual bool CanInteract()
    {
        return true;
    }

    private int LayerMaskToLayer(LayerMask mask)
    {
        int bitmask = mask.value;
        if (bitmask == 0) return 0;

        int layer = 0;
        while (bitmask > 1)
        {
            bitmask >>= 1;
            layer++;
        }
        return layer;
    }
}
