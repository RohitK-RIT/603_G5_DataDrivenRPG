using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

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

        private int _enemiesKilled;
        private int _unitsLost;
        private Dictionary<string, int> weaponUsage = new();

        public bool HasStarted { get; private set; }

        private List<Unit> _trackedUnits = new();

        public SessionAnalyticsData()
        {
            StartDateTime = DateTime.MinValue;
            EndDateTime = DateTime.MinValue;
            HasStarted = false;
        }

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
            
            CalculateWeaponUsage();
            CalculateKilledUnits();
        }
        
        /// <summary>
        /// Function to calculate the weapon usage.
        /// </summary>
        private void CalculateWeaponUsage()
        {
            foreach (var @event in CapturedEvents)
            {
                if (@event is not WeaponUsedEvent weaponUsedEvent) 
                    continue;
                
                if (!weaponUsage.TryAdd(weaponUsedEvent.WeaponUsed.name, 1))
                    weaponUsage[weaponUsedEvent.WeaponUsed.name]++;
            }
        }
        
        /// <summary>
        /// Function to calculate the killed units.
        /// </summary>
        private void CalculateKilledUnits()
        {
            foreach (var @event in CapturedEvents)
            {
                if (@event is not UnitKilledEvent unitKilledEvent) 
                    continue;
                
                if (unitKilledEvent.UnitKilled.Hostility == Hostility.Hostile)
                    _enemiesKilled++;
                else
                    _unitsLost++;
            }
        }

        /// <summary>
        /// Function to export the session data to a CSV file.
        /// </summary>
        /// <param name="filePath"></param>
        public virtual void ExportSessionDataToCSV(string filePath)
        {
            try
            {
                // Create a StreamWriter to write to the CSV file
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

                // Write start and end date time. 
                writer.WriteLine($"StartDateTime,{StartDateTime}");
                writer.WriteLine($"EndDateTime,{EndDateTime}\n");
                writer.WriteLine($"Total Session Time,{TotalTimePlayed}");
                writer.Write($"Enemies Killed,{_enemiesKilled}");
                writer.WriteLine($",Units Lost,{_unitsLost}");
                foreach (var pair in weaponUsage)
                {
                    writer.WriteLine($"{pair.Key},{pair.Value}");
                }
                
                writer.WriteLine("Raw Captured Events");
                writer.WriteLine("Name, Parameter, Value");

                // Write the event and it's parameters
                foreach (var eventDataString in from @event in CapturedEvents
                         let eventDataString = $"{@event.Name}"
                         select @event.Parameters.Select(parameter => $",{parameter.Name}, {parameter.Value}\n")
                             .Aggregate(eventDataString, (current, paramString) => current + paramString))
                {
                    writer.WriteLine(eventDataString);
                }

                writer.Close();
            }
            catch (IOException ex)
            {
                Debug.LogError("Error writing to CSV file: " + ex.Message);
            }
        }
    }

    public class WeaponUsedEvent : AnalyticsEvent
    {
        public readonly Weapon WeaponUsed;

        public WeaponUsedEvent(Weapon weapon) : base("Weapon Used")
        {
            WeaponUsed = weapon;
        }
    }
    
    public class UnitKilledEvent : AnalyticsEvent
    {
        public readonly Unit UnitKilled;

        public UnitKilledEvent(Unit unitKilled) : base("Unit Killed")
        {
            UnitKilled = unitKilled;
        }
    }
}