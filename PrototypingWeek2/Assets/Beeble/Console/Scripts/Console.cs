using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Console
{
    private static List<string> p_lines = new List<string>();
    public static List<string> Lines { get => p_lines; }

    /// <summary>
    /// Writes a line to the console.
    /// </summary>
    /// <param name="line">The string to write.</param>
    public static void WriteLine(string line)
    {
        p_lines.Add(line);
        ConsoleDisplay.Display.AddLine(line);
    }

    /// <summary>
    /// Process the recieved command.
    /// </summary>
    /// <param name="command">The command to process.</param>
    public static void RecieveCommand(string command)
    {
        string call;
        string action;
        string[] snippets = command.Split(' ');

        if (snippets.Length >= 2)
        {
            call = snippets[0];
            action = snippets[1];
            Console.WriteLine("Do a thing.");
        }
        else
        {
            Console.WriteLine("Not enough params.");
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class Command : Attribute
{
    private string call;
    private string action;

    public Command(string command)
    {
        string[] snippets = command.Split(' ');
        if (snippets.Length >= 2)
        {
            call = snippets[0];
            action = snippets[1];
        }
        else
        {
            Console.WriteLine("Not enough params.");
        }
    }
}
