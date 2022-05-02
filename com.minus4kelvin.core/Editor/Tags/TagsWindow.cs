
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace m4k {
/// <summary>
/// Both a contextual popup for editing TagsEditor properties and a window for modifying collection of tags
/// </summary>
public class TagsWindow : EditorWindow {
    const float WindowHorizontalLabelWidths = 2;
    const int MaxWindowVerticalLines = 15;

    public Tags tags;
    public SerializedProperty property;

    Vector2 scrollPos;
    TagsSO tagsSO;
    ReorderableList reorderableList;
    string searchString;
    string addTagString = "";
    

    [MenuItem("Tools/Tag Editor")]
    private static void ShowWindow() {
        var window = GetWindow<TagsWindow>();
        window.titleContent = new GUIContent("Tags Window");
        window.Show();
    }

    private void Awake() {
        // tagsSO = TagsSO.GetAsset();
        tagsSO = TagsSO.I;
    }

    public Vector2 GetWindowSize() {
        var size = new Vector2(EditorGUIUtility.labelWidth * WindowHorizontalLabelWidths, MaxWindowVerticalLines * EditorGUIUtility.singleLineHeight);
        return size;
    }

    ReorderableList GetReorderableList() {
        var list = new ReorderableList(tagsSO.tags, typeof(string), true, false, false, true);

        list.drawElementCallback += ( rect, index, isActive, isFocused ) => {
            if(tags != null) {
                var toggleRect = new Rect(rect.x, rect.y, 20, rect.height);
                bool contains = tags.ContainsTag(tagsSO.tags[index]);
                bool toggled = contains;

                toggled = GUI.Toggle(toggleRect, toggled, "");

                if(!string.IsNullOrEmpty(tagsSO.tags[index])) {
                    ProcessToggle(toggled, contains, index);
                }
                rect.x += 20;
            }
            
            // tagsSO.tags[index] = GUI.TextField(rect, tagsSO.tags[index]);
            GUI.Label(rect, tagsSO.tags[index]);
        };

        return list;
    }

    void ProcessToggle(bool toggled, bool contains, int index) {
        if(toggled && !contains) {
            tags.AddTag(tagsSO.tags[index]);
            EditorUtility.SetDirty(tagsSO);
            if(property != null) 
                property.serializedObject.RepaintInspector();
        }
        else if(!toggled && contains) {
            tags.RemoveTag(tagsSO.tags[index]);
            EditorUtility.SetDirty(tagsSO);
            if(property != null) 
                property.serializedObject.RepaintInspector();
        }
    }

    private void OnGUI() {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        searchString = GUILayout.TextField(searchString);
        if(GUILayout.Button("Clear", GUILayout.Width(40))) {
            searchString = "";
        }
        GUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if(!string.IsNullOrEmpty(searchString) && tagsSO) {
            GUILayout.BeginHorizontal();
            for(int index = 0; index < tagsSO.tags.Count; ++index) {
                if(!tagsSO.tags[index].Contains(searchString))
                    continue;
                if(tags != null) {
                    bool contains = tags.ContainsTag(tagsSO.tags[index]);
                    bool toggled = contains;

                    toggled = GUILayout.Toggle(toggled, "", GUILayout.Width(20));

                    if(!string.IsNullOrEmpty(tagsSO.tags[index])) {
                        ProcessToggle(toggled, contains, index);
                    }
                }
                GUILayout.Label(tagsSO.tags[index]);
            }
            GUILayout.EndHorizontal();
        }
        else {
            if(reorderableList == null) {
                reorderableList = GetReorderableList();
            }
            reorderableList.DoLayoutList();
        }

        EditorGUILayout.EndScrollView();
        
        if(GUILayout.Button("Add tag")) {
            tagsSO.AddTag(addTagString);
        }
        addTagString = GUILayout.TextField(addTagString);
        
#if UNITY_2021_1_OR_NEWER
        EditorGUILayout.Separator();
        if(GUILayout.Button("Remove selected tag(s)")) {
            foreach(var i in reorderableList.selectedIndices) {
                tagsSO.RemoveTag(tagsSO.tags[i]);
            }
        }
#endif
        SaveChanges();
    }

    private void OnDestroy() {
        tags = null;
        property = null;
    }
}}