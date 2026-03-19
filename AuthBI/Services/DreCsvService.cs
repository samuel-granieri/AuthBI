using AuthBI.Models.Domain;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using AuthBI.Models.DTOs;

namespace AuthBI.Services
{
    public class DreCsvService
    {
        private readonly List<DreCsvModel> _dados;

        public DreCsvService(IWebHostEnvironment env)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            };

            var path = Path.Combine(env.WebRootPath, "data", "dre1.csv");

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);

            _dados = csv.GetRecords<DreCsvModel>().ToList();
        }

        public List<DreCsvModel> GetDados()
        {
            return _dados;
        }

        
    }
}
