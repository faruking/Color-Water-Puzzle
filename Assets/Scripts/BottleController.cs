using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    public BottleController bottleController;
    public bool justThisBottle = false;
    private int numberOfColorsToTransfer =0;
    public Color[] bottleColors;
    public SpriteRenderer bottleMaskSR;
    public float timeToRotate = 1.0f;

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

        UpdateColorsOnShader();
        UpdateTopCollorValues();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.P) && justThisBottle == true)
        {
            UpdateTopCollorValues();
            //ChooseRotationPointAndDirection();
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
            //ChooseRotationPointAndDirection();
            CalculateRotationIndex(4 - bottleController.numberOfColorsInBottle);
            StartCoroutine(RotateBottle());
        }
    }
    public void StartColorTransfer()
    {
        ChooseRotationPointAndDirection();

        numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayer, 4 - bottleController.numberOfColorsInBottle);
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
    void UpdateColorsOnShader()
    {
        bottleMaskSR.material.SetColor("_Color01", bottleColors[0]);
        bottleMaskSR.material.SetColor("_Color02", bottleColors[1]);
        bottleMaskSR.material.SetColor("_Color03", bottleColors[2]);
        bottleMaskSR.material.SetColor("_Color04", bottleColors[3]);
    }
    void StartColors()
    {
        bottleMaskSR.material.SetColor("_Color01", bottleColors[Random.Range(0, bottleColors.Length)]);
        bottleMaskSR.material.SetColor("_Color02", bottleColors[Random.Range(0, bottleColors.Length)]);
        bottleMaskSR.material.SetColor("_Color03", bottleColors[Random.Range(0, bottleColors.Length)]);
        bottleMaskSR.material.SetColor("_Color04", bottleColors[Random.Range(0, bottleColors.Length)]);
    }
    IEnumerator RotateBottle()
    {
        float t = 0;
        float lerpValue;
        float angleValue;
        float lastAngleValue = 0;

        while(t<timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(0.0f,directionMultiplier * rotationValues[rotationIndex], lerpValue);

            //transform.eulerAngles = new Vector3(0, 0, angleValue);

            transform.RotateAround(choosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotateMultiplierCurv.Evaluate(angleValue));
            if(fillAmounts[numberOfColorsInBottle] >FillAmountCurve.Evaluate(angleValue)+ 0.005f)
            {
                if (lineRenderer.enabled == false)
                {
                    lineRenderer.startColor = topColor;
                    lineRenderer.endColor = topColor;
                    lineRenderer.SetPosition(0, choosenRotationPoint.position);
                    lineRenderer.SetPosition(1, choosenRotationPoint.position - Vector3.up * 1.45f);

                    lineRenderer.enabled = true;
                }
                bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));
                bottleController.FillUp(FillAmountCurve.Evaluate(lastAngleValue)- FillAmountCurve.Evaluate(angleValue));
            }

            

            t += Time.deltaTime *RotateSpeedMultiplier.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame();
        }
        angleValue = directionMultiplier * rotationValues[rotationIndex];
        //transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotateMultiplierCurv.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

        numberOfColorsInBottle -= numberOfTopColorLayer;
        bottleController.numberOfColorsInBottle += numberOfColorsToTransfer;

        lineRenderer.enabled = false;
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

            //transform.eulerAngles = new Vector3(0, 0, angleValue);
            transform.RotateAround(choosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotateMultiplierCurv.Evaluate(angleValue));
            lastAngleValue = angleValue;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        UpdateTopCollorValues();
        angleValue = 0;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotateMultiplierCurv.Evaluate(angleValue));

        StartCoroutine(MoveBottleBack());
    }
    public void UpdateTopCollorValues()
    {
        Debug.Log("Upadted again....");
        if(numberOfColorsInBottle !=0)
        {
            numberOfTopColorLayer = 1;
            topColor = bottleColors[numberOfColorsInBottle - 1];
            if(numberOfColorsInBottle ==4)
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
                       
                    }
                }
            }
            else if (numberOfColorsInBottle == 2)
            {
                if (bottleColors[1].Equals(bottleColors[0]))
                {
                    numberOfTopColorLayer = 2;
                    
                }
            }
            rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayer);
        }
    }
    public bool FillBottleCheck(Color colorToCheck)
    {
        if(numberOfColorsInBottle ==0)
        {
            return true;
        }
        else
        {
            if(numberOfColorsInBottle ==4)
            {
                return false;
            }
            else
            {
                if(topColor.Equals(colorToCheck))
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
}
