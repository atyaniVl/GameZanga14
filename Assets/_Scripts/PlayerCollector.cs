using System;
using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    public int KeysCollected { get; private set; } = 0;
    public int MapPiecesCollected { get; private set; } = 0;

    public event Action<int> OnKeyCollected;
    public event Action<int> OnMapPieceCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Key"))
        {
            KeysCollected++;
            OnKeyCollected?.Invoke(KeysCollected);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("MapPiece"))
        {
            MapPiecesCollected++;
            OnMapPieceCollected?.Invoke(MapPiecesCollected);
            Destroy(other.gameObject);
        }
    }
}