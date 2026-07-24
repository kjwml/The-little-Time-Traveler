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

    public void Switch()
    {
        isGreekActive = !isGreekActive;

        if (isGreekActive)
        {
            textGreek.SetActive(true);
            buttonGreek.SetActive(true);
            textG.SetActive(false);
            textGothic.SetActive(false);
            buttonGothic.SetActive(false);
        }
        else
        {
            textGreek.SetActive(false);
            buttonGreek.SetActive(false);
            textG.SetActive(true);
            textGothic.SetActive(true);
            buttonGothic.SetActive(true);
        }
    }

}
