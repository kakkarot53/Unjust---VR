using UnityEngine;
public enum EvidenceSide
{
    Defense,
    Accusation,
    Other
}

public class WeightObj : MonoBehaviour
{
    [Header("evidence type")]
    [SerializeField] private EvidenceSide evidenceSide = EvidenceSide.Defense;

    public EvidenceSide GetSide() => evidenceSide;
}
