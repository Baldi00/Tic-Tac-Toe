using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class SpriteLooper : MonoBehaviour
{
    public Sprite[] spritesList;
    public float changeAfterSeconds = 1;

    private Image image;
    private int currentIndex;
    private float timer;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void OnEnable()
    {
        timer = 0;
        currentIndex = 0;
        image.sprite = spritesList[currentIndex];
        currentIndex = (currentIndex + 1) % spritesList.Length;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeAfterSeconds)
        {
            timer = 0;
            image.sprite = spritesList[currentIndex];
            currentIndex = (currentIndex + 1) % spritesList.Length;
        }
    }

}
