using UnityEngine;
using System;
using System.Collections.Generic;

namespace MWU.Shared.Base
{
    /// <summary>
    /// Parent Monobehaviour that we inherit from, allows us to do any global magic we require here
    /// 
    /// Inherit from this class, NOT the default Monobehaviour class
    /// </summary>
    public class BaseMonoBehaviour : MonoBehaviour
    {
        // override as necessary
        protected virtual void OnEnable( )
        {
            useGUILayout = false;	// disable Unity GUI
        }

        public Action<BaseMonoBehaviour> onDestroyed;

        protected virtual void OnDestroy()
        {
            if (onDestroyed != null) { onDestroyed(this); }
        }

        /// <summary>
        /// Avoid GetComponent where possible.
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <returns></returns>
        public I GetInterfaceComponent<I>( ) where I : class
        {
            return GetComponent(typeof(I)) as I;
        }

        /// <summary>
        /// Retrieve a list of components of a specific type
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <returns></returns>
        public static List<I> FindObjectsOfInterface<I>( ) where I : class
        {
            MonoBehaviour[] monoBehaviours = FindObjectsOfType<MonoBehaviour>();
            List<I> list = new List<I>();

            foreach ( MonoBehaviour behaviour in monoBehaviours )
            {
                I component = behaviour.GetComponent(typeof(I)) as I;

                if ( component != null )
                {
                    list.Add(component);
                }
            }

            return list;
        }
    }
}
