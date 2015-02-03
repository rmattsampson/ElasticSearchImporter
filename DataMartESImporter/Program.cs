using System;

namespace DataMartESImporter
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            //DateTime dateTimeNow = DateTime.Now;
            //DateTime startTime = dateTimeNow.Subtract(new TimeSpan(4, 0, 0));
            DataMartESImporter importer = new DataMartESImporter();

            importer.StartSynchronization();
        }
    }
}
