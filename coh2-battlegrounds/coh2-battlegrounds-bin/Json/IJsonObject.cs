using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Battlegrounds.Util;
using Battlegrounds.Functional;

namespace Battlegrounds.Json {
    
    /// <summary>
    /// Json interface for converting object to and from a Json object. Implements <see cref="IJsonElement"/>.
    /// </summary>
    public interface IJsonObject : IJsonElement {

        private static readonly IFormatProvider JSStandardFormat = new CultureInfo("en-GB");

        public static IFormatProvider JsonStandardFormatter => JSStandardFormat;

        private struct JsonAttributeSet {
            private JsonIgnoreIfValueAttribute IgnoreIfValueAttribute;
            private JsonIgnoreIfEmptyAttribute IgnoreIfEmptyAttribute;
            private JsonIgnoreIfNullAttribute IgnoreIfNullAttribute;
            private JsonIgnoreAttribute IgnoreAttribute;
            private JsonReferenceAttribute ReferenceAttribute;
            private JsonBackingFieldAttribute BackingAttribute;

            public bool IgnoreIfNull => this.IgnoreIfNullAttribute is JsonIgnoreIfNullAttribute;

            public bool IgnoreAlways => this.IgnoreAttribute is JsonIgnoreAttribute;

            public bool UseRef => this.ReferenceAttribute is JsonReferenceAttribute;

            public bool IgnoreIfValue => this.IgnoreIfValueAttribute is JsonIgnoreIfValueAttribute;

            public object IgnoreValue => this.IgnoreIfValueAttribute.Value;

            public bool IgnoreIfEmpty => this.IgnoreIfEmptyAttribute is JsonIgnoreIfEmptyAttribute;

            public bool HasBackingField => this.BackingAttribute is JsonBackingFieldAttribute;

            public string BackingFieldName => this.BackingAttribute.Field;

            public JsonAttributeSet(PropertyInfo pinfo) {
                this.IgnoreIfEmptyAttribute = pinfo.GetCustomAttribute<JsonIgnoreIfEmptyAttribute>();
                this.IgnoreIfValueAttribute = pinfo.GetCustomAttribute<JsonIgnoreIfValueAttribute>();
                this.IgnoreIfNullAttribute = pinfo.GetCustomAttribute<JsonIgnoreIfNullAttribute>();
                this.IgnoreAttribute = pinfo.GetCustomAttribute<JsonIgnoreAttribute>();
                this.ReferenceAttribute = pinfo.GetCustomAttribute<JsonReferenceAttribute>();
                this.BackingAttribute = pinfo.GetCustomAttribute<JsonBackingFieldAttribute>();
            }
            public JsonAttributeSet(FieldInfo finfo) {
                this.IgnoreIfEmptyAttribute = finfo.GetCustomAttribute<JsonIgnoreIfEmptyAttribute>();
                this.IgnoreIfValueAttribute = finfo.GetCustomAttribute<JsonIgnoreIfValueAttribute>();
                this.IgnoreIfNullAttribute = finfo.GetCustomAttribute<JsonIgnoreIfNullAttribute>();
                this.IgnoreAttribute = finfo.GetCustomAttribute<JsonIgnoreAttribute>();
                this.ReferenceAttribute = finfo.GetCustomAttribute<JsonReferenceAttribute>();
                this.BackingAttribute = finfo.GetCustomAttribute<JsonBackingFieldAttribute>();

            }
        }

        /// <summary>
        /// Get instance as a json reference string.
        /// </summary>
        /// <returns>Instance in json reference string form.</returns>
        public string ToJsonReference();

        /// <summary>
        /// Serialize self into a json object
        /// </summary>
        /// <returns>The json string representation of the <see cref="IJsonObject"/>.</returns>
        public virtual string Serialize()
            => this.Serialize(0);

        /// <summary>
        /// Serialize self into a json object
        /// </summary>
        /// <returns>The json string representation of the <see cref="IJsonObject"/>.</returns>
        public virtual string Serialize(int indent)
            => SerializeObject(indent, this);

