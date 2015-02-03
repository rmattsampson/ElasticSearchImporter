using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMartESImporter
{
    /// <summary>
    /// A very simple "record" for Elastic Search - an index name, a document type, and the JSON blob  
    /// </summary>
    public class Record
    {
        public string jsonString;
        public string indexName;
        public string typeName;

        private Record(string jsonString, string indexName, string typeName)
        {
            this.jsonString = jsonString;
            this.indexName = indexName.ToLower();
            this.typeName = typeName.ToLower();
        }

        /// <summary>
        /// Read 1 or more JSON records out of a specified JSON file
        /// </summary>
        private static IEnumerable<TResult> ReadJson<TResult>(string pathToJSONFile)
        {
            var serializer = new JsonSerializer();
            using (FileStream fstream = new FileStream(pathToJSONFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader strmReader = new StreamReader(fstream))
                {
                    using (var jsonReader = new JsonTextReader(strmReader))
                    {
                        //We can read MULTIPLE records here in the JSON, but "," delimiteres are not allowed, 
                        //Only things like " ", and CRLF
                        jsonReader.SupportMultipleContent = true;
                        while (jsonReader.Read())
                        {
                            yield return serializer.Deserialize<TResult>(jsonReader);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parse each JSON File and pull out the record into a "Record" object
        /// If file path is \\dropshare\Github\MydataFile.json then:
        ///     indexName = github
        ///     DocumentType/CollectionName = mydatafile
        /// </summary>
        public static IEnumerable<Record> GenerateListOfRecordsFromJSONFile(string pathToJSONFile, Log runLog)
        {
            List<Record> allRecords = new List<Record>();
            FileInfo currentFileInfo = new FileInfo(pathToJSONFile);
            try
            {
                string indexName = pathToJSONFile.Substring(ElasticSearchConnection.DropShare.Length, pathToJSONFile.IndexOf('\\', ElasticSearchConnection.DropShare.Length) - ElasticSearchConnection.DropShare.Length);

                //Remove all extensions from the file name
                string collectionName = pathToJSONFile;
                while (collectionName != Path.GetFileNameWithoutExtension(collectionName))
                {
                    collectionName = Path.GetFileNameWithoutExtension(collectionName);
                }
            
                IEnumerable<JObject> allJSONObjects;
                //Read the JSON out of the file
                allJSONObjects = Record.ReadJson<JObject>(pathToJSONFile);

                foreach (JObject record in allJSONObjects)
                {
                    Record currentRecord = new Record(record.ToString(), indexName, collectionName);
                    allRecords.Add(currentRecord);
                }
            }
            catch (IOException e)
            {
                runLog.LogMessage("Exception - file was probably locked " + pathToJSONFile + " Exception message: " + e.Message, Log.MessageType.Error);
            }
            catch (Exception e)
            {
                runLog.LogMessage("Exception - your file has some strange issues " + pathToJSONFile + " Exception message: " + e.Message, Log.MessageType.Error);
            }

            return allRecords;
        }
    }
}
