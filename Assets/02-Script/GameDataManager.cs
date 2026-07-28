using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    // Zustände der Items (gefunden = true)
   public bool amphoraFound = false;
    public bool mosaicFound = false;
    public bool statueFound = false;
    public bool columnFound = false;
    public bool wreathFound = false;
    public bool templeFound = false;
   

    private void Awake()
    {
        // Singleton-Muster: Stellt sicher, dass es nur einen Manager gibt
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
