using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class FinishLinePublisher : MonoBehaviour
{
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private bool _publishOnlyOnce = true;
    private bool _hasPublished;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_publishOnlyOnce && _hasPublished)
            return;

        Transform player = other.transform.root;
        if (!player.CompareTag(_playerTag))
            return;

        _hasPublished = true;
        if (EventBus.Instance)
            EventBus.Instance.Publish(new GameClearEvent());
    }
}
