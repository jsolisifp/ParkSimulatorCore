using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public class DefaultLog : Log
    {
        const string infoPrefix = "INFO";
        const string warningPrefix = "WARNING";
        const string errorPrefix = "ERROR";

        enum LogEntryType
        {
            info,
            warning,
            error
        };

        string fileName;
        StreamWriter? writer;

        public DefaultLog()
        {
            fileName = "";
        }

        public override void Init(Config config)
        {
            fileName = config.GetTextValue("logFileName", "log.txt");
            writer = new StreamWriter(fileName, true);
        }
        
        public override void Finish()
        {
            Debug.Assert(writer != null, "Log no inicializado");

            writer.Close();
        }

        public override void LogMessage(string category, string message)
        {
            Log(category, message, LogEntryType.info);
        }

        public override void LogWarning(string category, string message)
        {
            Log(category, message, LogEntryType.warning);
        }

        public override void LogError(string category, string message)
        {
            Log(category, message, LogEntryType.error);
        }

        void Log(string category, string message, LogEntryType type = LogEntryType.info)
        {
            Debug.Assert(writer != null, "Log no inicializado");

            string prefix;
            if(type == LogEntryType.info) { prefix = infoPrefix; }
            else if(type == LogEntryType.info) { prefix = warningPrefix; }
            else { prefix = errorPrefix; }

            string date = DateTime.Now.ToString("Dyyyy'.'MM'.'dd'.T'HH'.'mm'.'ss");

            writer.WriteLine(prefix + ":" + category + ":" + date + ":" + message);
            writer.Flush();

        }

    }
}
