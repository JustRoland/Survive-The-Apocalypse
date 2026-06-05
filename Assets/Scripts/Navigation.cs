using System;
using Task_Manager.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class Navigation : MonoBehaviour, IManager
{
    private Button _button;
    [SerializeField] private int sceneIndex; 

    private void LoadScene()
    {
        print($"Load requested to {SceneManager.GetSceneByBuildIndex(sceneIndex).name} ({sceneIndex})");
        if (TryGetComponent(out CompleteTaskScript task))
        {
            task.CompleteTask(0);
        }
        SceneManager.LoadScene(sceneIndex);
    }
    
    public void Initialize()
    {        
        print($"Initialize {name}");
        _button = GetComponent<Button>();
        _button.interactable = sceneIndex != SceneManager.GetActiveScene().buildIndex;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(LoadScene);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveAllListeners();
    }
}
