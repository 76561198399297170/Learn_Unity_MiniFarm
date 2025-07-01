using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmManager : MonoBehaviour
{
    public PlantItem m_select_plant = null;
    public bool m_is_planting = false;

    public int m_money = 8;
    public Text m_money_txt;

    public Color buy_color = Color.green;
    public Color cancel_color = Color.red;

    // Start is called before the first frame update
    void Start()
    {
        m_money_txt.text = "$ " + m_money;
    }

    public void SelectPlant(PlantItem newplant)
    {
        if (m_select_plant == newplant)
        {
            m_select_plant.m_btn_image.color = buy_color;
            m_select_plant.m_btn_txt.text = "buy";

            m_select_plant = null;
            m_is_planting = false;
        }
        else
        {
            if (m_select_plant != null)
            {
                m_select_plant.m_btn_image.color = buy_color;
                m_select_plant.m_btn_txt.text = "buy";
            }
            m_select_plant = newplant;

            m_select_plant.m_btn_image.color = cancel_color;
            m_select_plant.m_btn_txt.text = "cancel";
            
            m_is_planting = true;
        }
    }

    public void Transaction(int value)
    {
        m_money += value;
        m_money_txt.text = "$ " + m_money;
    }

}
