using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private bool _occupied;

    public bool Occupied => _occupied;

    public void SetOccupied(bool occupied)
    {
        _occupied = occupied;
    }
}