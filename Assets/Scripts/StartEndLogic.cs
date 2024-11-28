using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartEndLogic : MonoBehaviour
{
    [SerializeField] private GameObject simulationGO;

    private bool gameStarted = false;

    private void Start()
    {
        simulationGO.SetActive(false);
    }

    private void Update()
    {
        if (!gameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            gameStarted = true;
            StartSimualtion();
        }

        if (Input.anyKey && !Input.GetKeyDown(KeyCode.Space) 
                         || !Input.GetKeyDown(KeyCode.W) 
                         || !Input.GetKeyDown(KeyCode.A) 
                         || !Input.GetKeyDown(KeyCode.S) 
                         || !Input.GetKeyDown(KeyCode.D))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameStarted = false;
            simulationGO.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    public void StartSimualtion() 
    {
        simulationGO.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void QuitGame() 
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
