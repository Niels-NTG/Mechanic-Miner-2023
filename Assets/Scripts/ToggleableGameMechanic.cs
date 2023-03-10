using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Random = System.Random;

public class ToggleableGameMechanic
{
    private readonly Component component;
    private readonly PropertyInfo componentPropertyInfo;

    public readonly object defaultValue;

    public readonly String operatorType;

    private bool isActive;
    
    private readonly String[] operatorTypes = { "double", "half", "invert" };

    public ToggleableGameMechanic(List<Component> componentsWithToggleableProperties, Random rng)
    {
        component = SelectComponent(componentsWithToggleableProperties, rng);
        componentPropertyInfo = SelectComponentProperty(component, rng);
        Type componentType = component.GetType();
        String fieldName = componentPropertyInfo.Name;
        
        defaultValue = GetValue();

        operatorType = SetModifier(rng);

        Debug.Log( component.name + " " + componentType.Name + " " + fieldName + " : " + defaultValue + " / " + ApplyModifier(defaultValue) + " (" + operatorType + ")");
    }

    public object GetValue()
    {
        return componentPropertyInfo.GetValue(component);
    }

    public void SetValue(object value)
    {
        componentPropertyInfo.SetValue(component, value);
    }

    public void Toggle()
    {
        Toggle(isActive = !isActive);
    }

    public void Toggle(bool newState)
    {
        object modifierValue = ApplyModifier(defaultValue);
        
        SetValue(newState ? modifierValue : defaultValue);
        isActive = newState;
    }

    private object ApplyModifier(object inputValue)
    {
        if (operatorType == "double")
        {
            switch (inputValue)
            {
                case float value:
                    return value * 2f;
                case int value:
                    return value * 2;
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
            }
        } else if (operatorType == "half")
        {
            switch (inputValue)
            {
                case float value:
                    return value / 2f;
                case int value:
                    return value / 2;
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
            }
        } else if (operatorType == "invert")
        {
            switch (inputValue)
            {
                case bool value:
                    return !value;
                case float value:
                    return value * -1f;
                case int value:
                    return value * -1;
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
            }
        }
        return inputValue;
    }

    public Component SelectComponent(List<Component> componentsWithToggleableProperties, Random rng)
    {
        return componentsWithToggleableProperties[rng.Next(componentsWithToggleableProperties.Count)];
    }

    public PropertyInfo SelectComponentProperty(Component selectedComponent, Random rng)
    {
        Type componentType = selectedComponent.GetType();
        PropertyInfo[] componentProperties = componentType.GetProperties();

        bool[] sampleFlags = new bool[componentProperties.Length];
        Array.Fill(sampleFlags, false);
        
        PropertyInfo selectedProperty = null;
        do
        {
            int nextRandomIndex = rng.Next(componentProperties.Length);
            if (sampleFlags[nextRandomIndex]) continue;
            sampleFlags[nextRandomIndex] = true;

            bool isEditableMechanic;
            PropertyInfo candidateProperty = componentProperties[nextRandomIndex];
            try
            {
                object outputValue = candidateProperty.GetValue(selectedComponent);
                isEditableMechanic =
                    candidateProperty.SetMethod != null && (
                        outputValue is float ||
                        outputValue is int ||
                        outputValue is bool ||
                        outputValue is Vector2 ||
                        outputValue is Vector3 ||
                        outputValue is Vector4 ||
                        outputValue is Vector2Int ||
                        outputValue is Vector3Int
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

        return selectedProperty;
    }

    public String SetModifier(Random rng)
    {
        switch (defaultValue)
        {
            case float:
            case int:
            case Vector2:
            case Vector3:
            case Vector4:
            case Vector2Int:
            case Vector3Int:
                return operatorTypes[rng.Next(operatorTypes.Length)];
            case bool:
                return "invert";
            default:
                return "invert";
        }
    }
}
