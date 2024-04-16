using System.Collections.Generic;

namespace Core.Managers.Analytics
{
    public class AnalyticsEvent : GameEvent
    {
        public string Name { get; private set; }

        public List<Parameter> Parameters { get; private set; }

        public AnalyticsEvent(string name)
        {
            Name = name;
            Parameters = new List<Parameter>();
        }

        public AnalyticsEvent AddParameter(string parameterName, string parameterValue)
        {
            return AddParameter(new Parameter(parameterName, parameterValue));
        }

        public AnalyticsEvent AddParameter(Parameter parameter)
        {
            Parameters.Add(parameter);
            return this;
        }
    }
}