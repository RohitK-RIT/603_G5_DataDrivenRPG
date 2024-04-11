using Core.Managers.Events;

namespace Core.Managers
{
    public abstract class GameEvent
    {
        public void Raise(bool raiseOnce = false)
        {
            EventManager.Raise(this, raiseOnce);
        }
    }
}