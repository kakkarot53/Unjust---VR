using UnityEngine;

public abstract class BaseInteractible : MonoBehaviour, IInteractible
{
    [Header("Layer Hover Settings")]
    [SerializeField] private LayerMask oriLayer;    // No outline effect layer
    [SerializeField] private LayerMask targetLayer; // Outline/Highlight effect layer
    [SerializeField] private LayerMask defaultLayer; // Outline/Highlight effect layer

    [SerializeField] private GameObject[] changeExtraObj;

    protected int originalLayerIndex;
    protected int targetLayerIndex;
    protected int defaultLayerIndex;
    protected bool isInteracting;
    [SerializeField] private bool canInteract = true;   //can player initially interact with this
    [SerializeField] private protected int levelID;               //when to fully reset it to the initial canInteract
    protected bool oriCanInteract;

    private UnjustGameManager m_game;

    protected virtual void Awake()
    {
        originalLayerIndex = LayerMaskToLayer(oriLayer);
        targetLayerIndex = LayerMaskToLayer(targetLayer);
        defaultLayerIndex = LayerMaskToLayer(defaultLayer); // Cached index numerical representation
    }

    protected virtual void Start()
    {
        m_game = UnjustGameManager.instance;

        if (m_game != null)
        {
            m_game.OnRoomChange += ResetInteractibleState;
        }

        oriCanInteract = canInteract;
        ResetToDefaultLayerState();
    }

    // VR Pro-Tip: Always unsubscribe from global managers when destroyed to avoid phantom memory bugs!
    protected virtual void OnDestroy()
    {
        if (m_game != null)
        {
            m_game.OnRoomChange -= ResetInteractibleState;
        }
    }

    public virtual void OnHoverEnter()
    {
        if (!canInteract || isInteracting) return;

        SetLayerOnAll(targetLayerIndex);
    }

    public virtual void OnHoverExit()
    {
        if (!canInteract || isInteracting) return;

        SetLayerOnAll(originalLayerIndex);
    }

    public virtual void Interact()
    {
        if (!canInteract) return;

        isInteracting = !isInteracting;

        if (isInteracting)
        {
            // FIX: Use the cached index (defaultLayerIndex) instead of the LayerMask container (defaultLayer)!
            SetLayerOnAll(defaultLayerIndex);
        }
        else
        {
            SetLayerOnAll(originalLayerIndex);
        }
    }

    public virtual void SetCanInteract(bool state)
    {
        canInteract = state;
    }

    public virtual bool CanInteract()
    {
        return canInteract;
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

    private void SetLayerOnAll(int targetIndex)
    {
        gameObject.layer = targetIndex;
        if (changeExtraObj != null && changeExtraObj.Length > 0)
        {
            foreach (GameObject g in changeExtraObj)
            {
                if (g != null) g.layer = targetIndex;
            }
        }
    }

    private void ResetToDefaultLayerState()
    {
        SetLayerOnAll(originalLayerIndex);
    }

    protected virtual void ResetInteractibleState(int _id)
    {
        if (_id == levelID)
        {
            canInteract = oriCanInteract;

            // FIX: Revert the operational states back to absolute default baselines
            isInteracting = false;
            ResetToDefaultLayerState();

            //Debug.Log($"<color=cyan>[Reset]</color> {gameObject.name} fully cleared logic flags and returned to original layout layer indices.");
        }
    }
}
