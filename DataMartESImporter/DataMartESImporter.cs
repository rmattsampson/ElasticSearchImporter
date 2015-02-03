using Nest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DataMartESImporter
{
    /// <summary>
    /// Main engine for inserting a list of "records" into an ElasticSearch db 
    /// </summary>
    public class DataMartESImporter
    {
        //TODO: Maybe allow this to be toggled with a config setting
        private bool stopThisBusiness = false;
        private IList<string> filePathsToBeProcessed = new List<string>();
        private DateTime lastProcessedTimeRun;
        private DateTime lastProcessedTime
        {
            get
            {
                return this.lastProcessedTimeRun;
            }

            set
            {
                this.lastProcessedTimeRun = value;
            }
        }
        /// <summary>
        /// Initialize the DataMartESImporter
        /// </summary>
        public DataMartESImporter()
            : this(Log.GetLastRunTime())
        { }

        /// <summary>
        /// Initialize the DataMartESImporter
        /// </summary>
        public DataMartESImporter(DateTime lastProcessedTime)
        {
            this.lastProcessedTime = lastProcessedTime;
        }

        /// <summary>
        /// Start pulling data out of the files from the drop share and importing it into ElasticSearch
        /// </summary>
        public void StartSynchronization()
        {
            //Make sure you run this synchronously
            while (!stopThisBusiness)
            {
                Log runLog = Log.StartLog();
                StartDataImporterRunAsync(runLog);

                //Sleep
                System.Threading.Thread.Sleep(1000 * 60 * DataMartAppSettings.Default.SleepTimeInMinutes);
            }
        }

        private async Task StartDataImporterRunAsync(Log runLog)
        {
            try
            {
                runLog.LogMessage("Checking for new records to process");

                this.filePathsToBeProcessed = new List<string>();

                runLog.LogMessage("Finding all files in directory: " + ElasticSearchConnection.DropShare);

                //TODO: We should really sort files by DATE Ascending ideally so we process the older records first
                this.filePathsToBeProcessed = this.BuildUpListOfFilesToProcess(ElasticSearchConnection.DropShare, runLog);
                this.lastProcessedTime = DateTime.Now.ToUniversalTime(); //Mark our last processed time now

                foreach (string filePath in this.filePathsToBeProcessed)
                {
                    runLog.LogMessage("Generating list of records for file: " + filePath);

                    //Get all the Record objects from the specified file
                    //Records should formatted like the sample JSON file in solutino
                    IEnumerable<Record> recordsInAFile = Record.GenerateListOfRecordsFromJSONFile(filePath, runLog);
                    int recordsIndexedSuccessfully = 0;
                    int recordsNotIndexed = 0;
                    bool checkIndex = true;
                    runLog.LogMessage("Done generating records, preparing to index any records found in : " + filePath);

                    foreach (Record r in recordsInAFile)
                    {
                        //Only need to check to see if index exists the first time around
                        bool successfullyIndexed = await this.InsertAsync(r, filePath, runLog, checkIndex);
                        checkIndex = false;
                        if (successfullyIndexed)
                        {
                            recordsIndexedSuccessfully++;
                        }
                        else
                        {
                            recordsNotIndexed++;
                        }
                    }
                    runLog.LogMessage("Finished indexing " + recordsIndexedSuccessfully + " records in file: " + filePath);
                    if (recordsNotIndexed > 0)
                    {
                        runLog.LogMessage("Error - unable to index  " + recordsNotIndexed + " records in file: " + filePath, Log.MessageType.Error);
                    }
                }
                this.filePathsToBeProcessed.Clear();
                runLog.LogMessage("All done - Sleeping ");
            }

            finally
            {
                Log.LogLastRunTime(this.lastProcessedTime);
            }
        }

        /// <summary>
        /// Insert the record into the ElasticSearch Server now
        /// </summary>
        private async Task<bool> InsertAsync(Record recordToIndex, string filePath, Log runLog, bool checkIndex = true)
        {

            //If the index doesn't exist, we'll create it
            if (checkIndex)
            {
                if (!ElasticSearchConnection.ESClient.IndexExists(recordToIndex.indexName).Exists)
                {
                    var keywordA = new KeywordAnalyzer();
                    //We set the Default analyzer here to be "Keyword" type - so it will treat strings as "whole strings" and not as separate keywords
                    //very important for strings like "C#" or "Some long string"
                    ElasticSearchConnection.ESClient.CreateIndex(recordToIndex.indexName, x => x.Analysis(a => a.Analyzers(an => an.Add("default", keywordA))));
                }
            }

            Elasticsearch.Net.ElasticsearchResponse<Elasticsearch.Net.DynamicDictionary> response = null;
            JObject myObj = null;
            try
            {
                // See if we can parse the JSON and try to retrieve the _id field out of it
                myObj = JObject.Parse(recordToIndex.jsonString);
                if (myObj["_id"] != null)
                {
                    //If _id was specified then we'll just udpate the previous record automatically if there was one
                    string id = myObj["_id"].Value<String>();
                    response = await ElasticSearchConnection.ESClient.Raw.IndexAsync(recordToIndex.indexName, recordToIndex.typeName, id, recordToIndex.jsonString);
                    if (response.Success == false)
                    {
                        //The _id field of the JSON document is often problematic, so letting ES generate it's own ID field instead here might fix it
                        recordToIndex.jsonString = recordToIndex.jsonString.Replace("_id", "old_id");
                        // Try one more time to index now after renaming the _id field
                        response = await ElasticSearchConnection.ESClient.Raw.IndexAsync(recordToIndex.indexName, recordToIndex.typeName, recordToIndex.jsonString);
                        if (response.Success == true)
                        {
                            //renaming the _id field fixed the problem
                            runLog.LogMessage("Recovered from ERROR - But had to rename _id field to old_id to index record in " + filePath);
                        }
                    }
                }
                else
                {
                    // If they didn't specify an "_id" then that's fine just index it anyway
                    response = await ElasticSearchConnection.ESClient.Raw.IndexAsync(recordToIndex.indexName, recordToIndex.typeName, recordToIndex.jsonString);
                }
            }
            catch
            {
                //just couldn't parse the JSON, try to index it anyway
                response = ElasticSearchConnection.ESClient.Raw.Index(recordToIndex.indexName, recordToIndex.typeName, recordToIndex.jsonString);
            }

            if (response.Success == false)
            {
                //Failed to index a record
                runLog.LogMessage("Error: Failed to index a record in " + filePath, Log.MessageType.Error);
            }
            return response.Success;
        }

        /// <summary>
        /// Get a list of all JSON files in the drop share
        /// </summary>
        private IList<string> BuildUpListOfFilesToProcess(string path, Log runLog)
        {
            IList<string> listOfFilesToProcess = new List<string>();
            try
            {
                string[] allFiles = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
                foreach (string file in allFiles)
                {
                    FileSystemInfo fileDetail = new FileInfo(file);
                    DateTime lastWriteTime = fileDetail.LastWriteTime;
                    if (lastWriteTime.ToUniversalTime() > this.lastProcessedTime)
                    { listOfFilesToProcess.Add(file); }
                }
            }
            catch (Exception e)
            {
                runLog.LogMessage("Exception - caught when building up a list of files to process: " + e.Message, Log.MessageType.Error);
            }
            return listOfFilesToProcess;
        }
    }
}