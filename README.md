# ElasticSearchImporter
Service to auto-import JSON files into a specified ElasticSearch node. Will automatically create indexes and document types as needed in ElasticSearch

To Run:
-Compile the Solution to generate the exe
-Specify the configuration settings (in App.config)
-Run the exe and will immediately start "polling" the specified drop share (recursively) for any new JSON files. Any files that it finds are automatically indexed into the ElasticSearch server specified.
  -Run the exe as a Windows Task if you want it to automatically start    http://windows.microsoft.com/en-us/windows/schedule-task#1TC=windows-7
  
