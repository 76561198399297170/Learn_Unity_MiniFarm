using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Plant")]
public class PlantObject : ScriptableObject
{
    public string m_plant_name;//��������
    public Sprite m_icon;//����ͼ��

    public Sprite[] m_plant_stages;//���ﾫ����Ⱦ������
    public float m_time_stages = 2f;//ÿ���׶��л�ʱ��

    public int m_buy_price;//����
    public int m_sell_price;//�ۼ�
}
