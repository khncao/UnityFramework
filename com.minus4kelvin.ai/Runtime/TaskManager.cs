using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
/// <summary>
/// Manage tasks: track open and inprogress tasks; assign open tasks to valid processor
/// </summary>
public class TaskManager : Singleton<TaskManager>
{
    public System.Action onTaskAdded;

    List<Task> openTasks = new List<Task>();
    List<Task> busyTasks = new List<Task>();


    public Task TryGetTask(StateProcessor processor) {
        if(openTasks.Count > 0) {
            var pop = openTasks[openTasks.Count - 1];
            if(processor.TryChangeState(pop)) {
                openTasks.Remove(pop);
                busyTasks.Add(pop);
                return pop;
            }
        }
        return null;
    }

    public void AbortAllTasks() {
        foreach(var t in busyTasks) {
            t.processor.AbortState();
        }
    }

    public void AbortTask(Task task) {
        if(busyTasks.Contains(task)) {
            CleanupTask(task);
            openTasks.Add(task);
        }
    }

    public void CancelTask(Task task, bool unlist = true) {
        if(task == null) return;

        if(busyTasks.Contains(task)) {
            Debug.Log($"Canceling {task.description}");
            task.processor.AbortState();
            CleanupTask(task);
        }

        if(unlist) {
            int taskInd = openTasks.FindIndex(x=>x == task);
            if(taskInd != -1) {
                openTasks.RemoveAt(taskInd);
            }
        }
    }

    public void CompleteTask(Task task) {
        if(task != null && busyTasks.Contains(task)) {
            CleanupTask(task);
        }
    }

    void CleanupTask(Task task) {
        busyTasks.Remove(task);
        task.OnExit();
    }

    // Register tasks

    // Queue<System.Action> taskQueue = new Queue<System.Action>();
    // taskQueue.Enqueue(()=>RegisterDeliverItemsTask(deliverTarget, deliverInteract, items));

    public void RegisterTask(Task task) {
        openTasks.Add(task);
        onTaskAdded?.Invoke();
    }

    // TODO: self-contained GetItems and PutItems editor editable
    public Task RegisterDeliverItemsTask(Transform deliverTarget, IStateInteractable deliverInteract = null, List<ItemInstance> items = null) 
    {
        var inv = StateInteractableManager.I.GetClosestInteractableInventoryWithItems(items, deliverTarget);
        if(inv == null) {
            Debug.Log("No task inv");
            return null;
        }

        var task = new Task("deliver items task", 10);

        task.Enqueue(new Path(inv.transform));
        task.Enqueue(new GetItems(items, inv));
        task.Enqueue(new Path(deliverTarget));
        task.Enqueue(new PutItems(items, deliverInteract));

        RegisterTask(task);
        return task;
    }

    public Task RegisterActionTask(string triggerParam, int animLayer, Transform target = null, int p = 10, GameObject destroyTarget = null) 
    {
        var task = new Task($"{triggerParam} task", p);

        if(target) 
            task.Enqueue(new Path(target));

        task.Enqueue(new TriggerAnimation(animLayer, triggerParam));

        if(destroyTarget) 
            task.Enqueue(new Destroy(destroyTarget));

        RegisterTask(task);
        return task;
    }
}
}