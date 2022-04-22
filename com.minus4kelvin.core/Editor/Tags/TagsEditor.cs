
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace m4k {
[CustomPropertyDrawer(typeof(Tags))]
public class TagsEditor: PropertyDrawer {
    ReorderableList currentList;

    Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();

    TagsSO tagsSO;
    List<string> missingTags = new List<string>();
    bool helpBox = false;


    ReorderableList GetReorderableList(SerializedProperty property) 
    {
        if(!lists.TryGetValue(property.propertyPath, out var list)) 
        {
            list = new ReorderableList(property.serializedObject, property, true, false, false, false);

            list.drawElementCallback += ( rect, index, isActive, isFocused ) => {
                SerializedProperty elementProp = list.serializedProperty.GetArrayElementAtIndex( index );
                EditorGUI.LabelField(rect, elementProp.stringValue);
            };

            list.elementHeightCallback += idx => {
                if(idx >= property.arraySize) return EditorGUIUtility.singleLineHeight;
                SerializedProperty elementProp = property.GetArrayElementAtIndex( idx );
                return EditorGUI.GetPropertyHeight( elementProp );
            };

            lists.Add(property.propertyPath, list);
        }
        
        return list;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
    {
        var listProp = property.FindPropertyRelative("tags");
        currentList = GetReorderableList(listProp);
        var height = EditorGUIUtility.singleLineHeight;
        if(helpBox) {
            height += EditorGUIUtility.singleLineHeight * 4;
        }
        if(property.isExpanded) {
            height += currentList.GetHeight();
        }
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
    {
        EditorGUI.BeginProperty (position, label, property);

        if(!tagsSO)
            // tagsSO = TagsSO.GetAsset();
            tagsSO = TagsSO.I;

        var tags = property.GetValue() as Tags;
        missingTags.Clear();
        foreach(var i in tags.tags) {
            if(!tagsSO.ContainsTag(i)) {
                missingTags.Add(i);
            }
        }
        var helpRect = position;
        helpRect.height = EditorGUIUtility.singleLineHeight * 2;
        helpBox = false;

        if(missingTags.Count > 0) {
            var s = "Tags missing from TagsSO: ";
            foreach(var t in missingTags)
                s += $"'{t}' ";
            EditorGUI.HelpBox(helpRect, s, MessageType.Warning);
            Debug.LogWarning(s, property.serializedObject.targetObject);

            position.y += EditorGUIUtility.singleLineHeight * 2;
            position.height = EditorGUIUtility.singleLineHeight;
            if(GUI.Button(position, "Add missing tags to TagsSO")) {
                foreach(var t in missingTags)
                    tagsSO.AddTag(t);
            }
            position.y += EditorGUIUtility.singleLineHeight;
            if(GUI.Button(position, "Remove missing tags")) {
                foreach(var t in missingTags)
                    tags.RemoveTag(t);
            }
            position.y += EditorGUIUtility.singleLineHeight * 2;
            helpBox = true;
        }

        var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        var buttonRect = new Rect(position.x + position.width - 40, position.y, 40, EditorGUIUtility.singleLineHeight);
        if(GUI.Button(buttonRect, "Edit")) {
            var window = EditorWindow.GetWindow<TagsWindow>(utility:true);
            window.tags = tags;
            window.property = property;

            var dropDownRect = GUIUtility.GUIToScreenRect(buttonRect);
            window.ShowAsDropDown(dropDownRect, window.GetWindowSize());
        }
        
        if(property.isExpanded) {
            var listProp = property.FindPropertyRelative("tags");
            currentList = GetReorderableList(listProp);

            position.y += EditorGUIUtility.singleLineHeight;
            currentList.DoList(position);
        }

        property.serializedObject.ApplyModifiedProperties();

        EditorGUI.EndProperty ();
    }
}
}