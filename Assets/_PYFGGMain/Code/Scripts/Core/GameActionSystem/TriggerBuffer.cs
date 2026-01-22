using System;
using System.Collections.Generic;

namespace PYFGG.GameActionSystem
{
    internal class TriggerBuffer
    {
        private readonly int bufferSize;
        private readonly Queue<ActionRequest> buffer = new();

        internal TriggerBuffer(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }

        internal void Register(ActionRequest request)
        {
            if (buffer.Count >= bufferSize)
                return;

            buffer.Enqueue(request);

            OnBufferUpdate?.Invoke(buffer.Count > 0 ? buffer.Peek() : null);
        }

        internal void Tick(float time)
        {
            CleanupExpired(time);
        }

        internal bool Accept(out ActionRequest request)
        {
            if (buffer.Count == 0)
            {
                request = default;
                return false;
            }

            request = buffer.Dequeue();
                OnBufferUpdate?.Invoke(buffer.Count > 0 ? buffer.Peek() : null);
            return true;
        }

        private void CleanupExpired(float time)
        {
            if (buffer.Count == 0) return;
            

            while (buffer.Count > 0 && buffer.Peek().expireTime <= time)
            {
                buffer.Dequeue();

                OnBufferUpdate?.Invoke(buffer.Count > 0 ? buffer.Peek() : null);
            }
        }

        internal event Action<ActionRequest?> OnBufferUpdate;
    }
}
