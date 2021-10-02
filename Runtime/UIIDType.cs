using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIIDType
{
    static int TotalIDCount = 0;
    int _uiNo = 0;
    public UIIDType()
    {
        _uiNo = TotalIDCount;
        TotalIDCount++;
    }
    public static void Init() => TotalIDCount = 0;
}