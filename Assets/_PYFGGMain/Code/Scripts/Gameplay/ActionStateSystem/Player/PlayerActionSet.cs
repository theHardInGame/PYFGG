using System.Net.Security;
using PYFGG.GameActionSystem;
using UnityEngine;

public class PlayerActionSet : AgentTypeActionSetFactory
{
    [SerializeField] private JumpActionConfig jumpConfig;
    [SerializeField] private DashActionConfig dashConfig;

    public override IActionSetConfig Create()
    {
        ActionSetBuilder builder = new("PlayerActionSet");

        builder.AddAction<JumpAction>(jumpConfig).AddTrigger<JumpActionTrigger>();
        builder.AddAction<DashAction>(dashConfig).AddTrigger<DashActionTrigger>();

        return builder.Build();
    }
}
