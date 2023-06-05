using System.Collections;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject gameManager;
    public GameObject soundManager;
    // Start is called before the first frame update
    void Awake()
    {
        if (GameManager.instance == null)
            Instantiate(gameManager);
        if (SoundManager.instance == null)
            Instantiate(soundManager);
    }
}
