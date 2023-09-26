using HorizonReports.DataDictionary;

namespace FilterBuilderExample.Models {

    static class SampleFilterData {
        public static Database HRDatabase = new Database("demodb");
        public static List<Table> Tables = new List<Table>()
            {
                new Table("Customers")
                {
                    Database = HRDatabase,
                    Caption = "Customers"
                },
                new Table("Orders")
                {
                    Database=HRDatabase,
                    Caption = "Orders"
                },
                new Table("Products")
                {
                    Database=HRDatabase,
                    Caption = "Products"
                }
            };
        public static List<Field> Fields { get; set; } = GetFields();

        public static List<Field> GetFields()
        {
            var customers = Tables.First(x => x.Name == "Customers");
            var orders = Tables.First(x => x.Name == "Orders");
            var products = Tables.First(x => x.Name == "Products");
            var fields = new List<Field>()
            {
                new Field("Customers.CustomerID")
                {
                    DataType = typeof(Int32),
                    Table = customers,
                    Caption = "Customer ID"
                }, 
                new Field("Customers.CustomerName")
                {
                    DataType = typeof(String),
                    Table = customers, 
                    Caption = "Name"
                }, 
                new Field("Orders.OrderID")
                {
                    DataType = typeof(Int32),
                    Table = orders,
                    Caption = "Order ID"
                },
                new Field("Orders.OrderDate")
                {
                    DataType = typeof(DateTime),
                    Table = orders,
                    Caption = "Order Date",
                    Format = "{0:d}"
                },
                new Field("Orders.OrderTotal")
                {
                    DataType = typeof(Decimal),
                    Table = orders,
                    Caption = "Order Total",
                    Format = "{0:c2}"
                },
                new Field("Products.ProductName")
                {
                    DataType = typeof(String),
                    Table = products,
                    Caption = "Name"
                },
                new Field("Products.Category")
                {
                    DataType = typeof(String),
                    Table = products,
                    Caption = "Category"
                }
            };
            return fields;
        }
    }
}
