﻿using HarmonyLib;
using UnityEngine;

namespace TrombLoader.Class_Patches;

[HarmonyPatch(typeof(LevelSelectController))]
[HarmonyPatch(nameof(LevelSelectController.incrementIndexCounter))]
public class LevelSelectControllerPatch
{
    static bool Prefix(ref int __result, LevelSelectController __instance, ref int indexcounter, ref int addnum)
    {
        if (GlobalVariables.localsave.tracks_played == 0)
        {
            __result = 0;
            return false;
        }
        int num = GlobalVariables.data_tracktitles.Length - 1;

        indexcounter += addnum;
        if (indexcounter > num)
        {
            indexcounter = 0;
        }
        else if (indexcounter < 0)
        {
            indexcounter = num;
        }

        __result = indexcounter;
        return false;
    }
}

[HarmonyPatch(typeof(LevelSelectController))]
[HarmonyPatch(nameof(LevelSelectController.advanceSongs))]
public class LevelSelectControllerAdvanceSongsPatch
{
    static bool Prefix(LevelSelectController __instance, ref int dir)
    {
        __instance.doSfx(__instance.sfx_click);
        __instance.lastindex = __instance.songindex;
        int num;
        if (dir > 0)
        {
            num = 1;
        }
        else
        {
            num = -1;
        }
        float num2;
        if (dir < 0)
        {
            num2 = 60f;
        }
        else
        {
            num2 = -60f;
        }
        float xmult = 0.4f;
        __instance.btnspanelr.anchoredPosition3D = new Vector3(num2 * xmult, num2, 0f);
        LeanTween.value(num2, 0f, 0.08f).setEaseOutQuart().setOnUpdate(delegate(float val)
        {
            __instance.btnspanelr.anchoredPosition3D = new Vector3(val * xmult, val, 0f);
        });
        LeanTween.value(-60f, -30f, 0.15f).setEaseOutQuart().setOnUpdate(delegate(float val)
        {
            __instance.pointerarrow.anchoredPosition3D = new Vector3(val, 0f, 0f);
        });
        int num3 = GlobalVariables.data_tracktitles.Length - 1;
        for (int i = 0; i < Mathf.Abs(dir); i++)
        {
            __instance.songindex += num;
            if (__instance.songindex > num3)
            {
                __instance.songindex = 0;
            }
            else if (__instance.songindex < 0)
            {
                __instance.songindex = num3;
            }
        }
        if (GlobalVariables.localsave.tracks_played == 0)
        {
            __instance.songindex = 0;
            __instance.lastindex = 0;
        }
        __instance.populateSongNames();
        Debug.Log(__instance.lastindex + "," + __instance.songindex);
        return false;
    }
}

[HarmonyPatch(typeof(LevelSelectController))]
[HarmonyPatch(nameof(LevelSelectController.Start))]
public class LevelSelectStartPatch
{
    public static bool isActuallyTromboneChamp = false;
    //This looks ugly, but saves us from using a transpiler
    static void Prefix(LevelSelectController __instance)
    {
        isActuallyTromboneChamp = GlobalVariables.localsave.progression_trombone_champ;
        GlobalVariables.localsave.progression_trombone_champ = true;
    }
    
    static void Postfix(LevelSelectController __instance)
    {
        if (!isActuallyTromboneChamp) GlobalVariables.localsave.progression_trombone_champ = false;
    }
}

