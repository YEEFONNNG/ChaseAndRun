using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance { get; private set; }

    [SerializeField]
    private Button _gameOverUI;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _gameOverUI.onClick.AddListener(
            () =>
            {
                SceneManager.LoadScene(0);
            }
        );
    }

    public void GameOver()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _gameOverUI.transform.parent.gameObject.SetActive(true);
    }
}
