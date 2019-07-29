using MWU.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MWU.Timeline
{

    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class TimelineId : MonoBehaviour
    {
        public static readonly Dictionary<string, GameObject> IdMap = new Dictionary<string, GameObject>();
        public static long LastUpdate { get; private set; }

        [SerializeField, ShowAsReadOnly]
        public string Id = Guid.NewGuid().ToString();

        void Awake()
        {
            Register();
        }

        void OnValidate()
        {
            Register();
        }

        void Register()
        {
            IdMap[Id] = this.gameObject;
            LastUpdate++;
        }

        void OnDestroy()
        {
            IdMap.Remove(Id);
            LastUpdate++;
        }
    }
}