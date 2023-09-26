using HorizonReports.DataDictionary;

namespace FilterBuilderExample.Models
{
    public class FilterField
    {
        public string caption { get; set; }
        public string dataField { get; set; }
        public DxDataTypes.DxDataType dataType { get; set; }
        public string format { get; set; }
        public LookupField lookup { get; set; }

        public static List<FilterField> GetFromHRFieldList(List<Field> hrfields)
        {
            var filterfields = hrfields.Select(x => new FilterField
            {
                caption = x.Caption,
                dataField = x.Name,
                dataType = DxDataTypes.FromHRDataType(x.DataType),
                format = DxFormats.FromHRFormat(x.Format)
            }).ToList();

            // This is a hardcoded example of how to populate a pick list for a field
            var productcategory = filterfields.First(x => x.dataField == "Products.Category");
            productcategory.lookup = new LookupField() { dataSource = new List<string> { "Produce", "Meat", "Dairy", "Bakery" } };

            return filterfields;
        }
    }
    public class LookupField
    {
        public List<string> dataSource { get; set; } = new List<string>();
    }

    public static class DxDataTypes
    {
        public enum DxDataType
        {
            @string,
            number,
            date,
            boolean,
            @object,
            datetime
        }
        public static DxDataType FromHRDataType(Type dataType)
        {
            return dataType.ToString() switch
            {
                "System.String" => DxDataType.@string,
                "System.DateTime" => DxDataType.datetime,
                "System.Decimal" => DxDataType.number,
                "System.Double" => DxDataType.number,
                "System.Int16" => DxDataType.number,
                "System.Int32" => DxDataType.number,
                "System.Int64" => DxDataType.number,
                "System.Boolean" => DxDataType.boolean,
                _ => DxDataType.@string
            };
        }
    }

    public static class DxFormats
    {
        public enum DxFormat
        {
            fixedPoint,
            @decimal,
            percent,
            currency
        }

        public static string FromHRFormat(string format)
        {
            return format switch
            {
                "{0:c2}" => DxFormat.currency.ToString(),
                _ => String.Empty
            }; 
        }
    }

}
