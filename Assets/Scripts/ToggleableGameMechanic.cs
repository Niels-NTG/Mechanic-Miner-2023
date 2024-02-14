using System;
using System.Collections.Generic;
using System.Linq;
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
        catch (NullReferenceException)
        { }
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
                    case Vector2[] value:
                        Vector2[] newVector2Values = (Vector2[]) value.Clone();
                        for (int i = 0; i < newVector2Values.Length; i++)
                        {
                            newVector2Values[i] *= 2f;
                        }
                        return newVector2Values;
                    case Vector3[] value:
                        Vector3[] newVector3Values = (Vector3[]) value.Clone();
                        for (int i = 0; i < newVector3Values.Length; i++)
                        {
                            newVector3Values[i] *= 2f;
                        }
                        return newVector3Values;
                    case Vector4[] value:
                        Vector4[] newVector4Values = (Vector4[]) value.Clone();
                        for (int i = 0; i < newVector4Values.Length; i++)
                        {
                            newVector4Values[i] *= 2f;
                        }
                        return newVector4Values;
                    case Vector2Int[] value:
                        Vector2Int[] newVector2IntValues = (Vector2Int[]) value.Clone();
                        for (int i = 0; i < newVector2IntValues.Length; i++)
                        {
                            newVector2IntValues[i] *= 2;
                        }
                        return newVector2IntValues;
                    case Vector3Int[] value:
                        Vector3Int[] newVector3IntValues = (Vector3Int[]) value.Clone();
                        for (int i = 0; i < newVector3IntValues.Length; i++)
                        {
                            newVector3IntValues[i] *= 2;
                        }
                        return newVector3IntValues;
                    case Quaternion value:
                        return Quaternion.Euler(value.eulerAngles * 2f);
                    case Matrix4x4 value:
                        return value * Matrix4x4.Scale(new Vector3(2, 2, 2));
                    case Rect value:
                        return new Rect(
                            value.min - value.size / 4,
                            value.size * 2 - value.size / 2
                        );
                    case RectInt value:
                        return new RectInt(
                            value.min - value.size / 4,
                            value.size * 2 - value.size / 2
                        );
                    case Bounds value:
                        return new Bounds(
                            value.min - value.size / 4,
                            value.size * 2 - value.size / 2
                        );
                    case BoundsInt value:
                        return new BoundsInt(
                            value.position - value.size / 4,
                            value.size * 2 - value.size / 2
                        );
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
                    case Vector2[] value:
                        Vector2[] newVector2Values = (Vector2[]) value.Clone();
                        for (int i = 0; i < newVector2Values.Length; i++)
                        {
                            newVector2Values[i] /= 2f;
                        }
                        return newVector2Values;
                    case Vector3[] value:
                        Vector3[] newVector3Values = (Vector3[]) value.Clone();
                        for (int i = 0; i < newVector3Values.Length; i++)
                        {
                            newVector3Values[i] /= 2f;
                        }
                        return newVector3Values;
                    case Vector4[] value:
                        Vector4[] newVector4Values = (Vector4[]) value.Clone();
                        for (int i = 0; i < newVector4Values.Length; i++)
                        {
                            newVector4Values[i] /= 2f;
                        }
                        return newVector4Values;
                    case Vector2Int[] value:
                        Vector2Int[] newVector2IntValues = (Vector2Int[]) value.Clone();
                        for (int i = 0; i < newVector2IntValues.Length; i++)
                        {
                            newVector2IntValues[i] /= 2;
                        }
                        return newVector2IntValues;
                    case Vector3Int[] value:
                        Vector3Int[] newVector3IntValues = (Vector3Int[]) value.Clone();
                        for (int i = 0; i < newVector3IntValues.Length; i++)
                        {
                            newVector3IntValues[i] /= 2;
                        }
                        return newVector3IntValues;
                    case Quaternion value:
                        return Quaternion.Euler(value.eulerAngles * 0.5f);
                    case Matrix4x4 value:
                        return value * Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 0.5f));
                    case Rect value:
                        return new Rect(
                            value.min + value.size / 4,
                            value.size / 2
                        );
                    case RectInt value:
                        return new RectInt(
                            value.min + value.size / 4,
                            value.size / 2
                        );
                    case Bounds value:
                        return new Bounds(
                            value.min + value.size / 4,
                            value.size / 2
                        );
                    case BoundsInt value:
                        return new BoundsInt(
                            value.position + value.size / 4,
                            value.size / 2
                        );
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
                    case Vector2[] value:
                        Vector2[] newVector2Values = (Vector2[]) value.Clone();
                        for (int i = 0; i < newVector2Values.Length; i++)
                        {
                            newVector2Values[i] *= -1f;
                        }
                        return newVector2Values;
                    case Vector3[] value:
                        Vector3[] newVector3Values = (Vector3[]) value.Clone();
                        for (int i = 0; i < newVector3Values.Length; i++)
                        {
                            newVector3Values[i] *= -1f;
                        }
                        return newVector3Values;
                    case Vector4[] value:
                        Vector4[] newVector4Values = (Vector4[]) value.Clone();
                        for (int i = 0; i < newVector4Values.Length; i++)
                        {
                            newVector4Values[i] *= -1f;
                        }
                        return newVector4Values;
                    case Vector2Int[] value:
                        Vector2Int[] newVector2IntValues = (Vector2Int[]) value.Clone();
                        for (int i = 0; i < newVector2IntValues.Length; i++)
                        {
                            newVector2IntValues[i] *= -1;
                        }
                        return newVector2IntValues;
                    case Vector3Int[] value:
                        Vector3Int[] newVector3IntValues = (Vector3Int[]) value.Clone();
                        for (int i = 0; i < newVector3IntValues.Length; i++)
                        {
                            newVector3IntValues[i] *= -1;
                        }
                        return newVector3IntValues;
                    case Quaternion value:
                        return Quaternion.Inverse(value);
                    case Matrix4x4 value:
                        return value.inverse;
                    case Rect value:
                        return new Rect(value.min, value.size * -1);
                    case RectInt value:
                        return new RectInt(value.min, value.size * -1);
                    case Bounds value:
                        value.SetMinMax(value.max, value.min);
                        return value;
                    case BoundsInt value:
                        value.SetMinMax(value.max, value.min);
                        return value;
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

    public String SelectGameObject(String gameObjectName)
    {
        selectedGameObject = gameObjectsWithToggleableProperties.Find(gameObject =>
            gameObject.name == gameObjectName
        );
        if (selectedGameObject == null)
        {
            Debug.LogError($"No GameObject was found for {gameObjectName} !");
        }
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

    public String SelectComponent(String componentName)
    {
        if (selectedGameObject == null)
        {
            Debug.LogError($"No GameObject was found to find component {componentName} within!");
        }
        selectedComponent = componentsWithToggleableProperties.Find(component =>
            component.gameObject == selectedGameObject && component.GetType().Name == componentName
        );
        if (selectedComponent == null)
        {
            Debug.LogError($"No Component found with the name {componentName} in GameObject {selectedGameObject} !");
        }
        return GetComponentName();
    }

    public String SelectComponentProperty()
    {
        PropertyInfo[] componentProperties = GetComponentType().GetProperties();

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
                        IsQuaternion(outputValue) ||
                        IsMatrix(outputValue) ||
                        IsRect(outputValue)
                    );

                // Check if applying a modifier to this value results in a different value. If it results in the same
                // value, skip this property.
                if (
                    isEditableMechanic && (
                        outputValue.Equals(ApplyModifier(outputValue, "double"))  ||
                        outputValue.Equals(ApplyModifier(outputValue, "half")) ||
                        outputValue.Equals(ApplyModifier(outputValue, "invert"))
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
            selectedProperty == null &&
            Array.TrueForAll(sampleFlags, b => b) == false
        );

        selectedComponentProperty = selectedProperty;
        initialPropertyValue = GetValue();

        return GetComponentFieldName();
    }

    public String SelectComponentProperty(String componentFieldName)
    {
        if (selectedComponent == null)
        {
            Debug.LogError("No component was set for this TGM!");
        }

        List<PropertyInfo> componentProperties = GetComponentType().GetProperties().ToList();
        selectedComponentProperty = componentProperties.Find(property =>
            property.Name == componentFieldName
        );
        initialPropertyValue = GetValue();

        return GetComponentFieldName();
    }

    public String SelectModifier()
    {
        selectedModifier = SelectModifier(initialPropertyValue, rng);
        return GetModifier();
    }

    public String SelectModifier(String modifierName)
    {
        if (!modifierTypes.Contains(modifierName))
        {
            Debug.LogError($"Invalid modifier name {modifierName} !");
        }
        selectedModifier = IsBoolean(initialPropertyValue) ? "invert" : modifierName;
        return GetModifier();
    }

    private static String SelectModifier(object v, Random rng)
    {
        if (IsNumeric(v) || IsVector(v) || IsQuaternion(v) || IsMatrix(v) || IsRect(v))
        {
            return modifierTypes[rng.Next(modifierTypes.Length)];
        }
        if (IsBoolean(v))
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
        return v is Vector2 ||
               v is Vector3 ||
               v is Vector4 ||
               v is Vector2Int ||
               v is Vector3Int ||
               v is Color ||
               v is Vector2[] ||
               v is Vector3[] ||
               v is Vector4[] ||
               v is Vector2Int[] ||
               v is Vector3Int[];
    }

    private static bool IsQuaternion(object v)
    {
        return v is Quaternion;
    }

    private static bool IsMatrix(object v)
    {
        return v is Matrix4x4;
    }

    private static bool IsRect(object v)
    {
        return v is Rect || v is RectInt || v is Bounds || v is BoundsInt;
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

    private Type GetComponentType()
    {
        return selectedComponent.GetType();
    }

    public Type GetFieldValueType()
    {
        return selectedComponentProperty.PropertyType;
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
