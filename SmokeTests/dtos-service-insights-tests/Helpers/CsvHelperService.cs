using System.Collections.Generic;
using System.Globalization;
using System.IO;
using AllOverIt.Extensions;
using CsvHelper;
using CsvHelper.Configuration;


namespace Ddtos_service_insights_tests.Helpers;

public static class CsvHelperService
{
        public static List<string> ExtractNhsNumbersFromCsv(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            var records = new List<string>();
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                records.Add(csv.GetField(3));
            }
            return records;
        }

        public static List<Dictionary<string, string>> ReadLastRecordOfCsv(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            int numberOfRecords = File.ReadAllLines(filePath).Length;
            var records = new List<Dictionary<string, string>>();
            csv.Read();
            csv.ReadHeader();

            var headers = csv.HeaderRecord;
            int rowCount=0;
            while (csv.Read())
            {
                if(rowCount==numberOfRecords-2){
                var record = new Dictionary<string, string>();
                foreach (var header in headers)
                {
                    record[header] = csv.GetField(header);
                }

                records.Add(record);
                }
                rowCount++;
            }
            return records;
        }

        public static List<Dictionary<string, string>> ReadCsv(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            int numberOfRecords = File.ReadAllLines(filePath).Length;
            var records = new List<Dictionary<string, string>>();
            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord;
            int rowCount=0;
            while (csv.Read())
            {
                var record = new Dictionary<string, string>();
                foreach (var header in headers)
                {
                    record[header] = csv.GetField(header);
                }
                records.Add(record);
                rowCount++;
            }
            return records;
        }

}
