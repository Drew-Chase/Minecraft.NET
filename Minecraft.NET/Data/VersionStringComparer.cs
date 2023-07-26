/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

namespace Chase.Minecraft.Data;

public class VersionStringComparer : IComparer<string>
{
    public int Compare(string x, string y)
    {
        string[] versionX = x.Split('.');
        string[] versionY = y.Split('.');

        int maxLength = Math.Max(versionX.Length, versionY.Length);
        for (int i = 0; i < maxLength; i++)
        {
            int partX = i < versionX.Length && int.TryParse(versionX[i], out int parsedX) ? parsedX : 0;
            int partY = i < versionY.Length && int.TryParse(versionY[i], out int parsedY) ? parsedY : 0;

            if (partX != partY)
                return partX.CompareTo(partY);
        }

        return 0;
    }
}