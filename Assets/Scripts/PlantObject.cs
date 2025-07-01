using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Plant")]
public class PlantObject : ScriptableObject
{
    public string m_plant_name;//作物名称
    public Sprite m_icon;//作物图标

    public Sprite[] m_plant_stages;//作物精灵渲染器数组
    public float m_time_stages = 2f;//每个阶段切换时长

    public int m_buy_price;//购价
    public int m_sell_price;//售价
}
