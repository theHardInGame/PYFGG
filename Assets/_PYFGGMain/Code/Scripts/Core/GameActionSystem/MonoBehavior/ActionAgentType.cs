using UnityEngine;

namespace PYFGG.GameActionSystem
{
    public abstract class ActionAgentType : MonoBehaviour, IActionAgent
    {
        [SerializeField] protected Animator animator;
        public abstract ActionContext CreateContext();
    }
}