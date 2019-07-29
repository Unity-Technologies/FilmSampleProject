using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MWU.Shared.Base
{
    /// <summary>
    ///     Our primary service location pattern. All global services are registered with us
    /// </summary>
    public class ServiceLocator
    {
        private static readonly List<object> services = new List<object>();

        public static List<object> Services {  get { return services; } }

        public static T Get<T>()
        {
            var matching = services.OfType<T>().ToList();
            if (matching.Count < 1)
            {
                Debug.Log("Failed to get service of type: " + typeof(T).FullName);
                return default(T);
            }
            else
            {
                return matching[0];
            }
        }

        public static bool Has<T>()
        {
            var matching = services.OfType<T>().ToList();
            return matching.Count > 0;
        }

        public static T Add<T>(T service) where T : MonoBehaviour
        {
            //Object.DontDestroyOnLoad(service.gameObject);
            services.Add(service);
            return service;
        }

        public static T AddRaw<T>(T service)
        {
            services.Add(service);
            return service;
        }

        /// <summary>
        /// In case we need to remove any services
        /// </summary>
        public static bool Remove<T>(T service) where T : MonoBehaviour
        {
            if (services.Contains(service))
            {
                services.Remove(service);
                return true;
            }
            return false;
        }

        public static bool RemoveRaw<T>(T service)
        {
            if( services.Contains( service))
            {
                services.Remove(service);
                return true;
            }
            return false;
        }

    }
}