using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    public BottleControl firstBottle;
    public BottleControl secondBottle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                if(hit.collider.GetComponent<BottleControl>()!= null)
                {
                    if (firstBottle == null)
                    {
                        firstBottle = hit.collider.GetComponent<BottleControl>();
                    }
                    else
                    {
                        if (firstBottle == hit.collider.GetComponent<BottleControl>())
                        {
                            firstBottle = null;
                        }
                        else
                        {
                            secondBottle = hit.collider.GetComponent<BottleControl>();
                            firstBottle.bottleControlRef = secondBottle;

                            firstBottle.UpdateTopColorValues();
                            secondBottle.UpdateTopColorValues();

                            if (secondBottle.FillBottleCheck(firstBottle.topColor) == true)
                            {
                                firstBottle.StartColorTransfer();
                                firstBottle = null;
                                secondBottle = null;
                            }
                            else
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
}
