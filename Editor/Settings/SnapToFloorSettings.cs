using UnityEditor;
using UnityEngine;

public class SnapToFloorSettings : ScriptableObject
{
    public enum SnapMode
    {
        _3D,
        _2D,
    }
    
    public enum ELanguage
    {
        English,
        한국어,
    }
    
    public enum StarUp
    {
        Always,
        Never,
    }
    
    [field: SerializeField] public SnapMode Mode { get; set; }

    [field: SerializeField] public ELanguage Language { get; set; }

    [field: SerializeField] public StarUp ShowUp { get; set; }
}