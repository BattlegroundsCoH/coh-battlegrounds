using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Battlegrounds.Json {
    
    public static class JsonRecord {
    
        public static bool IsRecordType(Type type, out PropertyInfo equalityContractProperty) {
            equalityContractProperty = type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.Public);
            if (equalityContractProperty is null) {
                return false;
            }
            if (equalityContractProperty.PropertyType == type) {
                return true;
            } else {
                return false;
            }
        }

        public static bool IsRecordType(Type type) => IsRecordType(type, out _);

        public static bool IsValidRecordType(Type type, out bool isConsideredRecord, out PropertyInfo equalityContractProperty) {
            isConsideredRecord = IsRecordType(type, out equalityContractProperty);
            if (isConsideredRecord) {
                if (type.GetConstructors().Any(x => x.GetParameters().Length == 0)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        public static IEnumerable<PropertyInfo> Derecord(Type recordType, IEnumerable<PropertyInfo> properties) {
            if (IsValidRecordType(recordType, out bool isRecord, out PropertyInfo property)) {
                return properties.Except(new List<PropertyInfo>() { property });
            } else {
                if (isRecord) {
                    throw new JsonTypeException(recordType, $"Invalid record type. No parameterless constructor found.");
                }
                return properties;
            }
        }

    }

}
