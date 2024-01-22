using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 데이터를 저장하는 클래스이다.
/// </summary>
[Serializable]
public class PlayerData
{
    /// <summary> 플레이어 기본 체력 </summary>
    public static readonly int max_hp = 3;


    /// <summary> 보유한 아이템 (Index = Code) </summary>
    public List<int> hasItems;

    /// <summary> 보유한 사진 (Index = Code) </summary>
    public List<int> hasCaptures;

    /// <summary> 보유한 조사일지 (Index = Code) </summary>
    public List<int> hasRecords;

    /// <summary> 여주 및 남주 체력 </summary>
    public int player_w_hp, player_m_hp;

    /// <summary> 여주 및 남주 위치 </summary>
    public Vector2 player_w_pos, player_m_pos;

    public PlayerData()
    {
        hasItems = new List<int>();
        hasCaptures = new List<int>();
        hasRecords = new List<int>();

        player_m_hp = player_w_hp = max_hp;
        //player_m_pos = Vector2.zero;
        //player_w_pos = Vector2.zero;
    }


    /// <summary> 'item_code'에 해당하는 아이템을 저장하는 함수이다. </summary>
    /// <param name="item_code">아이템 식별 번호</param>
    public void AddItem(int item_code)
    {
        if (item_code < 0 || item_code >= DataManager.itemNum)
        {
            Debug.LogError("아이템 획득 Error!! : 올바르지 못한 아이템 코드");
            return;
        }

        hasItems.Add(item_code);
    }

    /// <summary> 'capture_code'에 해당하는 사진을 저장하는 함수이다. </summary>
    /// <param name="capture_code">조사 이벤트 식별 번호</param>
    public void AddCapture(int capture_code)
    {
        if (capture_code < 0 || capture_code >= DataManager.captureNum)
        {
            Debug.LogError("사진 획득 Error!! : 올바르지 못한 사진 코드");
            return;
        }

        if (CheckHasCapture(capture_code))
        {
            Debug.Log("이미 확인된 조사 이벤트");
            return;
        }

        hasCaptures.Add(capture_code);
    }

    public bool CheckHasCapture(int capture_code)
        => hasCaptures.IndexOf(capture_code) != -1;

    /// <summary> 'record_code'에 해당하는 조사일지를 저장하는 함수이다. </summary>
    /// <param name="record_code">조사일지 식별 번호</param>
    public void AddRecord(int record_code)
    {
        if (record_code < 0 || record_code >= DataManager.recordNum)
        {
            Debug.LogError("조사일지 획득 Error!! : 올바르지 못한 조사일지 코드");
            return;
        }

        hasRecords.Add(record_code);
    }
}