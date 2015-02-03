using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMartESImporter
{
    /// <summary>
    /// Log Class - logging infrastructure to capture all messages and errors into an ElasticSearch backend
    /// </summary>
    public class Log
    {
        public enum MessageType
        {
            Error,
            Information
        }

        public string message;
        public MessageType msgType;
        private DateTime timeStampLog;
        public DateTime timeStamp
        {
            get
            {
                return this.timeStampLog;
            }
            set
            {
                this.timeStampLog = value.ToUniversalTime();
            }

        }
        //This will be our 'RunID'
        public string runId;

        private Log()
        {
        }

        public static Log StartLog()
        {
            Log myLog = new Log();
            myLog.runId = Guid.NewGuid().ToString("N");
            return myLog;
        }

        /// <summary>
        /// Log messages to ElasticSearch LOG index and out to a log file
        /// </summary>
        public void LogMessage(string message, MessageType msgType = MessageType.Information)
        {
            LogESMessage(message, msgType);
        }

        /// <summary>
        /// Log a message out to the ES backend
        /// </summary>
        private void LogESMessage(string message, MessageType msgType = MessageType.Information)
        {
            string collectionName = "log_" + this.runId;

            this.msgType = msgType;
            this.message = message;
            timeStamp = DateTime.Now;
            ElasticSearchConnection.ESClient.IndexAsync(this, x => x.Index("logs").Type(collectionName));
        }

        /// <summary>
        /// Store the last completed run time
        /// </summary>
        public static void LogLastRunTime(DateTime lastRunDateTime)
        {
            ElasticSearchConnection.ESClient.Raw.IndexAsync("logs", "lastruntime", "1", Newtonsoft.Json.JsonConvert.SerializeObject(new { lastruntime = lastRunDateTime }));
        }

        /// <summary>
        /// Retrieve the last completed run time (in UTC)
        /// </summary>
        public static DateTime GetLastRunTime()
        {
            DateTime lastRunDateTime = DateTime.Now.ToUniversalTime();

            IList<string> ids = new List<string>();
            ids.Add("1"); //only valid ID, last run time is always just ID =1

            var documents = ElasticSearchConnection.ESClient.Search<JObject>(x => x.Index("logs").Type("lastruntime").Query(y => y.Ids(ids)));
            if (documents.Documents.Count() > 0)
            {
                var lastrunTime = documents.Documents.First()["lastruntime"];
                lastRunDateTime = DateTime.Parse(lastrunTime.ToString());
            }
            return lastRunDateTime;
        }

    }
}
