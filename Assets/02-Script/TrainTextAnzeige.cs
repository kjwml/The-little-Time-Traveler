using UnityEngine;

public class TrainTextAnzeige : MonoBehaviour
{
    [Header("greek antique objects")]
    public GameObject textGreek;
    public GameObject buttonGreek;

    [Header("gothic objects")]
    public GameObject textGothic;
    public GameObject buttonGothic;

    private bool isGreekActive = true;

    private void Start()
    {
        
        textGreek.SetActive(true);
        buttonGreek.SetActive(true);
        textGothic.SetActive(false);
        buttonGothic.SetActive(false);
    }

    public void Switch()
    {
        isGreekActive = !isGreekActive;

        if (isGreekActive)
        {
            textGreek.SetActive(true);
            buttonGreek.SetActive(true);
            textGothic.SetActive(false);
            buttonGothic.SetActive(false);
        }
        else
        {
            textGreek.SetActive(false);
            buttonGreek.SetActive(false);
            textGothic.SetActive(true);
            buttonGothic.SetActive(true);
        }
    }

}