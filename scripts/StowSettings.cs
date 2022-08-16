
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class StowSettings : UdonSharpBehaviour
{
    [Header("place this script as the first child of a pickup")]
    [SerializeField] private int sizeClass = 0;

    
    
    public int GetSizeClass()
    {
        return sizeClass;
    }

    
}
