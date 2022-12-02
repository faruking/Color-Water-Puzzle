using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleControl : MonoBehaviour
{
    public Color[] bottleColor; //Colors that will be found in each  bottle
    public SpriteRenderer bottleMaskSR; // the mask of the bottle

    public AnimationCurve ScaleAndRotationMultiplierCurve;
    public AnimationCurve FillAmountCurve;
    public AnimationCurve RotationSpeedMultiplier;

    public float[] fillAmounts;
    public float[] rotationValues;

    private int rotationIndex = 0;

    [Range(0,4)]
    public int numberOfColorsInBottle = 4;

    public Color topColor;
    public int numberOfTopColorLayers = 1;

    public BottleControl bottleControlRef;
    public bool justThisBottle = false;
    private int numberOfColorsToTransfer = 0;

    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    private Transform chosenRotationPoint;

    private float directionMultiplier = 1.0f;

    Vector3 originalPosition;
    Vector3 startPosition;
    Vector3 endPosition;

    public LineRenderer lineRenderer;
    private Color color1,color2,color3,color4;
    // Start is called before the first frame update
    void Start()
    {
        //StartColor();
        numberOfColorsInBottle = Random.Range(1, 4);
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);

        originalPosition = transform.position;

        UpdateColorOnShader();

        UpdateTopColorValues();
    }
    void StartColor()
    {
        color1 = bottleColor[Random.Range(0, bottleColor.Length)];
        color2 = bottleColor[Random.Range(0, bottleColor.Length)];
        color3 = bottleColor[Random.Range(0, bottleColor.Length)];
        color4 = bottleColor[Random.Range(0, bottleColor.Length)];
    }
    void RandomFillAmount()
    {
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[Random.Range(0,4)]);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.P) && justThisBottle ==true)
        {
            UpdateTopColorValues();
            if(bottleControlRef.FillBottleCheck(topColor))
            {
                ChoseRotationPointAndDirection();

                numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - bottleControlRef.numberOfColorsInBottle);
                for (int i = 0; i < numberOfColorsToTransfer; i++)
                {
                    bottleControlRef.bottleColor[bottleControlRef.numberOfColorsInBottle + i] = topColor;
                }
                bottleControlRef.UpdateColorOnShader();
            }
            CalculateRotationIndex(4 - bottleControlRef.numberOfColorsInBottle);
            StartCoroutine(RotateBottle());
        }
    }
    public void StartColorTransfer()
    {
        ChoseRotationPointAndDirection();

        numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - bottleControlRef.numberOfColorsInBottle);
        for (int i = 0; i < numberOfColorsToTransfer; i++)
        {
            bottleControlRef.bottleColor[bottleControlRef.numberOfColorsInBottle + i] = topColor;
        }
        bottleControlRef.UpdateColorOnShader();

        CalculateRotationIndex(4 - bottleControlRef.numberOfColorsInBottle);

        transform.GetComponent<SpriteRenderer>().sortingOrder += 2;
        bottleMaskSR.sortingOrder += 2;

        StartCoroutine(MoveBottle());
    }
    IEnumerator MoveBottle()
    {
        startPosition = transform.position;
        if (chosenRotationPoint == leftRotationPoint)
        {
            endPosition = bottleControlRef.rightRotationPoint.position;
        }
        else
        {
            endPosition = bottleControlRef.leftRotationPoint.position;
        }
        float t = 0;
        while (t <= 1)
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
        while (t <= 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;

            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;

        transform.GetComponent<SpriteRenderer>().sortingOrder -= 2;
        bottleMaskSR.sortingOrder -= 2;
    }
    void UpdateColorOnShader()
    {
        //setting the color on the shader to the color in our array of bottlecolor
        bottleMaskSR.material.SetColor("_Color01", bottleColor[0]);
        bottleMaskSR.material.SetColor("_Color02", bottleColor[1]);
        bottleMaskSR.material.SetColor("_Color03", bottleColor[2]);
        bottleMaskSR.material.SetColor("_Color04", bottleColor[3]);
    }
    public float timeToRotate = 1.0f;
    IEnumerator RotateBottle()
    {
        float t = 0;
        float lerpValue;
        float angleValue;
        float lastAngleValue = 0;
        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(0.0f, directionMultiplier * rotationValues[rotationIndex], lerpValue);
            //transform.eulerAngles = new Vector3(0, 0, angleValue);

            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            if(fillAmounts[numberOfColorsInBottle]>FillAmountCurve.Evaluate(angleValue) +0.005f)
            {
                if (lineRenderer.enabled == false)
                {
                    lineRenderer.startColor = topColor;
                    lineRenderer.endColor = topColor;
                    lineRenderer.SetPosition(0, chosenRotationPoint.position);
                    lineRenderer.SetPosition(1, chosenRotationPoint.position - Vector3.up * 1.45f);

                    lineRenderer.enabled = true;
                }

                bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

                bottleControlRef.FillUp(FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue));
            }

            t += Time.deltaTime * RotationSpeedMultiplier.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame();
        }
        angleValue = directionMultiplier * rotationValues[rotationIndex];
        //transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

        numberOfColorsInBottle -= numberOfTopColorLayers;
        bottleControlRef.numberOfColorsInBottle += numberOfColorsToTransfer;

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
            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            lastAngleValue = angleValue;

            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        UpdateTopColorValues();
        angleValue = 0;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

        StartCoroutine(MoveBottleBack());
    }
    public void UpdateTopColorValues()
    {
        if(numberOfColorsInBottle !=0)
        {
            numberOfTopColorLayers = 1;

            topColor = bottleColor[numberOfColorsInBottle - 1];

            if(numberOfColorsInBottle ==4)
            {
                if(bottleColor[3].Equals(bottleColor[2]))
                {
                    numberOfTopColorLayers = 2;
                    if(bottleColor[2].Equals(bottleColor[1]))
                    {
                        numberOfTopColorLayers = 3;
                        if(bottleColor[1].Equals(bottleColor[0]))
                        {
                            numberOfTopColorLayers = 4;
                        }
                    }
                }
            }
            else if (numberOfColorsInBottle == 3)
            {
                if (bottleColor[2].Equals(bottleColor[1]))
                {
                    numberOfTopColorLayers = 2;
                    if (bottleColor[1].Equals(bottleColor[0]))
                    {
                        numberOfTopColorLayers = 3;
                        
                    }
                }
            }
            else if (numberOfColorsInBottle == 2)
            {
                if (bottleColor[1].Equals(bottleColor[0]))
                {
                    numberOfTopColorLayers = 2;
                }
            }
            rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers);
        }
    }
    public bool FillBottleCheck(Color colorToCheck)
    {
        if (numberOfColorsInBottle ==0)
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
    private void CalculateRotationIndex(int emptySpaces)
    {
        rotationIndex = 3 - (numberOfColorsInBottle - Mathf.Min(emptySpaces, numberOfTopColorLayers));
    }
    private void FillUp(float fillAmountToAdd)
    {
        bottleMaskSR.material.SetFloat("_FillAmount", bottleMaskSR.material.GetFloat("_FillAmount") + fillAmountToAdd);
    }
    private void ChoseRotationPointAndDirection()
    {
        if (transform.position.x > bottleControlRef.transform.position.x)
        {
            chosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;

        }
        else
        {
            chosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }
}
