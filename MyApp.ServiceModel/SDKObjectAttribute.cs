using ServiceStack;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace MyApp.ServiceModel
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SDKObjectAttribute : Attribute
    {
    }
}



