using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public  class GameController : MonoBehaviour
{
    public static bool mute = false;
    public BottleController firstBottle;
    public BottleController secondBottle;

    public GameObject home;
    public GameObject gamePlayPanel;
    public GameObject topBar;
    public GameObject backgroundPanel;
    public GameObject bottlePanel;
    public GameObject pauseMenu;


    private AudioSource audioSource;

    public GameObject levelCompletedPanel;
    public GameObject bottleContainer;
    public GameObject gameOverPanel;

    public TextMeshProUGUI moves;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI soundText;
    public TextMeshProUGUI pauseSoundText;
    public TextMeshProUGUI bottlesRemainingText; 
    public TextMeshProUGUI highScoreText; 
    public TextMeshProUGUI gameOverHighScoreText; 

    public int highScore;
    public int numberOfCompletedBottles;
    public int numberOfMoves = 24;
    public int score;
    public int bottlesRemaining; 
    public int level;
    private string isSoundEnabled;

    private void Awake() {
        score = PlayerPrefs.GetInt("Score",0);
        level = PlayerPrefs.GetInt("Level",1);
        highScore = PlayerPrefs.GetInt("highScore",0);
        isSoundEnabled = PlayerPrefs.GetString("sound","on");
        bottlesRemaining = PlayerPrefs.GetInt("Bottles Remaining",3);
        if(score > 0){
        highScoreText.text = "High Score: " + highScore;    
        infoText.text = "";
        gamePlayPanel.SetActive(true);
        topBar.SetActive(true);
        home.SetActive(false);
        levelText.text = "Level: " + level;
        scoreText.text = "Score: " + score;
        if (level > 3 && level < 6)
        {
            numberOfMoves = 20;
            bottlesRemaining = 4;
        }
        if (level >= 6)
        {
            bottlesRemaining = 4;
            numberOfMoves = 15;
        } 
        moves.text = "Moves Left: " + numberOfMoves;
        bottlesRemainingText.text = "Bottles Left: " + bottlesRemaining;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (isSoundEnabled == "on")
        {
            audioSource.Play();
            pauseSoundText.text = "Sound ON";
        }
        else
        {
            audioSource.Stop();
            pauseSoundText.text = "Sound OFF";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
    {
    // Back Button was pressed!
        quitGame();
    }
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if(hit.collider != null)
            {
                
                if(hit.collider.GetComponent<BottleController>()!= null)
                {
                    if(firstBottle == null)
                    {
                        firstBottle = hit.collider.GetComponent<BottleController>();
                        AudioManager.instance.Play("bottleClicked");
                    }else
                    {
                        Debug.Log("Else of FirstBottle Null");
                        //Hitting same bottle twice 
                        if (firstBottle == hit.collider.GetComponent<BottleController>())
                        {
                            firstBottle = null;
                            AudioManager.instance.Play("error");
                            Debug.Log("hit same bottle twice");
                        }else
                        {
                            // hitting first and second bottle one after the other
                            secondBottle = hit.collider.GetComponent<BottleController>();
                            firstBottle.bottleController = secondBottle;
                            // firstBottle.isFirstBottleEmpty();
                            if(secondBottle.FillBottleCheck())
                            {
                                // checking for game over conditions
                                if (numberOfMoves > 0) 
                                {
                                AudioManager.instance.Play("bottleClicked");
                                // Transferring color
                                firstBottle.StartColorTransfer();
                                infoText.text = "";
                                firstBottle = null;
                                secondBottle = null;  
                                }
                              
                            }else
                            {
                                AudioManager.instance.Play("error");
                                Debug.Log("Not Possible");
                                //transfer is not possible
                                firstBottle = null;
                                secondBottle = null;
                            }
                        }
                    }
                }
            }
        }
    }
    public void Play(){
        topBar.SetActive(true);
        bottlesRemaining = 3;
        bottlesRemainingText.text = "Bottles Left: " + bottlesRemaining;
        home.SetActive(false);
        scoreText.text = "Score: " + score;
        highScoreText.text = "High Score: " + highScore;
        gamePlayPanel.SetActive(true);
        levelText.text = "Level: " + level;
        moves.text = "Moves Left: " + numberOfMoves;
        infoText.text = "Click on a bottle then click on another bottle to transfer color";
    }
    public void Pause(){
        pauseMenu.SetActive(true);
        Time.timeScale = 0.001f;
        gamePlayPanel.SetActive(false);
    }
    public void resume(){
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        gamePlayPanel.SetActive(true);
    }
    public void updateMoves(){
        numberOfMoves--;
        moves.text = "Moves Left : " + numberOfMoves;
        score += 2;
        scoreText.text = "Score: " + score;
        if (numberOfMoves == 0)
        {
            highScore = highScore > score ? highScore : score;
            highScoreText.text = "High Score: " + highScore;
            PlayerPrefs.SetInt("highScore", highScore);
            GameOver();
        }
    }
    public void GameOver(){
        bottleContainer.SetActive(false);
        gameOverHighScoreText.text = "High Score: " + highScore;
        gameOverPanel.SetActive(true);
        topBar.SetActive(false);
        AudioManager.instance.Play("gameover");
    }
    public void levelCompleted(){
            levelCompletedPanel.SetActive(true);
            gamePlayPanel.SetActive(false);
            Time.timeScale = 0.001f;
            PlayerPrefs.SetInt("Score", score);
            PlayerPrefs.SetInt("Level", level + 1);
            AudioManager.instance.Play("winlevel");
            PlayerPrefs.SetInt("Bottles Left",3);     
    }
    public void reduceBottle(){
        bottlesRemaining--;
        bottlesRemainingText.text = "Bottles Left: " + bottlesRemaining; 
    } 
    public void restart(){
        SceneManager.LoadScene(0);
    }
    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("Score", 0);
        PlayerPrefs.SetInt("Level", 1);
    }
    public void quitGame(){
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
   public void LoadNextScene()
    {
        if((SceneManager.GetActiveScene().buildIndex + 1) < 2){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
    public void nextLevel(){
        levelCompletedPanel.SetActive(false);
        gamePlayPanel.SetActive(true);
        numberOfMoves = 20;
        moves.text = "Moves Left: " + numberOfMoves;
        Time.timeScale = 1;
        numberOfCompletedBottles = 0;
        bottlesRemainingText.text = "Bottles Left: " + 3;
        SceneManager.LoadScene(0);
    }
    public void toggleMute(){
        if (mute)
        {
            soundText.text = "Sound ON";
            pauseSoundText.text = "Sound ON";
            PlayerPrefs.SetString("sound","on");
            audioSource.Play();
            mute = false;
        }
        else{
            soundText.text = "Sound OFF";
            pauseSoundText.text = "Sound OFF";
            audioSource.Stop();
            PlayerPrefs.SetString("sound","off");
            mute = true;
        }
    }
}
