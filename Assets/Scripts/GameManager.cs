using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public LevelGenerator levelGenerator;
    [SerializeField]
    private GameObject playerPrefab;

    public FPSController playerController;
    public UI ui;

    private int currentLevel;
    public bool changingLevel;

    void Start()
    {
        levelGenerator = GetComponent<LevelGenerator>();
        StartCoroutine(levelGenerator.GenerateNewLevel(1));
        ui.SetLevel(1);
        currentLevel = 1;
        Time.timeScale = 1f;
        Transform player = GameObject.Instantiate(playerPrefab).transform;
        playerController = player.GetComponent<FPSController>();
        StartCoroutine(DisableFirstGameText());
    }

    IEnumerator DisableFirstGameText()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        FindObjectOfType<UI>().firstGame.enabled = false;
    }

    public void NextLevel()
    {
        changingLevel = true;
        currentLevel += 1;
        ui.SetLevel(currentLevel);
        levelGenerator.DestroyLevel();
        StartCoroutine(levelGenerator.GenerateNewLevel(currentLevel));
        playerController.GetComponent<CharacterController>().enabled = false;
    }

    public static void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
