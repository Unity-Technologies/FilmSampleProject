using System;
using UnityEngine;

namespace MWU.Attributes
{

    [AttributeUsage(AttributeTargets.Field)]
    public class ShowAsReadOnlyAttribute : PropertyAttribute
    {
    }
}