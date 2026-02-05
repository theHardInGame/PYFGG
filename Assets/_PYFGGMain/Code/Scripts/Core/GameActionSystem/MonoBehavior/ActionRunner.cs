using UnityEngine;
using TMPro;

namespace PYFGG.GameActionSystem
{
    public sealed class ActionRunner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private string runnerName;
        [SerializeField] private AgentTypeActionSetFactory actionSetFactory;
        [SerializeField] private ActionAgentType actionAgent;
        [SerializeField] private int bufferSize = 2;
        [SerializeField] internal float bufferLife = 1f;
        [SerializeField] private TextMeshProUGUI debugText;

        private IActionSetConfig actionSetConfig;
        private TriggerBuffer buffer;
        private ActionContext actionContext;


        private IAction runningAction;
        private ActionRequest? bufferedRequest;


        #region Lazy Accessors
        internal IActionSetConfig ActionSetConfig => actionSetConfig ??= actionSetFactory.Create();
        internal TriggerBuffer Buffer => buffer ??= new TriggerBuffer(bufferSize);
        internal ActionContext AgentContext => actionContext ??= actionAgent.CreateContext();
        #endregion


        private void OnEnable()
        {
            Buffer.OnBufferUpdate += OnBufferUpdated;           
        }

        private void OnDisable()
        {
            Buffer.OnBufferUpdate -= OnBufferUpdated;
        }

        private void FixedUpdate()
        {
            Buffer.Tick(Time.time);

            TryConsumeBufferedRequest();

            RunAction();
        }

        #region Buffer Handling

        private void OnBufferUpdated(ActionRequest? request)
        {
            bufferedRequest = request;
        }

        private void TryConsumeBufferedRequest()
        {
            if (!bufferedRequest.HasValue) return;

            ActionRequest request = bufferedRequest.Value;

            if (IsContinuousRefresh(request))
            {
                Buffer.Accept(out _);
                runningAction.Update();
                return;
            }

            if (IsAllowedToReplaceCurrentAction(request))
            {
                runningAction?.Kill();
                runningAction = null;
                StartAction(bufferedRequest.Value);
            }
        }

        private bool IsAllowedToReplaceCurrentAction(ActionRequest request)
        {
            if (runningAction != null)
            {
                switch(request.definition.config.actionInterruptionPolicy)
                {
                    case ActionInterruptionAuthority.IfAllowed:
                        if (runningAction.CanInterrupt(request.definition)) break;
                        else return false;

                    case ActionInterruptionAuthority.Force:
                        break;

                    case ActionInterruptionAuthority.Never:
                        return false;
                }
            }

            return request.definition.CanStart(AgentContext);
        }

        private bool IsContinuousRefresh(ActionRequest request)
        {
            return runningAction != null && request.definition.config.actionMode == ActionMode.Continuous && runningAction.GetType() == request.definition.ActionType;
        }

        #endregion

        #region Action Execution

        private void StartAction(ActionRequest request)
        {
            Buffer.Accept(out ActionRequest r);
            runningAction = request.definition.Create(AgentContext, request.data);
            runningAction.Start();

            debugText.text = $"Current Action:\n{ r.definition.config.actionName }";
        }

        private void RunAction()
        {
            if (runningAction == null) return;

            if (!runningAction.Run())
            {
                runningAction.Kill();
                runningAction = null;
                debugText.text = "Current Action:\nNo Action";
            }
        }

        #endregion
    }
}