        public static string SerializeObject(int indent, object obj) {

            Type il_type = obj.GetType();
            TxtBuilder jsonbuilder = new TxtBuilder();
            jsonbuilder.SetIndent(indent);

            // Get all the fields
            IEnumerable<FieldInfo> fields = il_type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() is null && !x.Name.EndsWith("_BackingField"));

            // Get all the properties
            IEnumerable<PropertyInfo> properties = il_type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() is null);

            // Derecord the object
            properties = JsonRecord.Derecord(obj, properties);

            // Begin building
            jsonbuilder.AppendLine("{");
            jsonbuilder.IncreaseIndent();

            jsonbuilder.AppendLine($"\"jsdbtype\": \"{il_type.FullName}\"{((fields.Count() + properties.Count() > 0) ? "," : "")}");

            foreach (PropertyInfo pinfo in properties) {
                var attribSet = new JsonAttributeSet(pinfo);
                if (attribSet.HasBackingField) {
                    FieldInfo finfo = il_type.GetField(attribSet.BackingFieldName, BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic);
                    WriteKeyValuePair(jsonbuilder, finfo.FieldType, attribSet, pinfo.Name, finfo.GetValue(obj), pinfo != properties.Last() || fields.Any());
                } else {
                    WriteKeyValuePair(
                        jsonbuilder,
                        pinfo.PropertyType,
                        attribSet,
                        pinfo.Name,
                        pinfo.GetValue(obj),
                        pinfo != properties.Last() || fields.Any()
                        );
                }
            }

            foreach (FieldInfo finfo in fields) {
                WriteKeyValuePair(
                    jsonbuilder,
                    finfo.FieldType,
                    new JsonAttributeSet(finfo),
                    finfo.Name,
                    finfo.GetValue(obj),
                    finfo != fields.Last()
                    );
            }

            if (jsonbuilder.EndsWith(",", true)) {
                int lComma = jsonbuilder.LastIndexOf(',');
                jsonbuilder.Popback(jsonbuilder.Length - lComma);
                jsonbuilder.Append("\n", false);
            }

            jsonbuilder.DecreaseIndent();
            jsonbuilder.AppendLine("}");

