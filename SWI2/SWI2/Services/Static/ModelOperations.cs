using SWI2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Services.Static
{
    public static class ModelOperations
    {
        public static void CopyValues<T1, T2>(T1 toObject, T2 fromObject)
        {
            var T1Type = toObject.GetType();
            var T1Properties = T1Type.GetProperties();
            var T2Type = fromObject.GetType();
            var T2Properties = T2Type.GetProperties();
            foreach (var property in T1Properties)
            {
                var usedProperty = T2Properties.FirstOrDefault(x => x.Name == property.Name);
                if (usedProperty == default)
                    continue;
                if (usedProperty.Name == "Id")
                    continue;
                property.SetValue(toObject, usedProperty.GetValue(fromObject));
            }
        }
        public static void CopyValues<T1, T2>(T1 toObject, T2 fromObject,string[] ingnoredProperties)
        {
            var T1Type = toObject.GetType();
            var T1Properties = T1Type.GetProperties();
            var T2Type = fromObject.GetType();
            var T2Properties = T2Type.GetProperties();
            foreach (var property in T1Properties)
            {
                var usedProperty = T2Properties.FirstOrDefault(x => x.Name == property.Name);
                if (usedProperty == default)
                    continue;
                if (ingnoredProperties.Contains(usedProperty.Name))
                    continue;
                property.SetValue(toObject, usedProperty.GetValue(fromObject));
            }
        }
        public static IEnumerable<T> ExecuteQuery<T>(IEnumerable<T> collection, QueryViewModel query)
        {
            return collection.Skip((query.Page - 1) * query.ElementsPerPage + query.Offset).Take(query.ElementsPerPage);
        }
    }
}
