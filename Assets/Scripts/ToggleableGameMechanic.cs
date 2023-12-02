using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Threading.Tasks;
using Random = System.Random;

public class ToggleableGameMechanic
{
    private readonly List<Component> componentsWithToggleableProperties;
    private readonly Random rng;
    private readonly List<GameObject> gameObjectsWithToggleableProperties = new List<GameObject>();

    private GameObject selectedGameObject;
    private Component selectedComponent;
    private PropertyInfo selectedComponentProperty;

    private object initialPropertyValue;

    private String selectedModifier;
    private static readonly String[] modifierTypes = { "double", "half", "invert" };

    private bool isActive;

    public ToggleableGameMechanic(List<Component> componentsWithToggleableProperties, Random rng)
    {
        this.componentsWithToggleableProperties = componentsWithToggleableProperties;
        this.rng = rng;
        foreach (Component component in componentsWithToggleableProperties)
        {
            if (!gameObjectsWithToggleableProperties.Contains(component.gameObject))
            {
                gameObjectsWithToggleableProperties.Add(component.gameObject);
            }
        }
    }

    public void GenerateNew()
    {
        SelectComponent();
        SelectComponentProperty();
        SelectModifier();
    }

    private object GetValue()
    {
        return selectedComponentProperty.GetValue(selectedComponent);
    }

    private void SetValue(object value)
    {
        selectedComponentProperty.SetValue(selectedComponent, value);
    }

    public void Reset()
    {
        try
        {
            SetValue(initialPropertyValue);
        }
        catch (NullReferenceException e)
        {
            Debug.LogWarning($"Tried to reset TGM in an level that's already unloaded. Continuing… ({e})");
        }
    }

    public void Toggle()
    {
        isActive = !isActive;

        String modifierToApply = selectedModifier;
        if (selectedModifier == "double" && isActive == false)
        {
            modifierToApply = "half";
        } else if (selectedModifier == "half" && isActive == false)
        {
            modifierToApply = "double";
        }
        SetValue(ApplyModifier(GetValue(), modifierToApply));
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

    public String SelectGameObject()
    {
        selectedGameObject = gameObjectsWithToggleableProperties[rng.Next(gameObjectsWithToggleableProperties.Count)];
        return GetGameObjectName();
    }

    public String SelectComponent()
    {
        if (selectedGameObject == null)
        {
            selectedComponent = componentsWithToggleableProperties[rng.Next(componentsWithToggleableProperties.Count)];
            selectedGameObject = selectedComponent.gameObject;
        }
        else
        {
            List<Component> selectableComponents = new List<Component>();
            foreach (Component component in componentsWithToggleableProperties)
            {
                if (component.gameObject == selectedGameObject)
                {
                    selectableComponents.Add(component);
                }
            }
            selectedComponent = selectableComponents[rng.Next(selectableComponents.Count)];
        }
        return GetComponentName();
    }

    public String SelectComponentProperty()
    {
        Type componentType = selectedComponent.GetType();
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
                object outputValue = candidateProperty.GetValue(selectedComponent);
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

        selectedComponentProperty = selectedProperty;
        initialPropertyValue = GetValue();

        return GetComponentFieldName();
    }

    public String SelectModifier()
    {
        selectedModifier = SelectModifier(initialPropertyValue, rng);
        return selectedModifier;
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

    private String GetGameObjectName()
    {
        return selectedGameObject.name;
    }

    private String GetComponentName()
    {
        return selectedComponent.GetType().Name;
    }

    private String GetComponentFieldName()
    {
        return selectedComponentProperty.Name;
    }

    private String GetModifier()
    {
        return selectedModifier;
    }

    public Type GetFieldValueType()
    {
        return initialPropertyValue.GetType();
    }

    public override String ToString()
    {
        // WARNING: can only be called while scene is active.
        return Task.Run(async () =>
        {
            try
            {
                await Awaitable.MainThreadAsync();
                return $"{GetGameObjectName()} {GetComponentName()} {GetComponentFieldName()} : {initialPropertyValue} / {ApplyModifier(initialPropertyValue, GetModifier())} ({GetModifier()})";
            }
            catch (MissingReferenceException e)
            {
                return $"Cannot read TGM. Scene in which TGM instance resides is no longer present. {e}";
            }
        }).GetAwaiter().GetResult();
    }
}
