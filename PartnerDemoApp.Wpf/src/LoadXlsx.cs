using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Excel;

namespace PartnerDemoApp.Wpf.src
{
    public class LoadXlsx
    {
        private static string ResolveResourcePath(string relativePath)
        {
            return Path.Combine(AppContext.BaseDirectory, "res", relativePath);
        }

        public static List<T> LoadExcel<T>(string filename)
        {
            var filePath = ResolveResourcePath(filename);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл импорта не найден: {filename}", filePath);
            }

            using var csv = new CsvReader(new ExcelParser(filePath));
            return csv.GetRecords<T>().ToList();
        }

        public class PartnersImport
        {
            [Name("Тип партнера")]
            public required string PartnerType { get; set; }

            [Name("Наименование партнера")]
            public required string Name { get; set; }

            [Name("Директор")]
            public required string DirectorName { get; set; }

            [Name("Электронная почта партнера")]
            public required string Email { get; set; }

            [Name("Телефон партнера")]
            public required string PhoneNumber { get; set; }

            [Name("Юридический адрес партнера")]
            public required string Adress { get; set; }

            [Name("ИНН")]
            public required string Inn { get; set; }

            [Name("Рейтинг")]
            public int Rate { get; set; }
        }

        public class ProductsImport
        {
            [Name("Тип продукции")]
            public required string Type { get; set; }

            [Name("Наименование продукции")]
            public required string Name { get; set; }

            [Name("Артикул")]
            public required string Article { get; set; }

            [Name("Минимальная стоимость для партнера")]
            public double MinPrice { get; set; }
        }

        public class ProductTypeImport
        {
            [Name("Тип продукции")]
            public required string Type { get; set; }

            [Name("Коэффициент типа продукции")]
            public double Ratio { get; set; }
        }

        public class MaterialTypeImport
        {
            [Name("Тип материала")]
            public required string Type { get; set; }

            [Name("Процент брака материала ")]
            public double Ratio { get; set; }
        }

        public class PartnerProductsImport
        {
            [Name("Продукция")]
            public required string Product { get; set; }

            [Name("Наименование партнера")]
            public required string PartnerName { get; set; }

            [Name("Количество продукции")]
            public int Count { get; set; }

            [Name("Дата продажи")]
            public required string Date { get; set; }
        }
    }
}
