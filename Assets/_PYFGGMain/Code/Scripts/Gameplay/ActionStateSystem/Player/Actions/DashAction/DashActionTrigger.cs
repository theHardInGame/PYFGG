using PYFGG.GameActionSystem;

public class DashActionTrigger : ActionTriggerBase
{
    protected override void Activate()
    {
        PrepareData();

        InputManager.Instance.DashInputEvent += OnDash;
    }

    protected override void Deactivate()
    {
        InputManager.Instance.DashInputEvent -= OnDash;
    }

    private void OnDash(float f)
    {
        if (f < 0.5f) return;

        FireTrigger(data);
    }

    private DashAction.Data data;
    private void PrepareData()
    {
        data = new();
    }
}