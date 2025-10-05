using System;
using System.Collections.Generic;

namespace S2HD.Animation
{
    public class EffectEventManager
    {
        private List<EffectEvent> _activeEvents = new List<EffectEvent>();

        public void BeginEvent(IEnumerable<UpdateResult> effect)
        {
            _activeEvents.Add(new EffectEvent(effect));
        }

        public void Update()
        {
            for (int i = _activeEvents.Count - 1; i >= 0; i--)
            {
                if (!_activeEvents[i].Update())
                {
                    _activeEvents.RemoveAt(i);
                }
            }
        }
    }

    public class EffectEvent
    {
        private IEnumerator<UpdateResult> _enumerator;

        public EffectEvent(IEnumerable<UpdateResult> effect)
        {
            _enumerator = effect.GetEnumerator();
        }

        public bool Update()
        {
            if (_enumerator.MoveNext())
            {
                return true;
            }
            return false;
        }
    }

    public enum UpdateResult
    {
        Next
    }
}
