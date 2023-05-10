using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public int hp = 150;

    public Slider healthSlider;
    public TMPro.TextMeshProUGUI healthText;
    public Image healthImage;
    public Image damageOverlay;

    //private Slider dashSlider;
    //public TMPro.TextMeshProUGUI dashText;
    //public Image dashImage;

    //private Slider grapplingHookSlider;
    //public TMPro.TextMeshProUGUI grapplingHookText;
    //public Image grapplingHookImage;

    public TMPro.TextMeshProUGUI enemiesText;

    private Color green = Color.green;
    private Color yellow = Color.yellow;
    private Color red = Color.red;


    // Start is called before the first frame update
    void Start()
    {
        setHp(hp);
        damageOverlay.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (damageOverlay.enabled)
        {
            Color color = damageOverlay.color;
            color.a -= 0.2f * Time.deltaTime;
            damageOverlay.color = color;
            if (color.a <= 0f)
            {
                damageOverlay.enabled = false;
                color.a = 1f;
                damageOverlay.color = color;
            }
        }
    }

    public void setHp(int hp)
    {
        if (hp < this.hp)
        {
            Color color = damageOverlay.color;
            color.a = 1f;
            damageOverlay.color = color;
            damageOverlay.enabled = true;
        }

        this.hp = hp;
        healthSlider.value = (float)hp;
        healthText.text = hp.ToString() + " HP";

        if (hp < 50)
        {
            healthImage.color = red;
        }
        else if (hp < 100)
        {
            healthImage.color = yellow;
        }
        else
        {
            healthImage.color = green;
        }
    }

    public void DecreaseEnemyCount()
    {
        enemiesText.text = (int.Parse(enemiesText.text) - 1).ToString();
    }
}
