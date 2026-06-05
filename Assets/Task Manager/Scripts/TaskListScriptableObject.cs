using System.Collections.Generic;
using UnityEngine;

namespace Task_Manager.Scripts
{
    [CreateAssetMenu(menuName = "Task Manager/Task List")]
    public class TaskListScriptableObject : ScriptableObject
    {
        private static int ID;
        public readonly int id;

        public TaskList.TaskType type;

        public List<TaskScriptableObject> tasks = new();

        public TaskListScriptableObject()
        {
            id = ID++;
        }
    }
}
