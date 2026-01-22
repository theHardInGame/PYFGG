using PYFGG.GameActionSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Dash", menuName = "Gameplay/Actions/Player/Dash")]
public class DashActionConfig : ActionConfig
{
    [Header("Dash Data")]
    public float dashSpeed;
    public float dashDistance;
}