using System.Collections.Generic;
using System.Linq;
using Core.Managers.Events;
using UnityEngine;

namespace Core.Managers.Analytics
{
    public class AnalyticsManager
    {
        private static AnalyticsManager s_instance;

        private static AnalyticsManager Instance
        {
            get { return s_instance ??= new AnalyticsManager(); }
        }

        private readonly List<SessionAnalyticsData> _sessionData = new() { new SessionAnalyticsData() };
        private SessionAnalyticsData CurrentData => _sessionData.Last();

        public static AnalyticsManager Activate()
        {
            return Instance;
        }

        private AnalyticsManager()
        {
            EventManager.AddListener<StartSessionEvent>(OnSessionStarted);
            EventManager.AddListener<EndSessionEvent>(OnSessionEnded);
            EventManager.AddListener<AnalyticsEvent>(OnAnalyticsEventFired);
        }

        ~AnalyticsManager()
        {
            EventManager.RemoveListener<StartSessionEvent>(OnSessionStarted);
            EventManager.RemoveListener<EndSessionEvent>(OnSessionEnded);
            EventManager.RemoveListener<AnalyticsEvent>(OnAnalyticsEventFired);
        }

        private void OnAnalyticsEventFired(AnalyticsEvent @event)
        {
            CurrentData.CapturedEvents.Add(@event);
        }

        private void OnSessionStarted(StartSessionEvent startSessionEvent)
        {
            CurrentData.Start();
        }

        private void OnSessionEnded(EndSessionEvent endSessionEvent)
        {
            CurrentData.End();
            CurrentData.ExportSessionDataToCSV($"{Application.persistentDataPath}\\Session_{_sessionData.Count}_AnalyticsData.csv");
            _sessionData.Add(new SessionAnalyticsData());
        }

        public sealed class StartSessionEvent : GameEvent { }

        public sealed class EndSessionEvent : GameEvent { }
    }
}