using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    public BottleController bottleController;
    public GameObject bottle;
    public bool justThisBottle = false;
    private int numberOfColorsToTransfer = 0;
    public Color[] bottleColors;
    private Color randomColor1;
    private Color randomColor2;
    private Color randomColor3;
    private Color randomColor4;
    private Color[] randomColors;
    public Color greyedOutColor;
    public SpriteRenderer bottleMaskSR;
    public float timeToRotate = 1.0f;
    public GameController gameController;
    private int updatedNumberOfColors;
    public int numberOfCompletedBottles = 0;

    public AnimationCurve ScaleAndRotateMultiplierCurv;
    public AnimationCurve FillAmountCurve;
    public AnimationCurve RotateSpeedMultiplier;
   
    public float[] fillAmounts;
    public float[] rotationValues;

    public int rotationIndex = 0;

    [Range(0,4)]
    public int numberOfColorsInBottle = 4;

    public Color topColor;
    public int numberOfTopColorLayer = 1;

    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    private Transform choosenRotationPoint;

    public LineRenderer lineRenderer;

    private float directionMultiplier =1.0f;
    Vector3 originalPosition;
    Vector3 startPosition;
    Vector3 endPosition;
    // Start is called before the first frame update
    void Start()
    {
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);
        originalPosition = transform.position;
        StartColors();
        randomColors = new Color[4]{randomColor1, randomColor2, randomColor3, randomColor4};
        bottleColors[0] = randomColors[0];
        bottleColors[1] = randomColors[1];
        bottleColors[2] = randomColors[2];
        bottleColors[3] = randomColors[3];

        UpdateColorsOnShader();
        UpdateTopCollorValues();
    }
 void StartColors()
    {
        randomColor1 = bottleColors[Random.Range(0, bottleColors.Length)];
        randomColor2 = bottleColors[Random.Range(0, bottleColors.Length)];
        randomColor3 = bottleColors[Random.Range(0, bottleColors.Length)];
        randomColor4 = bottleColors[Random.Range(0, bottleColors.Length)];
        bottleMaskSR.material.SetColor("_Color01", randomColor1);
        bottleMaskSR.material.SetColor("_Color02", randomColor2);
        bottleMaskSR.material.SetColor("_Color03", randomColor3);
        bottleMaskSR.material.SetColor("_Color04", randomColor4);
    }
    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetKeyUp(KeyCode.M) && justThisBottle == true)
        {
            UpdateTopCollorValues();
            if (bottleController.FillBottleCheck(topColor))
            {
                ChooseRotationPointAndDirection();

                numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayer, 4 - bottleController.numberOfColorsInBottle);
                for (int i = 0; i < numberOfColorsToTransfer; i++)
                {
                    bottleController.bottleColors[bottleController.numberOfColorsInBottle + i] = topColor;
                }
                bottleController.UpdateColorsOnShader();

            }
            CalculateRotationIndex(4 - bottleController.numberOfColorsInBottle);
            StartCoroutine(RotateBottle());
        }
    
    }
    public void StartColorTransfer()
    {
        ChooseRotationPointAndDirection();
        numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayer, 4 - bottleController.numberOfColorsInBottle);
        // numberOfColorsToTransfer = 1;

        Debug.Log(numberOfColorsToTransfer + "number");
        for (int i = 0; i < numberOfColorsToTransfer; i++)
        {
            bottleController.bottleColors[bottleController.numberOfColorsInBottle + i] = topColor;
        }
        bottleController.UpdateColorsOnShader();

        CalculateRotationIndex(4 - bottleController.numberOfColorsInBottle);

        transform.GetComponent<SpriteRenderer>().sortingOrder += 2;
        bottleMaskSR.sortingOrder += 2;
        StartCoroutine(MoveBottle());     
    }
    IEnumerator MoveBottle()
    {
        startPosition = transform.position;
        if(choosenRotationPoint == leftRotationPoint)
        {
            endPosition = bottleController.rightRotationPoint.position;
        }else
        {
            endPosition = bottleController.leftRotationPoint.position;
        }
        float t = 0;
        while(t <1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;

            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
        StartCoroutine(RotateBottle());
    }
     IEnumerator RotateBottle()
    { 
        float t = 0;
        float lerpValue;
        float angleValue;
        float lastAngleValue = 0;
        while (t < timeToRotate)
        {
            AudioManager.instance.Play("PourSound");
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(0.0f, directionMultiplier * rotationValues[rotationIndex], lerpValue);
            transform.RotateAround(choosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotateMultiplierCurv.Evaluate(angleValue));
            if(fillAmounts[numberOfColorsInBottle]>FillAmountCurve.Evaluate(angleValue) +0.005f)
            {
                if (lineRenderer.enabled == false)
                {
                    lineRenderer.startColor = topColor;
                    lineRenderer.endColor = topColor;
                    lineRenderer.SetPosition(0, choosenRotationPoint.position);
                    lineRenderer.SetPosition(1, choosenRotationPoint.position - Vector3.up * 0.85f);

                    lineRenderer.enabled = true;
                }

                bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

                bottleController.FillUp(FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue));
            }

            t += Time.deltaTime * RotateSpeedMultiplier.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame();
        }
        angleValue = directionMultiplier * rotationValues[rotationIndex];
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotateMultiplierCurv.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

        numberOfColorsInBottle -= numberOfTopColorLayer;
        bottleController.numberOfColorsInBottle += numberOfColorsToTransfer;
        updatedNumberOfColors = bottleController.numberOfColorsInBottle;

        lineRenderer.enabled = false;
        // AudioManager.instance.Stop("PourSound");
        StartCoroutine(RotateBottleBack());
    }
      IEnumerator RotateBottleBack()
    {

        float t = 0;
        float lerpValue;
        float angleValue;
        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(directionMultiplier * rotationValues[rotationIndex], 0.0f, lerpValue);

            transform.RotateAround(choosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotateMultiplierCurv.Evaluate(angleValue));
            lastAngleValue = angleValue;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        angleValue = 0;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotateMultiplierCurv.Evaluate(angleValue));

        StartCoroutine(MoveBottleBack());
    }
    IEnumerator MoveBottleBack()
    {
        startPosition = transform.position;
        endPosition = originalPosition;
        float t = 0;
        while (t < 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;

            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;

        transform.GetComponent<SpriteRenderer>().sortingOrder -= 2;
        bottleMaskSR.sortingOrder -= 2;
    }
    public void UpdateColorsOnShader()
    {
        bottleMaskSR.material.SetColor("_Color01", bottleColors[0]);
        bottleMaskSR.material.SetColor("_Color02", bottleColors[1]);
        bottleMaskSR.material.SetColor("_Color03", bottleColors[2]);
        bottleMaskSR.material.SetColor("_Color04", bottleColors[3]);
    }
   
    public void UpdateTopCollorValues()
    {
        Debug.Log("Updated again....");
        if(numberOfColorsInBottle != 0)
        {
            numberOfTopColorLayer = 1;
            topColor = bottleColors[numberOfColorsInBottle - 1];
            if(numberOfColorsInBottle == 4)
            {
                if(bottleColors[3].Equals(bottleColors[2]))
                {
                    numberOfTopColorLayer = 2;
                    if(bottleColors[2].Equals(bottleColors[1]))
                    {
                        numberOfTopColorLayer = 3;
                        if(bottleColors[1].Equals(bottleColors[0]))
                        {
                            numberOfTopColorLayer = 4;
                            // bottle.SetActive(false);
                            //Lock bottle
                        }
                    }
                }
            }
            else if (numberOfColorsInBottle == 3)
            {
              

                if (bottleColors[2].Equals(bottleColors[1]))
                {
                    numberOfTopColorLayer = 2;
                    if (bottleColors[1].Equals(bottleColors[0]))
                    {
                        numberOfTopColorLayer = 3;
                        Debug.Log("filled!");
                        // bottle.SetActive(false);

                    }
                }
            }
            else if (numberOfColorsInBottle == 2)
            {
                if (bottleColors[1].Equals(bottleColors[0]))
                {
                    numberOfTopColorLayer = 2;
                    // bottle.SetActive(false);
                }
            }
            rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayer);
        }
    }
    public bool FillBottleCheck(Color colorToCheck)
    {
        if(numberOfColorsInBottle == 0)
        {
            return true;
            Debug.Log("Empty");
        }
        else
        {
            if(numberOfColorsInBottle == 4)
            {
                return false;
            }
            else
            {
                // if(topColor.Equals(colorToCheck))
                // {
                //     return true;
                // }
                // else
                // {
                //     return false;
                // }
                return true;
            }
        }
    }
    private void CalculateRotationIndex(int numSpacesInSecondBottle)
    {
        rotationIndex = 3 - (numberOfColorsInBottle - Mathf.Min(numSpacesInSecondBottle, numberOfTopColorLayer));
    }
    private void FillUp(float fillAmount)
    {
        bottleMaskSR.material.SetFloat("_FillAmount", bottleMaskSR.material.GetFloat("_FillAmount") + fillAmount);
    }
    private void ChooseRotationPointAndDirection()
    {
        //Debug.Log("FX CALLED");
        if(transform.position.x > bottleController.transform.position.x)
        {
            choosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;

        }else
        {
            choosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }
      public bool checkBottleFill(){
        if (bottleMaskSR.material.GetColor("_Color01") == bottleMaskSR.material.GetColor("_Color02") && bottleMaskSR.material.GetColor("_Color03") == bottleMaskSR.material.GetColor("_Color04"))
        {
            Debug.Log("bottle filled");
            numberOfCompletedBottles++;
      

            return true;
        }
        else{
            return false;
        }
    }
    public void disableBottle(){
        bottleMaskSR.material.SetColor("_Color01",greyedOutColor);
        bottleMaskSR.material.SetColor("_Color02",greyedOutColor);
        bottleMaskSR.material.SetColor("_Color03",greyedOutColor);
        bottleMaskSR.material.SetColor("_Color04",greyedOutColor);
    }

}
