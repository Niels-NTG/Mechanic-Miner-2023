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
    public readonly object modifierValue;

    public readonly String operatorType;

    private bool isActive;

    public ToggleableGameMechanic(List<Component> componentsWithToggleableProperties, Random rng)
    {
        component = SelectComponent(componentsWithToggleableProperties, rng);
        componentPropertyInfo = SelectComponentProperty(component, rng);
        Type componentType = component.GetType();
        String fieldName = componentPropertyInfo.Name;
        
        defaultValue = GetValue();

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
        } else if (GetValue() is Vector2)
        {
            operatorType = operatorTypes[rng.Next(operatorTypes.Length)];
            switch (operatorType)
            {
                case "double":
                    modifierValue = (Vector2)defaultValue * 2f;
                    break;
                case "half":
                    modifierValue = (Vector2)defaultValue / 2f;
                    break;
                case "invert":
                    modifierValue = (Vector2)defaultValue * -1f;
                    break;
            }
        }
        
        Debug.Log( component.name + " " + componentType.Name + " " + fieldName + " : " + defaultValue + " / " + modifierValue + " (" + operatorType + ")");
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
        SetValue(newState ? modifierValue : defaultValue);
        isActive = newState;
    }

    public static Component SelectComponent(List<Component> componentsWithToggleableProperties, Random rng)
    {
        return componentsWithToggleableProperties[rng.Next(componentsWithToggleableProperties.Count)];
    }

    public static PropertyInfo SelectComponentProperty(Component component, Random rng)
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

            bool isEditableMechanic;
            PropertyInfo candidateProperty = componentProperties[nextRandomIndex];
            try
            {
                isEditableMechanic =
                    candidateProperty.SetMethod != null && (
                        candidateProperty.GetValue(component) is float ||
                        candidateProperty.GetValue(component) is int ||
                        candidateProperty.GetValue(component) is bool ||
                        candidateProperty.GetValue(component) is Vector2
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
}
