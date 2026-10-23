using UnityEngine;
using UnityEngine.Serialization;

namespace Kogetsu.Library.Core
{
    public sealed class BasicRotateAbility : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("Rotate")] private Vector3 _rotationAxis = Vector3.up;
        [SerializeField, Min(0f), FormerlySerializedAs("RoateSpeed")] private float _rotationSpeed = 90f;
        [SerializeField] private Space _rotationSpace = Space.Self;

        private void Update()
        {
            if (_rotationSpeed <= 0f || _rotationAxis.sqrMagnitude <= Mathf.Epsilon)
                return;

            transform.Rotate(_rotationAxis.normalized, _rotationSpeed * Time.deltaTime, _rotationSpace);
        }
    }
}