            return jsonbuilder.GetContent();


        }

        private static void WriteKeyValuePair(TxtBuilder jsonbuilder, Type type, JsonAttributeSet attributeSet, string name, object val, bool appendComma) {
            if (val is null && attributeSet.IgnoreIfNull) {
                return;
            }
            if (attributeSet.IgnoreIfValue && val.Equals(attributeSet.IgnoreValue)) {
                return;
            }
            if (attributeSet.IgnoreIfEmpty) {
                try {
                    dynamic col = val;
                    if (col.Count == 0) {
                        return;
                    }
                } catch (Exception e) {
                    Trace.WriteLine(e);
                }
            }
            if (type.IsPrimitive || val is string) {
                jsonbuilder.AppendLine($"\"{name}\": \"{val}\"{((appendComma)?",":"")}");
            } else {
                if (val is IJsonObject jso) {
                    if (attributeSet.UseRef) {
                        jsonbuilder.AppendLine($"\"{name}\": \"{jso.ToJsonReference()}\"{((appendComma) ? "," : "")}");
                    } else {
                        jsonbuilder.AppendLine($"\"{name}\": {jso.Serialize(jsonbuilder.GetIndent()).Trim('\t', '\n')}{((appendComma) ? "," : "")}");
                    }
                } else if (type.GenericTypeArguments.Length == 2 && type == typeof(Dictionary<,>).MakeGenericType(type.GenericTypeArguments)) {
                    jsonbuilder.AppendLine($"\"{name}\": {{");
                    jsonbuilder.IncreaseIndent();
                    dynamic dic = val; // BAD
                    int i = 0;
                    int j = dic.Count - 1;
                    foreach (dynamic kvp in dic) {
                        Type t = kvp.Value.GetType();
                        string entryName = kvp.Key.ToString();
                        if (t.IsPrimitive || kvp.Value is string) {
                            jsonbuilder.Append($"\"{entryName}\" : \"{(kvp.Value as string).Replace("\\", "\\\\")}\"{((i < j) ? ",\n" : "")}");
                        } else if (kvp.Value is IJsonObject jso2) {
                            if (attributeSet.UseRef) {
                                jsonbuilder.Append($"\"{entryName}\" : \"{jso2.ToJsonReference()}\"{((i < j) ? ",\n" : "")}");
                            } else {
                                jsonbuilder.Append($"\"{entryName}\" : {jso2.Serialize(jsonbuilder.GetIndent()).TrimEnd('\n')}{((i < j) ? ",\n" : "")}", false);
                            }
                        }
                        i++;
                    }
                    if (j >= 0) {
                        jsonbuilder.Append("\n", false);
                    }
                    jsonbuilder.DecreaseIndent();
                    jsonbuilder.AppendLine($"}}{((appendComma) ? "," : "")}");
                } else if (type.GenericTypeArguments.Length > 0 && type.GetInterfaces().Contains(typeof(IEnumerable<>).MakeGenericType(type.GenericTypeArguments))) {
                    jsonbuilder.AppendLine($"\"{name}\": [");
                    jsonbuilder.IncreaseIndent();
                    dynamic dynVal = val; // CAREFUL!
                    int i = 0;
                    int j = dynVal.Count - 1;
                    foreach (dynamic c in dynVal) {
                        if (c is null) {
                            continue;
                        }
                        Type t = c.GetType();
                        if (t.IsPrimitive || c is string) {
                            jsonbuilder.Append($"\"{name}\"{((i < j) ? "," : "")}");
                        } else if (c is IJsonObject jso2) {
                            if (attributeSet.UseRef) {
                                jsonbuilder.Append($"\"{jso2.ToJsonReference()}\"{((i < j) ? "," : "")}");
                            } else {
                                jsonbuilder.Append($"{jso2.Serialize(jsonbuilder.GetIndent()).TrimEnd('\n')}{((i < j) ? ",\n" : "")}", false);
                            }
                        }
                        i++;
                    }
                    if (j >= 0) {
                        jsonbuilder.Append("\n", false);
                    }
                    jsonbuilder.DecreaseIndent();
                    jsonbuilder.AppendLine($"]{((appendComma) ? "," : "")}");
                } else {
                    jsonbuilder.AppendLine($"\"{name}\": \"{val}\"{((appendComma) ? "," : "")}");
                }

            }
        }

        /// <summary>
        /// Derserialize a json string representing an object into the C# requivalent.
        /// </summary>
        /// <param name="parsedJson">The parsed input of a json object</param>
        /// <returns>A deserialized instance of the json input string</returns>
        internal static object Deserialize(Dictionary<string, object> parsedJson) {

            // Solve type
            if (!parsedJson.TryGetValue("jsdbtype", out object type)) {
                type = typeof(JsonKeyValueSet).AssemblyQualifiedName;
            } else {
                parsedJson.Remove("jsdbtype");
            }

            // Make sure type is valid
            if (type is null) {
                throw new NullReferenceException("The given 'jsdbtype' was null!");
            }

            // Get the type
            Type il_type = Type.GetType(type as string);

            // Create object
            object source = Activator.CreateInstance(il_type);

            if (source is JsonKeyValueSet keyValues) {

                // Easily handle the dictionary style in a different method
                HandleKeyValueSet(keyValues, parsedJson);

            } else {

                // Get methods
                MethodInfo[] methods = il_type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                //methods.ForEach(x => (x.GetCustomAttribute<>)) // TODO: Preinitialize

                // Get all the fields
                FieldInfo[] fields = il_type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                // Get all the properties
                PropertyInfo[] properties = il_type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                // Set the value (fields expected!)
                foreach (var pair in parsedJson) {

                    // Define the reference attribute (it may be used)
                    JsonReferenceAttribute refAttrib;
                    JsonEnumAttribute enumAttrib;

                    if (fields.FirstOrDefault(x => x.Name == pair.Key) is FieldInfo finfo) { // Is Field?

                        // Get the reference attribute
                        refAttrib = finfo.GetCustomAttribute<JsonReferenceAttribute>();
                        enumAttrib = finfo.GetCustomAttribute<JsonEnumAttribute>();

                        // Use the reference method
                        bool useRef = !(refAttrib is null);

                        // Set the field value.
                        SetValue(source, pair.Value, finfo.FieldType, useRef, finfo.SetValue, refAttrib?.GetReferenceFunction(), enumAttrib);

                    } else if (properties.FirstOrDefault(x => x.Name == pair.Key) is PropertyInfo pinfo) { // Is Property?

                        // Get the reference attribute
                        refAttrib = pinfo.GetCustomAttribute<JsonReferenceAttribute>();
                        enumAttrib = pinfo.GetCustomAttribute<JsonEnumAttribute>();

                        // Use the reference method
                        bool useRef = !(refAttrib is null);

                        // If it has a backing field with specific name
                        if (pinfo.GetCustomAttribute<JsonBackingFieldAttribute>() is JsonBackingFieldAttribute backAttrib) {

                            // Get the backing field
                            FieldInfo backingField = il_type.GetField(backAttrib.Field, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                            // Set the value of the backing field.
                            SetValue(source, pair.Value, backingField.FieldType, useRef, backingField.SetValue, refAttrib?.GetReferenceFunction(), enumAttrib);

                        } else {

                            // Do we have a set method?
                            if (pinfo.SetMethod != null) {

                                // Set value using the 'SetMethod'
                                SetValue(source, pair.Value, pinfo.PropertyType, useRef, pinfo.SetValue, refAttrib?.GetReferenceFunction(), enumAttrib);

                            } else {

                                // Get the backing field
                                FieldInfo backingField = il_type.GetField($"<{pinfo.Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                                // Do if any backing field (otherwise it might just be a get-only, precalculated variable.
                                if (backingField is not null) {

                                    // Set the value of the backing field.
                                    SetValue(source, pair.Value, backingField.FieldType, useRef, backingField.SetValue, refAttrib?.GetReferenceFunction(), enumAttrib);

                                }

                            }

                        }

                    }

                }

                // Invoke all the OnDeserialized methods
                methods.ForEach(x => x.IfTrue(y => y.GetCustomAttribute<JsonOnDeserializedAttribute>() is JsonOnDeserializedAttribute).Then(y => y.Invoke(source, null)));

            }

            // Return the object we've deserialized
            return source;

        }

        internal static void SetValue(object instance, object value, Type valueType, bool byReference, 
            Action<object, object> setValueMethod, Func<string, object> derefMethood, JsonEnumAttribute eAttrib) {

            // Is it an array?
            if (value is JsonArray jsa) {

                // Create enumerable and populate
                object enumerableType = Activator.CreateInstance(valueType);
                jsa.Populate(enumerableType, valueType.GenericTypeArguments[0], derefMethood);

                // Set the value using the enumerable
                setValueMethod.Invoke(instance, enumerableType);

            } else if (value is JsonKeyValueSet jkvs) {

                // Create and populate key-value set
                object valueset = Activator.CreateInstance(valueType);
                jkvs.Populate(valueset, valueType.GenericTypeArguments[0], valueType.GenericTypeArguments[1], derefMethood);

                // Set the value
                setValueMethod.Invoke(instance, valueset);

            } else {

                // By reference?
                if (byReference) {

                    // Get the value
                    object val = derefMethood.Invoke(value as string);

                    // Set value
                    setValueMethod.Invoke(instance, val);

                } else { // Ordinary set-value

                    // If the attribute is not valid
                    if (eAttrib is null) {

                        // Set value
                        setValueMethod.Invoke(instance, Convert.ChangeType(value, valueType, JsonStandardFormatter));

                    } else {

                        if (eAttrib.IsNetEnum) {

                            if (Enum.TryParse(eAttrib.EnumType, value as string, out object enumValue)) {
                                setValueMethod.Invoke(instance, enumValue);
                            } else {
                                throw new InvalidCastException();
                            }

                        } else {

                            throw new NotImplementedException();

                        }

                    }

                }

            }


        }

        private static void HandleKeyValueSet(JsonKeyValueSet pairs, Dictionary<string, object> keyValues) {

            // Loop through
            foreach (var pair in keyValues) {

                // Get simplisticly
                IJsonElement keyStr = new JsonValue(pair.Key);
                IJsonElement valueObj = pair.Value as IJsonElement;

                if (valueObj is null && pair.Value is string s) {
                    valueObj = new JsonValue(s);
                }

                // Add
                pairs.Add(keyStr, valueObj);

            }

        }


    }

}
