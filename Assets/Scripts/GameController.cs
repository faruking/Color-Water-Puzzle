using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameController : MonoBehaviour
{
    public BottleController firstBottle;
    public BottleController secondBottle;
    public GameObject PlayButton;
    public GameObject title;
    public GameObject gamePlayPanel;
    public TextMeshProUGUI moves;
    public int numberOfMoves = 24;


    // Start is called before the first frame update
    void Start()
    {
        // Play();
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
                        if (firstBottle == hit.collider.GetComponent<BottleController>())
                        {
                            firstBottle = null;
                        }else
                        {
                            secondBottle = hit.collider.GetComponent<BottleController>();
                            firstBottle.bottleController = secondBottle;

                            //firstBottle.UpdateTopCollorValues();
                            //secondBottle.UpdateTopCollorValues();

                            if(secondBottle.FillBottleCheck(firstBottle.topColor)== true)
                            {
                                firstBottle.StartColorTransfer();
                                firstBottle = null;
                                secondBottle = null;
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
    public void Play(){
        PlayButton.SetActive(false);
        title.SetActive(false);
        gamePlayPanel.SetActive(true);
        Debug.Log("dddddddddddd");
        moves.text = "Moves Left: " + numberOfMoves;
    }
    public void updateMoves(){
        numberOfMoves--;
        if (numberOfMoves == 0)
        {
            GameOver();
        }
        moves.text = "Moves Left : " + numberOfMoves;
    }
    public void GameOver(){
        
    }
}
