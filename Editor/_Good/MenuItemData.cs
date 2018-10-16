using System.Reflection;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    /// <summary>
    /// This data will be produced as result of reflection all project assemblies
    /// </summary>
    internal class MenuItemData
    {
        public MethodInfo TargetMethod;
        public MenuItem TargetAttribute;
        public MethodInfo TargetMethodValidate;
        public MenuItem TargetAttributeValidate;
    }
}