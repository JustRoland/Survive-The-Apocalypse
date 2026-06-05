using System.Collections.Generic;
using Task_Manager.Scripts;
using UnityEngine;

public class TasksScriptableObject : ScriptableObject
{
    private static int ID;
    public int id;
    public TaskList.TaskType taskType;
    [Header("Task List")]
    public List<string> tasks = new List<string>();

    TasksScriptableObject()
    {
        id = ID++;
    }

}
