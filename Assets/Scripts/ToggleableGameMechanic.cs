using System;
using System.Collections.Generic;
using System.Linq;
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

    private String modifier;
    private static readonly String[] modifierTypes = { "double", "half", "invert" };

    private bool isActive;

    public ToggleableGameMechanic(List<Component> componentsWithToggleableProperties, Random rng)
    {
        this.componentsWithToggleableProperties = componentsWithToggleableProperties;
        this.rng = rng;
    }

    public void GenerateNew()
    {
        SelectComponent();
        SelectComponentProperty();
        SelectModifier();
    }

    public void GenerateFromGenotype(ToggleGameMechanicGenotype genotype)
    {
        SelectComponent(genotype.gameObjectName, genotype.componentTypeName);
        SelectComponentProperty(genotype.fieldName);
        modifier = genotype.modifier;
    }

    public ToggleGameMechanicGenotype GetTGMGenotype()
    {
        return UnityMainThreadDispatcher.Dispatch(() => new ToggleGameMechanicGenotype(component, componentProperty, modifier));
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
        return ApplyModifier(inputValue, modifier);
    }

    private static object ApplyModifier(object inputValue, String modifier)
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

    private void SelectComponent(String gameObjectName, String componentName)
    {
        component = componentsWithToggleableProperties.Find(component1 =>
            component1.gameObject.name == gameObjectName && component1.name == componentName
        );
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
            if (sampleFlags[nextRandomIndex])
            {
                continue;
            }

            sampleFlags[nextRandomIndex] = true;

            bool isEditableMechanic;
            PropertyInfo candidateProperty = componentProperties[nextRandomIndex];
            try
            {
                object outputValue = candidateProperty.GetValue(component);
                isEditableMechanic =
                    candidateProperty.SetMethod != null && (
                        IsNumeric(outputValue) ||
                        IsBoolean(outputValue) ||
                        IsVector(outputValue) ||
                        IsQuaternion(outputValue)
                    );

                // Check if applying a modifier to this value results in a different value. If it results in the same
                // value, skip this property.
                if (
                    isEditableMechanic && (
                        outputValue.GetHashCode() == ApplyModifier(outputValue, "double").GetHashCode() ||
                        outputValue.GetHashCode() == ApplyModifier(outputValue, "half").GetHashCode() ||
                        outputValue.GetHashCode() == ApplyModifier(outputValue, "invert").GetHashCode()
                    )
                )
                {
                    continue;
                }
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

    private void SelectComponentProperty(String componentPropertyName)
    {
        Type componentType = component.GetType();
        List<PropertyInfo> componentProperties = componentType.GetProperties().ToList();

        componentProperty = componentProperties.Find(selectedProperty => selectedProperty.Name == componentPropertyName);
        defaultValue = GetValue();
    }

    public void SelectModifier()
    {
        modifier = SelectModifier(defaultValue, rng);
    }

    private static String SelectModifier(object v, Random rng)
    {
        if (IsNumeric(v) || IsVector(v))
        {
            return modifierTypes[rng.Next(modifierTypes.Length)];
        }
        if (IsBoolean(v) || IsQuaternion(v))
        {
            return "invert";
        }
        return "invert";
    }

    private static bool IsNumeric(object v)
    {
        return v is float || v is int || v is sbyte || v is short || v is long || v is double || v is decimal;
    }

    private static bool IsVector(object v)
    {
        return v is Vector2 || v is Vector3 || v is Vector4 || v is Vector2Int || v is Vector3Int || v is Color;
    }

    private static bool IsQuaternion(object v)
    {
        return v is Quaternion;
    }

    private static bool IsBoolean(object v)
    {
        return v is bool;
    }

    public override string ToString()
    {
        Type componentType = component.GetType();
        String fieldName = componentProperty.Name;
        return $"{component.name} {componentType.Name} {fieldName} : {defaultValue} / {ApplyModifier(defaultValue)} ({modifier})";
    }
    public readonly struct ToggleGameMechanicGenotype
    {
        public readonly String gameObjectName;
        public readonly String componentTypeName;
        public readonly String fieldName;
        public readonly String modifier;

        public ToggleGameMechanicGenotype(Component component, PropertyInfo componentProperty, String modifier)
        {
            gameObjectName = component.name;
            componentTypeName = component.GetType().Name;
            fieldName = componentProperty.Name;
            this.modifier = modifier;
        }

        public override string ToString()
        {
            return $"TGM genotype: {gameObjectName} {componentTypeName} {fieldName} ({modifier})";
        }
    }
}
