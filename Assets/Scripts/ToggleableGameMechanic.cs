using System;
using UnityEngine;
using System.Reflection;
using Random = System.Random;

public class ToggleableGameMechanic
{
    private readonly Component component;
    private readonly FieldInfo componentFieldInfo;

    public readonly object defaultValue;
    public readonly object modifierValue;

    public readonly String operatorType;

    private bool isActive;

    ToggleableGameMechanic(Component component, String fieldName, Random rng)
    {
        Type componentType = component.GetType();
        componentFieldInfo = componentType.GetField(fieldName);

        defaultValue = GetValue();
        Debug.Log(defaultValue);

        String[] operatorTypes = { "double", "half", "invert" };
        if (GetValue() is float)
        {
            operatorType = operatorTypes[rng.Next(operatorTypes.Length)];
            switch (operatorType)
            {
                case "double":
                    modifierValue = (float)defaultValue * 2f;
                    break;
                case "half":
                    modifierValue = (float)defaultValue / 2f;
                    break;
                case "invert":
                    modifierValue = (float)defaultValue * -1f;
                    break;
            }
        } else if (GetValue() is int)
        {
            operatorType = operatorTypes[rng.Next(operatorTypes.Length)];
            switch (operatorType)
            {
                case "double":
                    modifierValue = (int)defaultValue * 2f;
                    break;
                case "half":
                    modifierValue = (int)defaultValue / 2f;
                    break;
                case "invert":
                    modifierValue = (int)defaultValue * -1f;
                    break;
            }
        } else if (GetValue() is bool)
        {
            operatorType = "invert";
            modifierValue = !(bool)defaultValue;
        }
        else
        {
            Debug.Log("Given field is not an int, float or bool type");
            // TODO maybe throw an error here?   
        }
    }

    public object GetValue()
    {
        return componentFieldInfo.GetValue(component);
    }

    public void SetValue(object value)
    {
        componentFieldInfo.SetValue(component, value);
    }

    public void Toggle()
    {
        Toggle(isActive = !isActive);
    }

    public void Toggle(bool newState)
    {
        SetValue(newState ? modifierValue : defaultValue);
        isActive = newState;
    }
}