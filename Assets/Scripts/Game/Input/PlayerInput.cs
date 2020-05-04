using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput
{

    public int ControllerIndex;

    public readonly static string[] ButtonNames = { "Start", "A", "B", "X", "Y", "Z", "L", "R" };
    private readonly static string controllerConstant = "_C";

    public PlayerInput(int controller)
    {
        if (controller < 1)
        {
            throw new System.ArgumentException("Not given a valid controller!");
        }
        ControllerIndex = controller;
    }

    public static string Button(int index, int controller)
    {
        if (index >= ButtonNames.Length || index < 0)
        {
            throw new System.IndexOutOfRangeException("Not a valid button index!");
        }

        return ButtonNames[index] + controllerConstant + controller;
    }

    public static string Button(string button, int controller)
    {
        if (!System.Array.Exists(ButtonNames, x => x == button))
        {
            throw new System.ArgumentException("Not a valid button string!");
        }
        return button + controllerConstant + controller;
    }

    public static string Axis(string axis, int controller)
    {
        return axis + controllerConstant + controller;
    }

    public string Button(string button)
    {
        return Button(button, ControllerIndex);
    }

    public string Button(int index)
    {
        return Button(index, ControllerIndex);
    }

    public string Axis(string axis)
    {
        return Axis(axis, ControllerIndex);
    }
}
