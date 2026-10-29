#region Imports
using UnityEngine;
#endregion
#region CameraSettingSO

public abstract class CameraSettingSO : ScriptableObject
{
    [Header("Look")]
    [SerializeField, Min(0f)] private float _lookSpeed = 0.1f;
    [SerializeField] private bool _invertX;
    [SerializeField] private bool _invertY;

    [Header("View Transition")]
    [SerializeField, Min(0.01f)] private float _transitionDuration = 0.5f;
    [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public float LookSpeed => _lookSpeed;
    public float LookX(float input) => input * _lookSpeed * (_invertX ? -1f : 1f);
    public float LookY(float input) => input * _lookSpeed * (_invertY ? -1f : 1f);
    public float TransitionDuration => _transitionDuration;
    public float EvaluateTransition(float progress) =>
        _transitionCurve == null ? Mathf.Clamp01(progress) : _transitionCurve.Evaluate(Mathf.Clamp01(progress));
}
#endregion
