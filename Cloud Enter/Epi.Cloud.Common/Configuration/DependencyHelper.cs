using System;
using System.Web.Mvc;

namespace Epi.Cloud.Common.Configuration
{
    public static class DependencyHelper
    {
        public static IDependencyResolver DependencyResolver { get { return System.Web.Mvc.DependencyResolver.Current; } }
        public static T GetService<T>()
        {
            return System.Web.Mvc.DependencyResolver.Current.GetService<T>();
        }
    }
}
