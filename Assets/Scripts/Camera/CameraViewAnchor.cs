#region Imports
using UnityEngine;
using Kogetsu.Library.Attribute;
#endregion
#region CameraViewAnchor

[DisallowMultipleComponent]
public sealed class CameraViewAnchor : MonoBehaviour
{
    [SerializeField, EditOnInspector] private CameraSettingSO _settings;
    public CameraSettingSO Settings => _settings;
}
#endregion
