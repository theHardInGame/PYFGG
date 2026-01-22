

namespace PYFGG.GameActionSystem
{
    public interface IAction
    {
        //ActionPhase CurrentPhase();
        internal void Start();
        internal void Update();
        internal void Kill();
        internal bool Run();
        internal bool CanInterrupt(ActionDefinition definition);
    }
}
