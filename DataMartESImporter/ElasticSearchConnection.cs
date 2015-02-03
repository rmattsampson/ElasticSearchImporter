using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMartESImporter
{
    /// <summary>
    /// Static class to hold all of the Elastic Search Connection Information
    /// </summary>
    public static class ElasticSearchConnection
    {
        //static constructor to ensure config settings are set
        static ElasticSearchConnection()
        {
            DropShareServerName = DataMartAppSettings.Default.dropshareServerName;
            DropShare = DataMartAppSettings.Default.dropshareFolderName;
            ElasticSearchUrl = DataMartAppSettings.Default.elasticSearchURL;
            ConnectionSettings conSettings = new ConnectionSettings(new Uri(ElasticSearchUrl));
            ESClient = new ElasticClient(conSettings);

            //Create the logging index in the Server if it doesn't already exists
            if (!ESClient.IndexExists("logs").Exists)
            {
                ESClient.CreateIndex("logs");
            }
        }

        private static ElasticClient esClient;
        private static string dropShare;
        private static string elasticSearchUrl;
        private static string dropShareServerName;

        public static ElasticClient ESClient
        {
            get { return esClient; }
            private set
            {
                esClient = value;
            }
        }

        public static string DropShare
        {
            get { return dropShare; }
            private set
            {
                dropShare = value;
            }
        }

        public static string ElasticSearchUrl
        {
            get { return elasticSearchUrl; }
            private set
            {
                elasticSearchUrl = value;
            }
        }

        public static string DropShareServerName
        {
            get { return dropShareServerName; }
            private set
            {
                dropShareServerName = value;
            }

        }

    }
}
