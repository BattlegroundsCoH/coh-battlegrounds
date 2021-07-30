using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Battlegrounds.Json {

    public static class JsonRecord {

        public static bool IsRecordType(object type, out PropertyInfo equalityContractProperty) {
            equalityContractProperty = type.GetType().GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic);
            if (equalityContractProperty is null) {
                return false;
            }
            if (equalityContractProperty.GetValue(type) as Type == type.GetType()) {
                return true;
            } else {
                return false;
            }
        }

        public static bool IsRecordType(Type type) => IsRecordType(type, out _);

        public static bool IsValidRecordType(object type, out bool isConsideredRecord, out PropertyInfo equalityContractProperty) {
            isConsideredRecord = IsRecordType(type, out equalityContractProperty);
            if (isConsideredRecord) {
                if (type.GetType().GetConstructors().Any(x => x.GetParameters().Length == 0)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        public static IEnumerable<PropertyInfo> Derecord(object record, IEnumerable<PropertyInfo> properties) {
            if (IsValidRecordType(record, out bool isRecord, out PropertyInfo property)) {
                return properties.Except(new List<PropertyInfo>() { property });
            } else {
                if (isRecord) {
                    throw new JsonTypeException(record.GetType(), $"Invalid record type. No parameterless constructor found.");
                }
                return properties;
            }
        }

    }

}
