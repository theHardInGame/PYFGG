
namespace PYFGG.GameActionSystem
{
    internal struct ActionRequest
    {
        public readonly ActionDefinition definition;
        public readonly IActionData data;
        public readonly float expireTime;

        public ActionRequest(ActionDefinition definition, IActionData data, float expireTime)
        {
            this.definition = definition;
            this.data = data;
            this.expireTime = expireTime;
        }
    }
}