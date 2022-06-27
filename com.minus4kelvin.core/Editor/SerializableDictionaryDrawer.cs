/// <summary>
/// From Unity Wiki
/// </summary>

using System;
using System.Reflection;

using UnityEngine;

using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(SerializableDictionary), true)]
public class SerializableDictionaryDrawer : PropertyDrawer {

    private ReorderableList list;

    private Func<Rect> VisibleRect;

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        if (list == null) {
            var listProp = property.FindPropertyRelative("list");
            list = new ReorderableList(property.serializedObject, listProp, true, false, true, true);
            list.drawElementCallback = DrawListItems;
            list.elementHeightCallback = ListItemHeight;
        }

        var firstLine = position;
        firstLine.height = EditorGUIUtility.singleLineHeight;
        label.text += $"[{list.count.ToString()}]";
        EditorGUI.PropertyField(firstLine, property, label);

        if (property.isExpanded) {
            position.y += firstLine.height;

            if (VisibleRect == null) {
                 var tyGUIClip = System.Type.GetType("UnityEngine.GUIClip,UnityEngine");
                 if (tyGUIClip != null) {
                    var piVisibleRect = tyGUIClip.GetProperty("visibleRect", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (piVisibleRect != null) {

                        var getMethod = piVisibleRect.GetGetMethod(true) ?? piVisibleRect.GetGetMethod(false);
                        VisibleRect = (Func<Rect>)Delegate.CreateDelegate(typeof(Func<Rect>), getMethod);
                    }
                 }
            }

            var vRect = VisibleRect();
            vRect.y -= position.y;

            if (elementIndex == null)
                elementIndex = new GUIContent();

            list.DoList(position, vRect);
        }
    }

    private static GUIContent[] pairElementLabels => s_pairElementLabels ?? (s_pairElementLabels = new[] {new GUIContent("Key"), new GUIContent ("=>")});
    private static GUIContent[] s_pairElementLabels;

    private static GUIContent elementIndex;

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused) {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index); // The element in the list

        var keyProp   = element.FindPropertyRelative("Key");
        var valueProp = element.FindPropertyRelative("Value");

        elementIndex.text = $"{index}";
        /*var label =*/ EditorGUI.BeginProperty(rect, elementIndex, element);

        // auto expand draw if objects or lists
        if(keyProp.isExpanded || valueProp.isExpanded) {
            rect = DrawListProperty(rect, index, isActive, isFocused, keyProp);

            // janky workaround for key container objects
            if(keyProp.isExpanded) 
                rect.y -= EditorGUIUtility.singleLineHeight;
            rect.y += EditorGUIUtility.singleLineHeight;
            
            rect = DrawListProperty(rect, index, isActive, isFocused, valueProp);
        }
        else { // single-line, spaced key & value if possible
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 75;

            var rect0 = rect; 

            var halfWidth = rect0.width / 2f;
            rect0.width = halfWidth;
            rect0.y += 1f;
            rect0.height -= 2f;

            EditorGUIUtility.labelWidth = 40;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect0, keyProp);
            
            rect0.x += halfWidth + 4f;

            EditorGUI.PropertyField(rect0, valueProp);
            EditorGUIUtility.labelWidth = prevLabelWidth;
            
        }

        EditorGUI.EndProperty();
    }
    GUIContent emptyLabel = new GUIContent("");

    Rect DrawListProperty(Rect rect, int index, bool isActive, bool isFocused, SerializedProperty property) {
        EditorGUI.PropertyField(rect, property, includeChildren:true);
        if(property.isExpanded)
            rect.y += EditorGUI.GetPropertyHeight(property, true);
        return rect;
    }

    float ListItemHeight(int index) {
        var height = EditorGUIUtility.singleLineHeight;
        if(index >= list.serializedProperty.arraySize)
            return height;

        SerializedProperty elementProp = list.serializedProperty.GetArrayElementAtIndex(index);

        var keyProp   = elementProp.FindPropertyRelative("Key");
        if(keyProp.isExpanded) 
            height += EditorGUI.GetPropertyHeight(keyProp, true);

        var valueProp = elementProp.FindPropertyRelative("Value");
        if(valueProp.isExpanded) 
            height += EditorGUI.GetPropertyHeight(valueProp, true);

        return height;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (property.isExpanded && list != null) {
            var listProp = property.FindPropertyRelative("list");

            return EditorGUIUtility.singleLineHeight + list.GetHeight();
            // if (listProp.arraySize < 2)
            //     return EditorGUIUtility.singleLineHeight + 52f;
            // else
            //     return EditorGUIUtility.singleLineHeight + 23f * listProp.arraySize + 29;
        }
        else
            return EditorGUIUtility.singleLineHeight;
    }
}

