namespace FilterBuilderExample.Models
{
    public class ToyStorage
    {
        static string filterValue = String.Empty;
        public static string GetStoredFilter()
        {
            return filterValue;
        }

        public static void SetFilter(string filtervalue)
        {
            filterValue = filtervalue;
        }
    }
}
