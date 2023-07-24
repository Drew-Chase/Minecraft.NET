// LFInteractive LLC. 2021-2024﻿

namespace Test;

internal class Program
{
    private static async Task Main()
    {
        Console.Write("Test What?\n0 = Modrinth\n1 = CurseForge\n2 = Minecraft\nIndex: ");
        string input = Console.ReadLine();
        switch (input)
        {
            case "0":
                await ModrinthTest.Start();
                break;

            case "1":
                await CurseforgeTest.Start();
                break;

            case "2":
                await MinecraftTest.Start();
                break;
        }
    }
}