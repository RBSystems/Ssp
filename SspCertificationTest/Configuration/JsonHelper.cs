using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

namespace SspCertificationTest.Configuration
{
    /// <summary>
    /// Helper class used to read the configuration JSON file and gather device mapping information.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Read the target file for JSON represntation of system information. The contents must be in a valid JSON format.
        /// </summary>
        /// <param name="filePath">The target file location with extension</param>
        /// <returns>the file contents or the empty string if the read failed. An error is logged upon failure</returns>
        /// <exception cref="ArgumentException">If filePath is null or an empty string.</exception>
        public static string ReadJsonData(string filePath)
        {
            if (filePath == null || filePath == string.Empty) throw new ArgumentException("filePath cannot be null or empty.");
            string data = string.Empty;
            using (StreamReader reader = new StreamReader(filePath))
            {
                data = reader.ReadToEnd();
            }
            return data;
        }

        /// <summary>
        /// Parses the supplied correctly formatted JSON string for a SystemConfiguration object. Any error encountered while
        /// attempting to parse the JSON object is logged in the processor error log.
        /// </summary>
        /// <param name="JsonData">a properly formatted Json representation of a SystemConfiguration object.</param>
        /// <returns>The system configuration information that was supplied by the given JSON data, or NULL if the parse failed</returns>
        /// <exception cref="ArgumentException">if 'jsonData' is null or the empty string.</exception>
        public static SystemConfiguration ParseConfigJson(string jsonData)
        {
            if (jsonData == null || jsonData == string.Empty) throw new ArgumentException("jsonData cannot be null or empty.");

            SystemConfiguration configData = null;
            try
            {
                configData = JsonConvert.DeserializeObject<SystemConfiguration>(jsonData);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Failed to parse system configuration data: {0} -- {1}", e.Message, e.StackTrace);
            }
            return configData;
        }

        /// <summary>
        /// Parses the supplied correctly formatted JSON string for a ScheduledEvents object. Any error encountered while
        /// attempting to parse the JSON object is logged in the processor error log.
        /// </summary>
        /// <param name="JsonData">a properly formatted Json representation of a ScheduledEvents object.</param>
        /// <returns>The scheduled events information that was supplied by the given JSON data, or NULL if the parse failed. an error is logged upon failure.</returns>
        /// <exception cref="ArgumentException">if 'jsonData' is null or the empty string.</exception>
        public static ScheduleEventData ParseScheduledEvents(string jsonData)
        {
            if (jsonData == null || jsonData == string.Empty) throw new ArgumentException("jsonData cannot be null or empty.");

            ScheduleEventData events = null;
            try
            {
                events = JsonConvert.DeserializeObject<ScheduleEventData>(jsonData);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Failed to parse system Scheduled event data: {0} -- {1}", e.Message, e.StackTrace);
            }
            return events;
        }
    }
}