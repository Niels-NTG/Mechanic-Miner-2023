using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Random = System.Random;

public class ToggleableGameMechanic
{
    private readonly List<Component> componentsWithToggleableProperties;
    private readonly Random rng;

    private Component component;
    private PropertyInfo componentProperty;

    private object defaultValue;

    public String modifier;
    private readonly String[] modifierTypes = { "double", "half", "invert" };

    private bool isActive;

    public ToggleableGameMechanic(List<Component> componentsWithToggleableProperties, Random rng)
    {
        this.componentsWithToggleableProperties = componentsWithToggleableProperties;
        this.rng = rng;
        
        GenerateTGM();
        
        Type componentType = component.GetType();
        String fieldName = componentProperty.Name;

        Debug.Log( component.name + " " + componentType.Name + " " + fieldName + " : " + defaultValue + " / " + ApplyModifier(defaultValue) + " (" + modifier + ")");
    }

    private void GenerateTGM()
    {
        SelectComponent();
        SelectComponentProperty();

        SelectModifier();
    }

    private object GetValue()
    {
        return componentProperty.GetValue(component);
    }

    private void SetValue(object value)
    {
        componentProperty.SetValue(component, value);
    }

    public void Toggle()
    {
        Toggle(isActive = !isActive);
    }

    private void Toggle(bool newState)
    {
        object modifierValue = ApplyModifier(defaultValue);
        
        SetValue(newState ? modifierValue : defaultValue);
        isActive = newState;
    }

    private object ApplyModifier(object inputValue)
    {
        switch (modifier)
        {
            case "double":
                switch (inputValue)
                {
                    case bool value:
                        return !value;
                    case float value:
                        return value * 2f;
                    case int value:
                        return value * 2;
                    case sbyte value:
                        return value * 2;
                    case short value:
                        return value * 2;
                    case long value:
                        return value * 2;
                    case double value:
                        return value * 2d;
                    case decimal value:
                        return value * 2m;
                    case Vector2 value:
                        return value * 2f;
                    case Vector3 value:
                        return value * 2f;
                    case Vector4 value:
                        return value * 2f;
                    case Vector2Int value:
                        return value * 2;
                    case Vector3Int value:
                        return value * 2;
                    case Quaternion value:
                        return Quaternion.Inverse(value);
                }
                break;
            case "half":
                switch (inputValue)
                {
                    case bool value:
                        return !value;
                    case float value:
                        return value / 2f;
                    case int value:
                        return value / 2;
                    case sbyte value:
                        return value / 2;
                    case short value:
                        return value / 2;
                    case long value:
                        return value / 2;
                    case double value:
                        return value / 2d;
                    case decimal value:
                        return value / 2m;
                    case Vector2 value:
                        return value / 2f;
                    case Vector3 value:
                        return value / 2f;
                    case Vector4 value:
                        return value / 2f;
                    case Vector2Int value:
                        return value / 2;
                    case Vector3Int value:
                        return value / 2;
                    case Quaternion value:
                        return Quaternion.Inverse(value);
                }
                break;
            case "invert":
                switch (inputValue)
                {
                    case bool value:
                        return !value;
                    case float value:
                        return value * -1f;
                    case int value:
                        return value * -1;
                    case sbyte value:
                        return value * -1;
                    case short value:
                        return value * -1;
                    case long value:
                        return value * -1;
                    case double value:
                        return value * -1d;
                    case decimal value:
                        return value * -1m;
                    case Vector2 value:
                        return value * -1f;
                    case Vector3 value:
                        return value * -1f;
                    case Vector4 value:
                        return value * -1f;
                    case Vector2Int value:
                        return value * -1;
                    case Vector3Int value:
                        return value * -1;
                    case Quaternion value:
                        return Quaternion.Inverse(value);
                }
                break;
        }
        return inputValue;
    }

    private void SelectComponent()
    {
        component = componentsWithToggleableProperties[rng.Next(componentsWithToggleableProperties.Count)];
    }

    public void SelectComponentProperty()
    {
        Type componentType = component.GetType();
        PropertyInfo[] componentProperties = componentType.GetProperties();

        bool[] sampleFlags = new bool[componentProperties.Length];
        Array.Fill(sampleFlags, false);
        
        PropertyInfo selectedProperty = null;
        do
        {
            int nextRandomIndex = rng.Next(componentProperties.Length);
            if (sampleFlags[nextRandomIndex]) continue;
            sampleFlags[nextRandomIndex] = true;
            // TODO only select field to which the same type of modifier can be applied,
            //      otherwise we risk applying an invalid modifier.
            
            bool isEditableMechanic;
            PropertyInfo candidateProperty = componentProperties[nextRandomIndex];
            try
            {
                object outputValue = candidateProperty.GetValue(component);
                isEditableMechanic =
                    candidateProperty.SetMethod != null && (
                        outputValue is float ||
                        outputValue is int ||
                        outputValue is bool ||
                        outputValue is sbyte ||
                        outputValue is short ||
                        outputValue is long ||
                        outputValue is double ||
                        outputValue is decimal ||
                        outputValue is Vector2 ||
                        outputValue is Vector3 ||
                        outputValue is Vector4 ||
                        outputValue is Vector2Int ||
                        outputValue is Vector3Int ||
                        outputValue is Quaternion
                    );
            }
            catch (NotSupportedException)
            {
                continue;
            }
            catch (TargetInvocationException)
            {
                continue;
            }
            if (isEditableMechanic)
            {
                selectedProperty = candidateProperty;    
            }
        } while (
            selectedProperty == null ||
            Array.TrueForAll(sampleFlags, b => b) == false
        );

        componentProperty = selectedProperty;
        defaultValue = GetValue();
    }
    
    public void SelectModifier()
    {
        modifier = _SelectModifier();
    }

    private String _SelectModifier()
    {
        switch (defaultValue)
        {
            case float:
            case int:
            case sbyte:
            case short:
            case long:
            case double:
            case decimal:
            case Vector2:
            case Vector3:
            case Vector4:
            case Vector2Int:
            case Vector3Int:
                return modifierTypes[rng.Next(modifierTypes.Length)];
            case bool:
            case Quaternion:
                return "invert";
            default:
                return "invert";
        }
    }
}
