using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlantItem : MonoBehaviour
{
    public PlantObject m_plant;

    public Text m_name_txt;
    public Text m_price_txt;
    public Image m_icon;

    public Image m_btn_image;
    public Text m_btn_txt;

    public AudioClip m_chois_bgm;

    FarmManager m_farm_manager;

    // Start is called before the first frame update
    void Start()
    {
        m_farm_manager = FindObjectOfType<FarmManager>();

        InitializeUI();
    }

    void InitializeUI()
    {
        m_name_txt.text = m_plant.m_plant_name;
        m_price_txt.text = "$ " + m_plant.m_buy_price;
        m_icon.sprite = m_plant.m_icon;
    }

    public void BuyPlant()
    {
        AudioSource bgm = gameObject.AddComponent<AudioSource>();
        bgm.clip = m_chois_bgm;
        bgm.pitch = 2f;
        bgm.Play();

        m_farm_manager.SelectPlant(this);
    }
}
