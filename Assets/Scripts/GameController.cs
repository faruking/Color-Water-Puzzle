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

    public GameObject PlayButton;
    public GameObject title;
    public GameObject gamePlayPanel;
    public GameObject scorePanel;
    public GameObject optionsPanel;

    public GameObject levelCompletedPanel;
    public GameObject bottleContainer;
    public GameObject gameOverPanel;

    public TextMeshProUGUI moves;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI soundText;


    public int numberOfCompletedBottles;
    public int numberOfMoves = 24;
    public int score;
    public int level;




    // Start is called before the first frame update
    void Start()
    {
        // Play();
        // AudioManager.instance.Play("bgm");
    }

    // Update is called once per frame
    void Update()
    {
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
                    }else
                    {
                        Debug.Log("Else of FirstBottle Null");
                        //Hitting same bottle twice 
                        if (firstBottle == hit.collider.GetComponent<BottleController>())
                        {
                            firstBottle = null;
                            Debug.Log("hit same bottle twice");
                        }else
                        {
                            // hitting first and second bottle one after the other
                            secondBottle = hit.collider.GetComponent<BottleController>();
                            firstBottle.bottleController = secondBottle;

                            firstBottle.UpdateTopCollorValues();
                            secondBottle.UpdateTopCollorValues();

                            if(secondBottle.FillBottleCheck(firstBottle.topColor) == true)
                            {
                                // checking for game over conditions
                                if (numberOfMoves > 0) 
                                {
                                    updateMoves();
                                // Transferring color
                                firstBottle.StartColorTransfer();
                                infoText.text = "";
                                if(secondBottle.checkBottleFill()){
                                    numberOfCompletedBottles++;
                                    // secondBottle.disableBottle();
                                    levelCompleted(3);
                                }
                                
                                firstBottle = null;
                                secondBottle = null;  
                                }else
                                {
                                    GameOver();
                                }
                              
                            }else
                            {
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
    private void Awake() {
        score = PlayerPrefs.GetInt("Score",0);
        level = PlayerPrefs.GetInt("Level",1);
        if(score > 0){
        infoText.text = "";
        PlayButton.SetActive(false);
        title.SetActive(false);
        // scoreText.text = "Score: " + score;
        gamePlayPanel.SetActive(true);
        optionsPanel.SetActive(false);
        levelText.text = "Level: " + level;
        scoreText.text = "Score: " + score;
        moves.text = "Moves Left: " + numberOfMoves;
        }
    }
    public void Play(){
        PlayButton.SetActive(false);
        title.SetActive(false);
        scoreText.text = "Score: " + score;
        gamePlayPanel.SetActive(true);
        optionsPanel.SetActive(false);
        levelText.text = "Level: " + level;
        moves.text = "Moves Left: " + numberOfMoves;
        infoText.text = "Click on a bottle then click on another bottle to transfer color";
    }
    public void updateMoves(){
        numberOfMoves--;
        moves.text = "Moves Left : " + numberOfMoves;
        score += 2;
        scoreText.text = "Score: " + score;
    }
    public void GameOver(){
        bottleContainer.SetActive(false);
        gameOverPanel.SetActive(true);
        AudioManager.instance.Play("gameover");
    }
    public void levelCompleted(int difficulty){
        if(numberOfCompletedBottles == difficulty){
            levelCompletedPanel.SetActive(true);
            gamePlayPanel.SetActive(false);
            Time.timeScale = 0.001f;
            PlayerPrefs.SetInt("Score", score);
            PlayerPrefs.SetInt("Level", level + 1);
            AudioManager.instance.Play("winlevel");

            // LoadNextScene();
        }

    }
    public void restart(){
        SceneManager.LoadScene(0);
    }
    public void quitGame(){
        PlayerPrefs.SetInt("Score", 0);
        PlayerPrefs.SetInt("Level", 1);
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

        SceneManager.LoadScene(0);

    }
    public void toggleMute(){
        if (mute)
        {
                    soundText.text = "Sound ON";

            mute = false;
        }
        else{
                    soundText.text = "Sound OFF";

            mute = true;
        }
    }
}
