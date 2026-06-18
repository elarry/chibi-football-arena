using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // eL: So that this script can be referenced in any other scrip
    // eL: We need to update the scores
    public static ScoreManager instance;


    // public Text scoreText;
    public TMP_Text blueScoreText;
    public TMP_Text purpleScoreText;

    int blueScore = 0;
    int purpleScore = 0;

    private void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        blueScoreText.text = blueScore.ToString();
        purpleScoreText.text = purpleScore.ToString();
    }

    // Update is called once per frame
    public void AddBluePoint() {
        blueScore += 1;
        blueScoreText.text = blueScore.ToString();
        Debug.Log($"Blue-Purple Score: {blueScore}-{purpleScore}");
    }
    
    public void AddPurplePoint() {
        purpleScore += 1;
        purpleScoreText.text = purpleScore.ToString();
        Debug.Log($"Blue-Purple Score: {blueScore}-{purpleScore}");
    }
}
