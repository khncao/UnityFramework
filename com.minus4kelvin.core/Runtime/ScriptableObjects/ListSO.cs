
using System;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
public abstract class ListSO<T> : RuntimeScriptableObject {
    [SerializeField]
    private List<T> list;
    public Action<T> onAddItem, onRemoveItem;

    public List<T> GetList() {
        return list;
    }

    public override void OnDisable() {
        list.Clear();
        onAddItem = null;
        onRemoveItem = null;
    }

    public void Add(T item) {
        list.Add(item);
        onAddItem?.Invoke(item);
    }

    public bool Remove(T item) {
        bool success = list.Remove(item);
        if(success) {
            onRemoveItem?.Invoke(item);
        }
        return success;
    }

    public bool Contains(T item) {
        return list.Contains(item);
    }
}}