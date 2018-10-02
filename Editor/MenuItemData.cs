using System.Reflection;
using UnityEditor;

namespace SKTools.MenuItemsFinder
{
    internal class MenuItemData
    {
        public MethodInfo TargetMethod;
        public MenuItem TargetAttribute;
        public MethodInfo TargetMethodValidate;
        public MenuItem TargetAttributeValidate;
    }
}