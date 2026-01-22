using UnityEngine;

namespace PYFGG.GameActionSystem
{
    public abstract class AgentTypeActionSetFactory : MonoBehaviour
    {
        public abstract IActionSetConfig Create();
    }
}
