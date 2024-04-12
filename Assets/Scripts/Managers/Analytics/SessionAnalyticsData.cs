using System;
using System.Collections.Generic;

namespace Core.Managers.Analytics
{
    public class SessionAnalyticsData
    {
        /// <summary>
        /// Start time of the session.
        /// </summary>
        public DateTime StartDateTime { get; private set; }

        /// <summary>
        /// End time of the session.
        /// </summary>
        public DateTime EndDateTime { get; private set; }

        /// <summary>
        /// Property to check the total time played.
        /// </summary>
        public TimeSpan TotalTimePlayed => EndDateTime - StartDateTime;

        /// <summary>
        /// List of to access captured events.
        /// </summary>
        public List<AnalyticsEvent> CapturedEvents { get; private set; }

        public bool HasStarted { get; private set; }

        /// <summary>
        /// Function to be called when starting a session.
        /// </summary>
        public void Start()
        {
            StartDateTime = DateTime.Now;
            HasStarted = true;
            CapturedEvents = new List<AnalyticsEvent>();
        }

        /// <summary>
        /// Function to be called when ending a session.
        /// </summary>
        public void End()
        {
            EndDateTime = DateTime.Now;
        }
    }
}