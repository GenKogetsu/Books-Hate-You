using UnityEngine;

namespace Kogetsu.Library.Core
{
    public sealed class BasicPingPongMovementAbility : MonoBehaviour
    {
        [SerializeField] private Vector3 _movementAxis = Vector3.right;
        [SerializeField, Min(0f)] private float _distance = 1f;
        [SerializeField, Min(0f)] private float _cyclesPerSecond = 0.5f;
        [SerializeField] private Space _movementSpace = Space.Self;
        [SerializeField] private bool _startAtCenter;

        private Vector3 _startPosition;
        private float _phase;

        private void Awake()
        {
            _startPosition = transform.position;
            _phase = _startAtCenter ? 0f : -Mathf.PI * 0.5f;
        }

        private void Update()
        {
            if (_distance <= 0f || _cyclesPerSecond <= 0f || _movementAxis.sqrMagnitude <= Mathf.Epsilon)
                return;

            _phase += Time.deltaTime * _cyclesPerSecond * Mathf.PI * 2f;
            float offset = Mathf.Sin(_phase) * _distance;
            Vector3 direction = _movementAxis.normalized * offset;

            if (_movementSpace == Space.Self && transform.parent)
                direction = transform.parent.TransformVector(direction);

            transform.position = _startPosition + direction;
        }
    }
}
