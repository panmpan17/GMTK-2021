using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject currentStep;
    public GameObject[] steps;

    public void ShowTutorial(int step)
    {
        currentStep.SetActive(false);
        currentStep = steps[step];
        currentStep.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
