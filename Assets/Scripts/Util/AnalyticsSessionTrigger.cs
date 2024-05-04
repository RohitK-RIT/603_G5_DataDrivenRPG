using Core.Managers.Analytics;
using UnityEngine;

namespace Util
{
    public class AnalyticsSessionTrigger : MonoBehaviour
    {
        private void OnEnable()
        {
            new AnalyticsManager.StartSessionEvent().Raise();
        }
        
        private void OnDisable()
        {
            new AnalyticsManager.EndSessionEvent().Raise();
        }
    }
}