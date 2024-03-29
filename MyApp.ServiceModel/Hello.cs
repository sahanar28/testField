
using Microsoft.AspNetCore.Http;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.Text.Jsv;
using ServiceStack.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text.Encodings.Web;
namespace MyApp.ServiceModel
{

    public static class AppHostConfig
    {
        static AppHostConfig()
        {
            JsConfig.ExcludeTypeInfo = false;
            JsConfig.AssumeUtc = true;
            JsConfig.DateHandler = DateHandler.ISO8601;
            JsConfig.ThrowOnError = true;
            JsConfig<TStringProperty>.SerializeFn = (obj) =>
            {
                return obj.Value;
            };
            JsConfig.TypeWriter = (tt) =>
            {
                return string.Format("{0}.{1}", tt.Namespace, tt.Name);
            };
            JsConfig<FieldDictionary>.RawDeserializeFn = (val) => { return ToDictionary<FieldDictionary, ITProperty>(val); };
            JsConfig<FieldDictionary>.RawDeserializeFn = r =>
            {
                var fields = new FieldDictionary();
                JsonObject jobject = jsvJsonParse(r);

                foreach (string key in jobject.Keys)
                {

                    string json = jobject.Child(key);
                    JsonObject jjobject = jsvJsonParse(json);
                    try
                    {
                        json = jjobject.Child("Value");
                    }
                    catch { }
                }
                return fields;
            };

            JsConfig<bool>.DeSerializeFn = r =>
            {
                bool boolVal = false;
                if (Boolean.TryParse(r, out boolVal))
                {
                    return boolVal;
                }
                else
                {
                    return false;
                }

            };
            JsConfig<PropertyOrFieldDef>.DeSerializeFn = ToPropertyOrFieldDef;
        }

        public static void Initialize()
        {
        }

        private static T ToDictionary<T, P>(string json)
        where T : System.Collections.IDictionary
        {
            T dictionary = Activator.CreateInstance<T>();
            Type baseType = typeof(T).BaseType;

            Type[] generics = baseType.GetGenericArguments();

            JsonObject jObject = JsonObject.Parse(json);

            foreach (string key in jObject.Keys)
            {
                object dicKey = null;
                if (generics[0].IsEnum)
                {
                    if (System.Enum.GetNames(generics[0]).Contains(key))
                    {
                        dicKey = System.Enum.Parse(generics[0], key, true);
                    }
                }
                else
                {
                    dicKey = key;
                }

                if (dicKey != null)
                {

                    var methodInfo = getTPropertyMethodInfo<P>(jObject, key);

                    if (methodInfo != null)
                    {

                        dictionary.Add(dicKey, methodInfo.Invoke(jObject, new object[] { jObject, key, null }));

                    }
                    else
                    {
                        dictionary.Add(dicKey, jObject.Get<P>(key));
                    }

                }

            }
            return dictionary;
        }

        private static MethodInfo getTPropertyMethodInfo<P>(JsonObject jObject, string key)
        {
            if (typeof(P) == typeof(ITProperty))
            {
                var strVal = jObject.Get(key);
                var typeName = strVal.Split(new string[] { "[[", "]]" }, StringSplitOptions.None);

                if (typeName.Length > 1)
                {
                    Type type = Type.GetType($"HP.T.ServiceModel.TProperty`1[[{typeName[1]}]], TSModel");
                    MethodInfo method = typeof(JsonExtensions)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == nameof(JsonExtensions.Get)).FirstOrDefault(x => x.IsGenericMethod);

                    return method.MakeGenericMethod(type);
                }

            }

            return null;
        }

        public class TStringProperty : ITProperty
        {
            public string Value
            {
                get;
                set;
            }
        }

        private static bool isJsonNotJsv(string json)
        {
            foreach (char c in json)
            {
                if (c == '"')
                {
                    return true;
                }

                if (c == ':')
                {
                    return false;
                }
            }
            return false;
        }

        private static JsonObject jsvJsonParse(string json)
        {
            if (isJsonNotJsv(json))
            {
                return JsonObject.Parse(json);
            }
            else if (json.StartsWith(@"{"))
            {
                return JsvReader.GetParseFn(typeof(JsonObject))(json) as JsonObject;
            }
            return null;
        }

        [DataContract]
        [SDKObjectAttribute]

        public partial class PropertyOrFieldDef
        {
            [DataMember]
            public string Id { get; set; }

            [DataMember]
            public Boolean IsEMailAddress { get; set; }

            [DataMember]
            public Boolean IsHyperlink { get; set; }

            [DataMember]
            public int DefaultColumnCharacters { get; set; }

            [DataMember]
            public int ColumnWidth { get; set; }

            [DataMember]
            public Boolean IsDefaultDataGridColumn { get; set; }

            [DataMember]
            public Boolean BestFitColumn { get; set; }

            [DataMember]
            public string Caption { get; set; }

            [DataMember]
            public Boolean IsAField { get; set; }

            [DataMember]
            public Boolean IsAProperty { get; set; }

            [DataMember]
            public Boolean IsMandatory { get; set; }

            [DataMember]
            public Boolean IsSuitableForAggregation { get; set; }

        }

        private static string ValueFromeJsonOrJsv(string r, string key = "Value")
        {
            JsonObject jobject = jsvJsonParse(r);
            if (jobject != null)
            {
                return jobject[key];
            }
            return null;
        }

        public static PropertyOrFieldDef ToPropertyOrFieldDef(string r)
        {
            if (r != null)
            {
                if (r.StartsWith("{"))
                {
                    string _columnWidth = ValueFromeJsonOrJsv(r, "ColumnWidth");
                    if (string.IsNullOrWhiteSpace(_columnWidth))
                    {
                        return new PropertyOrFieldDef() { Id = ValueFromeJsonOrJsv(r, "Id") };
                    }
                    else
                    {
                        return new PropertyOrFieldDef() { Id = ValueFromeJsonOrJsv(r, "Id"), ColumnWidth = Int32.Parse(_columnWidth) };
                    }
                }
            }
            return new PropertyOrFieldDef() { Id = r.Trim() };
        }

    }

    [DataContract]
    [Route("/hello")]
    [Route("/hello/{Name}")]
    public class Hello : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    [CollectionDataContract(Name = "Field", ItemName = "Field")]
    [SDKObjectAttribute]
    public partial class FieldDictionary : Dictionary<string, ITProperty>
    {
    }



    [DataContract]

    public abstract class ITProperty
    {
        [DataMember]
        public string StringValue { get; set; }
    }

    [DataContract]
    public class HelloResponse
    {
        public string Result { get; set; }

        [DataMember]
        [ApiMember(ExcludeInSchema = true, DataType = "object", Verb = "NONE")]
        public FieldDictionary Fields { get; set; }

    }

    [Route("/requiresauth")]
    [Route("/requiresauth/{Name}")]
    public class RequiresAuth : IReturn<RequiresAuthResponse>
    {
        public string Name { get; set; }
    }

    public class RequiresAuthResponse
    {
        public string Result { get; set; }
    }

    [Route("/requiresrole")]
    [Route("/requiresrole/{Name}")]
    public class RequiresRole : IReturn<RequiresRoleResponse>
    {
        public string Name { get; set; }
    }

    public class RequiresRoleResponse
    {
        public string Result { get; set; }
    }

    [Route("/requiresadmin")]
    [Route("/requiresadmin/{Name}")]
    public class RequiresAdmin : IReturn<RequiresAdminResponse>
    {
        public string Name { get; set; }
    }

    public class RequiresAdminResponse
    {
        public string Result { get; set; }
    }

}