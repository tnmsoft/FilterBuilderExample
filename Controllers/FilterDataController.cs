using FilterBuilderExample.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using Newtonsoft.Json.Linq;
using HorizonReports.Filtering;
using HorizonReports.SQLServices;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Data;

namespace FilterBuilderExample.Controllers {

    public class FilterDataController : Controller {

        [HttpGet]
        public object FilterFields() {
            return Json(FilterField.GetFromHRFieldList(SampleFilterData.Fields));
        }

        [HttpGet]
        public object FilterTables(DataSourceLoadOptions loadOptions)
        {
            return DataSourceLoader.Load(SampleFilterData.Tables.Select(x => new { x.Name, x.Caption }), loadOptions);
        }

        [HttpPost]
        public object GetSQL(string filter)
        {
            var filters = JArray.Parse(filter);
            var hrfilters = DXFilterToHRFilter.Convert(filters);
            var filterpredicate = hrfilters.ToPredicate();
            SQLOutputOptions options = new()
            {
                UseDatabasePrefix = false,
                UseSchemaPrefix = false,
                UseTablePrefix = true,
                ReplaceVariableReferenceWithValue = true,
                VariableReferenceValues = new Queue<object>()
            };
            foreach (IFilterCondition filtercondition in hrfilters)
            {
                foreach (var value in filtercondition.Values)
                {
                    options.VariableReferenceValues.Enqueue(value.Value);
                }
            }
            GenerateSQLStatementVisitor generatevisitor = new(options);
            filterpredicate.Accept(generatevisitor);
            return "WHERE " + generatevisitor.SQLText.ToString();
        }

        [HttpGet]
        public object LoadFilter()
        {
            return ToyStorage.GetStoredFilter();
        }

        [HttpPost]
        public object StoreFilter(string filter)
        {
            ToyStorage.SetFilter(filter);
            return filter;
        }

    }
    public static class DXFilterToHRFilter
    {
        public static List<IFilterCondition> Convert(IList filter)
        {
            if (filter == null || filter.Count == 0) return new List<IFilterCondition>();
            IList normalizedFilter = NormalizeFilter(filter);
            var filterConverted = ConvertFilter(normalizedFilter);
            if (filterConverted.First().OpenParentheses > 0)
            {
                filterConverted.First().OpenParentheses--;
                filterConverted.Last().CloseParentheses--;
            }
            return filterConverted;
        }

        const string NEGATION_OPERATOR = "!";
        static string[] logicalOperators = new string[] { "and", "or" };


        static IFilterCondition ParseFilterExpression(IList objects)
        {
            string field = objects[0].ToString();
            string op = objects[1].ToString().Trim();
            object value = objects[2];
            return BuildFilter(field, op, value);
        }
        static IFilterCondition BuildFilter(string field, string op, object value)
        {
            var datadictionaryfield = SampleFilterData.Fields.First(x => x.Name == field);
            IFilterCondition filter = new FilterCondition(datadictionaryfield);
            filter.Operator = GetHorizonReportsOperator(op);
            filter.Not = GetHorizonReportsNotValue(op);
            filter.Values.Add(value);
            return filter;
        }

        static IList NormalizeFilter(IList filter)
        {
            List<object> normalizedFilter = new List<object>(filter.Count);
            foreach (object element in filter)
            {
                normalizedFilter.Add(element);
            }
            return normalizedFilter;
        }
        static object NormalizeElement(object element)
        {
            object normalizedObject = element;
            if (element is JArray)
            {
                normalizedObject = ((JArray)element).ToList<object>();
                List<object> list = (List<object>)normalizedObject;
                if (list.Count == 1 && list[0] is JArray)
                {
                    normalizedObject = NormalizeElement(((List<object>)normalizedObject)[0]);
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i] = NormalizeElement(list[i]);
                    }
                }
            }
            if (element is JValue) normalizedObject = ((JValue)element).Value;
            return normalizedObject;
        }
        static List<IFilterCondition> ConvertFilter(IList filter)
        {
            List<IFilterCondition> filters = new List<IFilterCondition>();
            List<object> filterParts = new List<object>(filter.Count);
            foreach (object element in filter)
            {
                object elementToAdd = NormalizeElement(element);
                filterParts.Add(elementToAdd);
            }
            if (filterParts.Count == 1) filterParts = (List<object>)filterParts[0];
            var convertedfilters = ConvertCore(filterParts);
            filters.AddRange(convertedfilters);
            return filters;
        }
        static List<IFilterCondition> ConvertCore(List<object> filter)
        {
            List<IFilterCondition> filters = new List<IFilterCondition>();
            if (filter[0] is string)
            {
                if (filter[0].ToString() == NEGATION_OPERATOR)
                {
                    filters.AddRange(ConvertFilter((IList)filter[1]));
                }
                else
                {
                    filters.Add(ParseFilterExpression(filter));
                    return filters;
                }
            }
            if (filter[1] is string && logicalOperators.Contains(filter[1].ToString()))
            {
                var currentconnection = GetHorizonReportsConnection(filter[1].ToString());
                foreach (object filterPart in filter)
                {
                    if (!logicalOperators.Contains(filterPart.ToString()))
                    {
                        var converted = ConvertFilter((IList)filterPart);
                        //converted[0].OpenParentheses++;
                        converted[0].Connection = currentconnection;
                        filters.AddRange(converted);
                    }
                }
                if (filters.Count > 1)
                {
                    filters.First().OpenParentheses++;
                    filters.Last().CloseParentheses++;
                }
                filters[0].Connection = null;
                return filters;
            }
            return filters;
        }
        static Operator GetHorizonReportsOperator(string func)
        {
            switch (func)
            {
                case "contains": return new OperatorContains();
                case "startswith": return new OperatorBegins();
                case "endswith": return new OperatorEnds();
                case "notcontains": return new OperatorContains();
                case "=": return new OperatorEquals();
                case ">": return new OperatorGreater();
                case "<": return new OperatorLess();
                case ">=": return new OperatorGreaterEqual();
                case "<=": return new OperatorLessEqual();
                case "<>": return new OperatorEquals();
                default: return new OperatorEquals();
            }
        }

        static bool GetHorizonReportsNotValue(string func)
        {
            switch (func)
            {
                case "contains": return false;
                case "startswith": return false;
                case "endswith": return false;
                case "notcontains": return true;
                case "=": return false;
                case ">": return false;
                case "<": return false;
                case ">=": return false;
                case "<=": return false;
                case "<>": return true;
                default: return false;
            }
        }

        static FilterConnection GetHorizonReportsConnection(string connection)
        {
            switch (connection)
            {
                case "and": return new AndConnection(); ;
                case "or": return new OrConnection();
                default: return new AndConnection();
            }
        }
    }
}