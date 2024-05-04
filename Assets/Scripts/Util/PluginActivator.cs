using System;
using Core.Managers.Analytics;
using Core.Managers.Events;
using UnityEngine;

namespace Util
{
    public class PluginActivator : MonoBehaviour
    {
        private void Start()
        {
            EventManager.Activate();
            AnalyticsManager.Activate();
        }
    }
}