﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class SludgeJudgeScenario : MonoBehaviour, ITrackableEventHandler
{
    //Sludge Judge shortened to sJ often

    TrackableBehaviour mTrackableBehaviour;
    bool storyHasStarted;

    [Header("Object References")]
    [SerializeField] Transform sludgeJudge;
    [SerializeField] Transform mainTank;
    [SerializeField] Transform insertionPoint;
    [SerializeField] Transform tankWaterTop;
    [SerializeField] Transform indicator;

    [Space]

    public Camera mainCam;

    [Space]

    [Header("Sludge Colours")]
    public SludgeType sludgeType;

    public Material[] coloredMats;
    public Color[] sludgeColors;

    [SerializeField] private List<SludgeType> sludgeOptions = new List<SludgeType>();
    [Space]

    [Header("Animation Times")]
    [SerializeField] float sJDipTime;
    [SerializeField] float sJSampleTime;
    [SerializeField] float sJExamineTransTime;
    [SerializeField] float sJReturnDelay;

    [Header("Misc Values")]
    [SerializeField] float insertionDistance;

    [Header("Position Object References")]
    [SerializeField] Transform sJDipPoint;
    [SerializeField] Transform sJSampledPoint;
    [SerializeField] Transform sJExaminePoint;
    [SerializeField] Transform tankShrunkPoint;

    [Header("Start Positions")]
    [SerializeField] Vector3 sJStartPoint;
    [SerializeField] Vector3 sJStartScale;
    [SerializeField] Quaternion sJStartRot;
    [Space]
    [SerializeField] Vector3 tankStartPoint;
    [SerializeField] Vector3 tankStartScale;
    [SerializeField] Quaternion tankStartRot;


    public enum SludgeType {Primary, Chemical, ActivatedDark, ActivatedLight};

    // Start is called before the first frame update
    void Start()
    {
        //Set up the event handler for tracking from Vuforia
        mTrackableBehaviour = GameObject.Find("ImageTarget").GetComponent<TrackableBehaviour>();

        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);


        mainCam = Camera.main;
        for (int i = 0; i < 4; i++)
        {
            sludgeOptions.Add((SludgeType)i);
        }

        SetStartPositions();
    }

    // Update is called once per frame
    void Update()
    {
        if (sludgeOptions.Count > 0 && Input.GetKeyDown(KeyCode.A))
        {
            RandomizeSludge();
            SludgeColor();
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            RaycastHit hit = GlobalFunctions.DetectTouch(this);
            if(hit.transform != null)
            {
                Debug.Log(hit.transform.name);
            }
        }
    }

    void RandomizeSludge()
    {
        sludgeType = sludgeOptions[Random.Range(0, sludgeOptions.Count)];
        sludgeOptions.Remove(sludgeType);

        Debug.Log("Currently " + sludgeType + " sludge.");
    }

    void SludgeColor()
    {
        for (int i = 0; i < coloredMats.Length; i++)
        {
            coloredMats[i].color = sludgeColors[(int)sludgeType];
        }
    }
    

    IEnumerator SludgeJudgeStory()
    {
        while(true)
        {
            //if (GlobalFunctions.DetectTouch(this).transform == sludgeJudge)

            RaycastHit hit = GlobalFunctions.DetectConstantTouch();
            if(hit.transform != null)
            {
                if (hit.transform == tankWaterTop)
                {
                    sludgeJudge.position = new Vector3(hit.point.x, sludgeJudge.position.y, hit.point.z);
                }
            }
            else if(Vector2.Distance(new Vector2(sludgeJudge.position.x, sludgeJudge.position.z), new Vector2(insertionPoint.position.x, insertionPoint.position.z)) < insertionDistance)
            {
                sJDipPoint.position = new Vector3(sludgeJudge.position.x, sJDipPoint.position.y, sludgeJudge.position.z);
                sJSampledPoint.position = new Vector3(sludgeJudge.position.x, sJSampledPoint.position.y, sludgeJudge.position.z);
                sJStartPoint = sludgeJudge.position;
                indicator.gameObject.SetActive(false);
                break;
            }

            yield return null;
        }

        Debug.Log("Reached Dip Point");

        while (true)
        {
            //if (GlobalFunctions.DetectTouch(this).transform == sludgeJudge)
           
            GlobalFunctions.DetectTouch(this, new Vector2(100, 100));
            if (GlobalFunctions.swipeDirection == Vector2.down)
            {
                Debug.Log("Sludge judge swiped");
                StartCoroutine("SludgeJudgeDip");
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(sJDipTime + sJSampleTime + sJExamineTransTime + sJReturnDelay);

        while (true)
        {
            if (GlobalFunctions.DetectTouch(this).transform == sludgeJudge)
            {
                Debug.Log("Sludge judge tapped");
                break;
            }
            yield return null;
        }
    }

    IEnumerator SludgeJudgeDip()
    {
        float timeStart = Time.time;
        float currentTime;
        while (true)
        {
            currentTime = Time.time - timeStart;
            sludgeJudge.position = Vector3.Lerp(sJStartPoint, sJDipPoint.position, currentTime / sJDipTime);
            sludgeJudge.localScale = Vector3.Lerp(sJStartScale, sJDipPoint.localScale, currentTime / sJDipTime);
            sludgeJudge.rotation = Quaternion.Lerp(sJStartRot, sJDipPoint.rotation, currentTime / sJDipTime);

            if(currentTime >= sJDipTime)
            {
                break;
            }
            yield return null;
        }


        yield return new WaitForSeconds(10f);

        timeStart = Time.time;
        while (true)
        {
            currentTime = Time.time - timeStart;
            sludgeJudge.position = Vector3.Lerp(sJDipPoint.position, sJSampledPoint.position, currentTime / sJSampleTime);
            sludgeJudge.localScale = Vector3.Lerp(sJDipPoint.localScale, sJSampledPoint.localScale, currentTime / sJSampleTime);
            sludgeJudge.rotation = Quaternion.Lerp(sJDipPoint.rotation, sJSampledPoint.rotation, currentTime / sJSampleTime);

            if (currentTime >= sJSampleTime)
            {
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(3);

        timeStart = Time.time;
        while (true)
        {
            currentTime = Time.time - timeStart;
            sludgeJudge.position = Vector3.Lerp(sJSampledPoint.position, sJExaminePoint.position, currentTime / sJExamineTransTime);
            sludgeJudge.localScale = Vector3.Lerp(sJSampledPoint.localScale, sJExaminePoint.localScale, currentTime / sJExamineTransTime);
            sludgeJudge.rotation = Quaternion.Lerp(sJSampledPoint.rotation, sJExaminePoint.rotation, currentTime / sJExamineTransTime);


            mainTank.position = Vector3.Lerp(tankStartPoint, tankShrunkPoint.position, currentTime / sJExamineTransTime);
            mainTank.localScale = Vector3.Lerp(tankStartScale, tankShrunkPoint.localScale, currentTime / sJExamineTransTime);
            mainTank.rotation = Quaternion.Lerp(tankStartRot, tankShrunkPoint.rotation, currentTime / sJExamineTransTime);

            if (currentTime >= sJExamineTransTime)
            {
                break;
            }
            yield return null;
        }
    }
    




    //Sets a Vector3 to the position of an object at the start
    void SetStartPositions()
    {
        sJStartPoint = sludgeJudge.position;
        sJStartScale = sludgeJudge.localScale;
        sJStartRot = sludgeJudge.rotation;

        tankStartPoint = mainTank.position;
        tankStartScale = mainTank.localScale;
        tankStartRot = mainTank.rotation;
    }


    
    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        if ((newStatus == TrackableBehaviour.Status.TRACKED || newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) && previousStatus == TrackableBehaviour.Status.NO_POSE && storyHasStarted == false)
        {
            Debug.Log("Starting story");
            storyHasStarted = true;
            StartCoroutine("SludgeJudgeStory");
        }
    }
}
