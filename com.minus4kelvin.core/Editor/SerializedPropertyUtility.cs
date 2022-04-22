// https://github.com/garettbass/UnityExtensions.InspectInline

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace m4k
{

    public static class SerializedPropertyUtility
    {

        public static IEnumerable<SerializedProperty>
        EnumerateChildProperties(this SerializedObject serializedObject)
        {
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(enterChildren: true))
            {
                // yield return property; // skip "m_Script"
                while (iterator.NextVisible(enterChildren: false))
                {
                    yield return iterator;
                }
            }
        }

        public static IEnumerable<SerializedProperty>
        EnumerateChildProperties(this SerializedProperty parentProperty)
        {
            var iterator = parentProperty.Copy();
            var end = iterator.GetEndProperty();
            if (iterator.NextVisible(enterChildren: true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, end))
                        yield break;

                    yield return iterator;
                }
                while (iterator.NextVisible(enterChildren: false));
            }
        }

        // https://answers.unity.com/questions/505697/how-to-repaint-from-a-property-drawer.html

        /// <summary>
        /// Repaint editors from PropertyDrawer
        /// </summary>
        /// <param name="BaseObject"></param>
        public static void RepaintInspector(this SerializedObject BaseObject, bool setDirty = true)
        {
            foreach(var item in ActiveEditorTracker.sharedTracker.activeEditors)
                if(item.serializedObject == BaseObject) { 
                    if(setDirty)
                        EditorUtility.SetDirty(BaseObject.targetObject);
                    item.Repaint(); 
                    return; 
                }
        }

        // https://answers.unity.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html

        /// <summary>
        /// Get instance property from SerializedProperty in CustomPropertyDrawer
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetValue(this SerializedProperty prop)
        {
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            string[] elements = path.Split('.');

            foreach (string element in elements.Take(elements.Length))
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            return obj;
        }

        static object GetValue(object source, string name)
        {
            if(source == null)
                return null;
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if(f == null)
            {
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(p == null)
                    return null;
                return p.GetValue(source, null);
            }
            return f.GetValue(source);
        }

        static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while(index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }

    }

}