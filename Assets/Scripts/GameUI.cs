using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    public GameObject gameLoseScreen;
    public GameObject gameWinScreen;
    bool gameIsOver;


    // Start is called before the first frame update
    void Start()
    {
        GuardScript.OnGuardHasSpottedPlayer += ShowGameLoseUI;
        PlayerController.collectedAllCrystals += ShowGameWinUI;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameIsOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    void ShowGameWinUI()
    {
        OnGameOver(gameWinScreen);
    }

    void ShowGameLoseUI()
    {
        OnGameOver(gameLoseScreen);
    }


    void OnGameOver(GameObject gameOverUI)
    {
        gameOverUI.SetActive(true);
        gameIsOver = true;
        GuardScript.OnGuardHasSpottedPlayer -= ShowGameLoseUI;
        PlayerController.collectedAllCrystals -= ShowGameWinUI;
    }
}
