using UnityEngine;

namespace Task_Manager.Scripts
{
    [CreateAssetMenu(menuName = "Task Manager/Task")]
    public class TaskScriptableObject : ScriptableObject
    {
        private static int ID;
        public readonly int id;

        public string taskDescription;
        public bool checkWhileInactive;

        public TaskScriptableObject()
        {
            id = ID++;
        }

    }
}
