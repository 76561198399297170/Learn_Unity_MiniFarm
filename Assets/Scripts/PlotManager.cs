using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotManager : MonoBehaviour
{
    bool m_is_planted = false;//�Ƿ���ֲ����
    SpriteRenderer m_plant;//�������

    int m_plant_stage = 0;//��¼�׶�
    float m_timer = 0f;//��ʱ��

    PlantObject m_selected_plant;

    PolygonCollider2D m_plantCollider;//������ײ��

    FarmManager m_farm_manager;

    public Color available_color = Color.green;
    public Color unavailable_color = Color.red;

    public AudioClip m_do_plant_bgm;
    public AudioClip m_do_harvest_bgm;

    SpriteRenderer m_plot;

    // Start is called before the first frame update
    void Start()
    {
        m_plant = transform.GetChild(0).GetComponent<SpriteRenderer>();
        m_plantCollider = transform.GetChild(0).GetComponent<PolygonCollider2D>();

        m_farm_manager = transform.parent.GetComponent<FarmManager>();
        m_plot = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_is_planted)
        {
            m_timer += Time.deltaTime;

            if (m_timer > m_selected_plant.m_time_stages && m_plant_stage < m_selected_plant.m_plant_stages.Length - 1)
            {
                m_timer = Random.value;
                m_plant_stage++;
                UpdatePlant();
            }
        }
    }

    private void OnMouseDown()
    {
        if (m_is_planted)
        {
            if (m_plant_stage == m_selected_plant.m_plant_stages.Length - 1 && !m_farm_manager.m_is_planting)
            {
                doHarvest();
            }
        }
        else if (m_farm_manager.m_is_planting)
        {
            if (m_farm_manager.m_select_plant.m_plant.m_buy_price <= m_farm_manager.m_money)
            {
                doPlant(m_farm_manager.m_select_plant.m_plant);
            }
        }
    }

    private void OnMouseOver()
    {
        if (m_farm_manager.m_is_planting)
        {
            if (m_is_planted || m_farm_manager.m_select_plant.m_plant.m_buy_price > m_farm_manager.m_money)
            {
                m_plot.color = unavailable_color;
            }
            else
            {
                m_plot.color = available_color;
            }
        }
    }

    private void OnMouseExit()
    {
        m_plot.color = Color.white;
    }

    //�ջ���ֲ�ڸ����ص�����
    void doHarvest()
    {
        AudioSource bgm = gameObject.AddComponent<AudioSource>();
        bgm.clip = m_do_harvest_bgm;
        bgm.volume = 0.75f;
        bgm.Play();

        m_is_planted = false;
        m_plant.gameObject.SetActive(false);

        m_farm_manager.Transaction(m_selected_plant.m_sell_price);
    }

    //��ֲ�����ڸ�����
    void doPlant(PlantObject newplant)
    {
        AudioSource bgm = gameObject.AddComponent<AudioSource>();
        bgm.clip = m_do_plant_bgm;
        bgm.Play();

        m_selected_plant = newplant;

        m_plant_stage = 0;
        m_is_planted = true;

        m_farm_manager.Transaction(-m_selected_plant.m_buy_price);

        UpdatePlant();
        m_timer = 0;
        m_plant.gameObject.SetActive(true);
    }

    //����׶θ���
    void UpdatePlant()
    {
        m_plant.sprite = m_selected_plant.m_plant_stages[m_plant_stage];
    }

}
