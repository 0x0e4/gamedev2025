using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public TMP_Text ammo;
    public TMP_Text use;
    public TMP_Text level;
    public TMP_Text hp;
    public TMP_Text slowTimeCount;
    public TMP_Text escapeFailed;
    public TMP_Text firstGame;

    public void SetAmmo(int clips, int ammoCount)
    {
        ammo.text = clips + "/" + ammoCount;
    }

    public void SetLevel(int lvl)
    {
        level.text = "Уровень " + lvl;
    }

    public void SetHP(int curHp)
    {
        hp.text = "HP " + curHp;
    }

    public void SetSlowTimeCount(float slowTime)
    {
        slowTimeCount.text = "Deadeye " + slowTime.ToString("0.0");
    }

    void Update()
    {
        
    }
}
