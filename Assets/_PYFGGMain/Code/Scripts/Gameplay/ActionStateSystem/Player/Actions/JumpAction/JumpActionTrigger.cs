using PYFGG.GameActionSystem;
using UnityEngine.UI;

public class JumpActionTrigger : ActionTriggerBase
{
    protected override void Activate()
    {
        PrepareData();

        InputManager.Instance.JumpInputEvent += OnJump;
    }

    protected override void Deactivate()
    {
        InputManager.Instance.JumpInputEvent -= OnJump;
    }

    private void OnJump(float f)
    {
        if(f > 0.5f)
        {
            FireTrigger(data);
            data.buttonRelease = false;
            return;
        }

        data.buttonRelease = true;
    }
    

    private JumpAction.Data data;
    private void PrepareData()
    {
        data = new(default);
    }
}
