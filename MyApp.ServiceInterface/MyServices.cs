
using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;
using ServiceStack.Script;
using ServiceStack.DataAnnotations;
using MyApp.ServiceModel;
using ServiceStack.Configuration;


namespace MyApp.ServiceInterface
{
    public class MyServices : Service
    {
        public object Any(Hello request)
        {
            return new HelloResponse { Result = $"Hello, {request.Name}!" };
        }

        [Authenticate]
        public object Any(RequiresAuth request)
        {
            return new RequiresAuthResponse { Result = $"Hello, {request.Name}!" };
        }

        [RequiredRole("Manager")]
        public object Any(RequiresRole request)
        {
            return new RequiresRoleResponse { Result = $"Hello, {request.Name}!" };
        }

        [RequiredRole(nameof(RoleNames.Admin))]
        public object Any(RequiresAdmin request)
        {
            return new RequiresAdminResponse { Result = $"Hello, {request.Name}!" };
        }

        public object Get(Hello request)
        {
            var response = new HelloResponse
            {

                Fields = new FieldDictionary
             {
        { "Key1",  new TProperty<int> { Value = 10 }},
        { "Key2",  new TProperty<int> { Value = 20 }}
              }

            };

            return response;
        }



        [Serializable]
        public class TProperty<T> : ITProperty where T : struct
        {
            public T? Value
            {
                get;
                set;
            }
            public static implicit operator TProperty<T>(T val)
            {
                return new TProperty<T>() { Value = val };
            }
        }

    }
}