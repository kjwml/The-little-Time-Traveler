using UnityEngine;

public class Backpack : MonoBehaviour
{
   public GameObject itemBar;

    public void BackpackActive()
    {
        itemBar.SetActive(!itemBar.activeSelf);
    }
}
