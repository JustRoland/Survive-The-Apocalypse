using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Task_Manager.Scripts
{
    public class TaskManager : MonoBehaviour
    {

        [Header("Canvas Setup")]
        [SerializeField] private GameObject canvasTasksCollectionGameObject;
        [SerializeField] private GameObject taskListDisplayPrefab;
        [SerializeField] private GameObject taskDisplayPrefab;
        [SerializeField] private Color taskNormalColor;
        [SerializeField] private Color taskFadedColor;


        [Header("Task Collection")]
        [SerializeField] private bool printToConsole = false;
        [SerializeField] private List<TaskListScriptableObject> allTaskListScriptableObjects;
        [SerializeField] private TaskListScriptableObject[] activateTaskListOnStart;
        [SerializeField] private List<TaskList> _activeTaskLists = new();

        [Header("Events")]
        public UnityEvent OnTaskCompleted = new UnityEvent();
        public UnityEvent OnTaskListCompleted = new UnityEvent();
        public UnityEvent OnTaskListStarted = new UnityEvent();

        private List<TaskList> _allTaskLists = new();
        

        private void Start()
        {
            allTaskListScriptableObjects.ForEach(AddTaskList);
            foreach (var task in activateTaskListOnStart)
            {
                if (!allTaskListScriptableObjects.Contains(task)) AddTaskList(task);
                ActivateTaskList(task.id);
            }
        }

        public void AddTaskList(TaskListScriptableObject taskList)
        {
            //check all added task lists' ID against the new-to-be-added task lists' ID and skip it if the ID already exists.
            foreach (var alreadyAddedTaskList in _allTaskLists)
            {
                if (alreadyAddedTaskList.id != taskList.id) continue;

                GameManager.Instance.Message.NewMessage("Duplicate ID found. Make sure to have a unique ID for all task lists.");
                throw new Exception("Duplicate ID found. Make sure to have a unique ID for all task lists.");
            }

            _allTaskLists.Add(new(taskList));
        }

        public void CompleteTask(TaskScriptableObject task)
        {
            CompleteTask(task.id);
        }
        public void CompleteTask(int taskID)
        {

            Task task = GetTask(taskID);

            Debug.Assert(task != null, "Task not found.");
        
            if (task.complete) return;

            TaskList list = task.checkWhileInactive ? _allTaskLists.First(list => list.id == task.listid) : _activeTaskLists.First(list => list.id == task.listid);
        
            if (list.taskType == TaskList.TaskType.sequential)
            {
                //check if the task is the next one in line
                if (list.tasks[list.activeTaskIndex] != task)
                {
                    if (printToConsole) print($"<color=yellow> Task {task.name} (id: {task.id}) is not next in line. </color>");
                    return;
                }
                list.activeTaskIndex++;

            }

            task.complete = true;
            OnTaskCompleted.Invoke();

            //if there are still unfinished tasks, return.
            if (list.tasks.Any(task => task.complete == false))
            {
                UpdateTaskListVisuals();
                return;
            }

            //...else, complete list
            GameManager.Instance.Message.NewMessage($"Quest '{list.name}' is finished.");
            list.finished = true;
            OnTaskListCompleted.Invoke();
            UpdateTaskListVisuals();

        }

        Task GetTask(int taskID)
        {
            return _allTaskLists.SelectMany(taskList => taskList.tasks).FirstOrDefault(task => task.id == taskID);
        }


        public bool CheckIfTaskFinished(int taskID)
        {
            Task task = GetTask(taskID);

            return task.complete;
        }

        public void ActivateTaskList(TaskListScriptableObject taskList)
        {
            ActivateTaskList(taskList.id);
        }

        public void ActivateTaskList(int taskListID)
        {
            GameManager.Instance.Message.NewMessage("Starting new quest ...");
            
            foreach (TaskList taskList in _allTaskLists)
            {
                if (taskList.id != taskListID) continue;

                if (taskList.started) return;

                _activeTaskLists.Add(taskList);

                taskList.started = true;
                
                GameManager.Instance.Message.NewMessage($"Quest '{taskList.name}' is started.");

                CreateTaskVisualsFromPrefab(taskList);

                UpdateTaskListVisuals();

                OnTaskListStarted.Invoke();

                return;
            }
            
            GameManager.Instance.Message.NewMessage("No quest found.");

        }


        private void CreateTaskVisualsFromPrefab(TaskList taskList)
        {
            taskList.taskListCanvasObject = Instantiate(taskListDisplayPrefab, canvasTasksCollectionGameObject.transform);

            taskList.taskListCanvasObject.GetComponentInChildren<TaskListTitleObject>(true).GetComponent<TextMeshProUGUI>().text = taskList.name;

            if (taskList.taskType == TaskList.TaskType.sequential)
            {
                taskList.taskCanvasObjects = new GameObject[3];

                for (int i = 0; i < 3; i++)
                {
                    taskList.taskCanvasObjects[i] = Instantiate(taskDisplayPrefab, taskList.taskListCanvasObject.transform);
                }
            }
            else
            {
                taskList.taskCanvasObjects = new GameObject[taskList.tasks.Count];

                for (int i = 0; i < taskList.taskCanvasObjects.Length; i++)
                {
                    taskList.taskCanvasObjects[i] = Instantiate(taskDisplayPrefab, taskList.taskListCanvasObject.transform);
                }
            }

            taskList.taskCanvasObjects.ToList().ForEach(taskObj => taskObj.GetComponentInChildren<TaskCompleteVisual>(true).gameObject.SetActive(false));

        }


        private void UpdateTaskListVisuals()
        {
            foreach (var taskList in _allTaskLists)
            {
                //check all inactive ones if they have a visual created and destroy it if they do.
                if (!taskList.started || taskList.finished)
                {
                    if (!taskList.taskListCanvasObject) continue;

                    foreach (var obj in taskList.taskCanvasObjects)
                    {
                        Destroy(obj);
                    }

                    Destroy(taskList.taskListCanvasObject);

                    continue;
                }


                //check all active ones if they have a visual created and create new ones if they dont.
                if (taskList.started && !taskList.finished && !taskList.taskListCanvasObject)
                {
                    CreateTaskVisualsFromPrefab(taskList);
                }


                //Update the visuals
                if (taskList.taskType == TaskList.TaskType.sequential)
                {
                    var temp0 = taskList.taskCanvasObjects[1];
                    var temp1 = temp0.GetComponentInChildren<TaskDescriptionObject>(true);
                    var temp2 = temp1.GetComponent<TextMeshProUGUI>();
                    temp2.text = taskList.tasks[taskList.activeTaskIndex].description;
                    temp2.color = taskNormalColor;

                    if (taskList.activeTaskIndex - 1 >= 0)
                    {
                        temp0 = taskList.taskCanvasObjects[0];
                        temp1 = temp0.GetComponentInChildren<TaskDescriptionObject>(true);
                        temp2 = temp1.GetComponent<TextMeshProUGUI>();
                        temp2.text = taskList.tasks[taskList.activeTaskIndex - 1].description;
                        temp2.color = taskFadedColor;
                    }

                    if (taskList.activeTaskIndex + 1 < taskList.tasks.Count)
                    {
                        temp0 = taskList.taskCanvasObjects[2];
                        temp1 = temp0.GetComponentInChildren<TaskDescriptionObject>(true);
                        temp2 = temp1.GetComponent<TextMeshProUGUI>();
                        temp2.text = taskList.tasks[taskList.activeTaskIndex + 1].description;
                        temp2.color = taskFadedColor;
                    }
                }

                if (taskList.taskType == TaskList.TaskType.inconsecutive)
                {
                    for (int i = 0; i < taskList.taskCanvasObjects.Length; i++)
                    {
                        taskList.taskCanvasObjects[i].GetComponentInChildren<TaskDescriptionObject>(true).GetComponent<TextMeshProUGUI>().text = taskList.tasks[i].description;
                        if (taskList.tasks[i].complete) taskList.taskCanvasObjects[i].GetComponentInChildren<TaskCompleteVisual>(true).gameObject.SetActive(true);
                    }
                }


            }



        }

    }

    [Serializable]
    public class TaskList
    {
        public int id;
        public string name;
        public TaskType taskType;
        public bool started = false;
        public bool finished = false;
        [Tooltip("Determines which task comes next based on its index.")]
        public int activeTaskIndex = 0;

        [Header("Task List")]
        public List<Task> tasks = new();
        public enum TaskType { sequential, inconsecutive }

        public GameObject taskListCanvasObject;
        public GameObject[] taskCanvasObjects;

        public TaskList(TaskListScriptableObject taskList)
        {
            id = taskList.id;
            name = taskList.name;
            taskType = taskList.type;
            tasks.AddRange(taskList.tasks.Select(t => new Task(t.id, id, t.name, t.taskDescription, t.checkWhileInactive)));
        }


    }

    [Serializable]
    public class Task
    {
        public readonly int id;
        public readonly int listid;
        public readonly string name;
        public readonly string description;
        public readonly bool checkWhileInactive;
        public bool complete = false;

        public Task(int id, int listid, string name, string description, bool checkWhileInactive)
        {
            this.id = id;
            this.listid = listid;
            this.name = name;
            this.description = description;
            this.checkWhileInactive = checkWhileInactive;
        }

    }
}