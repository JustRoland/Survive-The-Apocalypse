using UnityEngine;

namespace Task_Manager.Scripts
{
    public class CompleteTaskScript : MonoBehaviour
    {
        [SerializeField] TaskScriptableObject[] tasks;

        public void CompleteTask(int taskIndex)
        {
            GameManager.Instance.Task.CompleteTask(tasks[taskIndex].id);
        }
    }
}
