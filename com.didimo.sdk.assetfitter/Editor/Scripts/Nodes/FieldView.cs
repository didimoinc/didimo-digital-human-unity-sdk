using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Didimo.AssetFitter.Editor.Graph
{
    public abstract class FieldView
    {
        public readonly VisualElement element;
        public readonly object target;
        public readonly FieldInfo fieldInfo;

        public delegate void OnValueChangedHandler(FieldView sender);
        public event OnValueChangedHandler OnValueChanged;

        public FieldView(object target, FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            this.target = target;
            this.element = createElement();
            setStyle();
        }

        protected abstract VisualElement createElement();
        protected VisualElement baseElement<TElement, TValue>() where TElement : BaseField<TValue>
        {
            var field = Activator.CreateInstance(typeof(TElement)) as BaseField<TValue>;
            field.RegisterValueChangedCallback(evt => setValue(field.value));
            field.SetValueWithoutNotify((TValue)fieldInfo.GetValue(target));
            return field;
        }

        protected void setValue(object value)
        {
            fieldInfo.SetValue(target, value);
            OnValueChanged?.Invoke(this);
        }

        void setStyle()
        {
            element.style.flexShrink = 1;
            element.style.flexGrow = 1;
        }

        public static FieldView GetInstance(object target, FieldInfo fieldInfo)
        {
            var type = fieldInfo.FieldType;
            if (type.IsEnum) return new FieldViewEnum(target, fieldInfo);
            else if (type.IsAssignableFrom(typeof(bool))) return new FieldView<Toggle, bool>(target, fieldInfo);
            else if (type.IsAssignableFrom(typeof(string))) return new FieldView<TextField, string>(target, fieldInfo);
            else if (type == typeof(int)) return new FieldView<IntegerField, int>(target, fieldInfo);
            else if (type == typeof(float)) return new FieldView<FloatField, float>(target, fieldInfo);
            else if (type == typeof(Vector2)) return new FieldView<Vector2Field, Vector2>(target, fieldInfo);
            else if (type == typeof(Vector3)) return new FieldView<Vector3Field, Vector3>(target, fieldInfo);
            else if (type == typeof(Vector2Int)) return new FieldView<Vector2IntField, Vector2Int>(target, fieldInfo);
            else if (type == typeof(Vector3Int)) return new FieldView<Vector3IntField, Vector3Int>(target, fieldInfo);
            else if (type.IsSubclassOf(typeof(UnityEngine.Object))) return new FieldObject(target, fieldInfo);
            throw new Exception("Type not support! " + type + " " + type.IsAssignableFrom(typeof(UnityEngine.Object)));
        }
    }

    public class FieldView<TElement, TValue> : FieldView where TElement : BaseField<TValue>
    {
        public FieldView(object target, FieldInfo fieldInfo) : base(target, fieldInfo) { }
        protected override VisualElement createElement() => baseElement<TElement, TValue>();
    }

    public class FieldObject : FieldView<ObjectField, UnityEngine.Object>
    {
        public FieldObject(object target, FieldInfo fieldInfo) : base(target, fieldInfo) { }
        protected override VisualElement createElement()
        {
            var field = base.createElement();
            (field as ObjectField).objectType = fieldInfo.FieldType;
            return field;
        }
    }

    public class FieldViewEnum : FieldView
    {
        public FieldViewEnum(object target, FieldInfo fieldInfo) : base(target, fieldInfo) { }
        protected override VisualElement createElement()
        {
            var flagAtrribute = fieldInfo.FieldType.GetCustomAttribute<System.FlagsAttribute>();
            BaseField<Enum> field = flagAtrribute == null ?
                new EnumField(fieldInfo.GetValue(target) as System.Enum) as BaseField<Enum> :
                new EnumFlagsField(fieldInfo.GetValue(target) as System.Enum) as BaseField<Enum>;

            field.RegisterValueChangedCallback(evt => setValue(field.value));
            return field;
        }
    }
}


// void setValue(object value)
// {
//     fieldInfo.SetValue(node, value);
//     OnExposedValueChanged(fieldInfo);
// }
// VisualElement setFieldStyle(VisualElement element)
// {
//     element.style.flexShrink = 1;
//     element.style.flexGrow = 1;
//     var children = element.Children();
//     return element;
// }

// if (fieldInfo.FieldType.IsEnum)
// {
//     if (fieldInfo.GetCustomAttribute<FlagsAttribute>() == null)
//     {
//         var field = new EnumField(fieldInfo.GetValue(node) as System.Enum);
//         field.RegisterValueChangedCallback(evt => setValue(field.value));
//         return setFieldStyle(field);
//     }
//     else
//     {
//         var field = new EnumFlagsField(fieldInfo.GetValue(node) as System.Enum);
//         field.RegisterValueChangedCallback(evt => setValue(field.value));
//         return setFieldStyle(field);
//     }
// }
// else if (fieldInfo.FieldType == typeof(string))
// {
//     var field = new TextField();
//     field.RegisterValueChangedCallback(evt => setValue(field.value));
//     field.SetValueWithoutNotify(fieldInfo.GetValue(node) as string);
//     return setFieldStyle(field);
// }
// else if (fieldInfo.FieldType == typeof(float))
// {
//     var field = new FloatField();
//     field.RegisterValueChangedCallback(evt => setValue(field.value));
//     field.SetValueWithoutNotify((float)fieldInfo.GetValue(node));
//     return setFieldStyle(field);
// }
// else if (fieldInfo.FieldType == typeof(int))
// {
//     var field = new IntegerField();
//     field.RegisterValueChangedCallback(evt => setValue(field.value));
//     field.SetValueWithoutNotify((int)fieldInfo.GetValue(node));
//     return setFieldStyle(field);
// }
// else if (fieldInfo.FieldType == typeof(bool))
// {
//     var field = new Toggle();
//     field.RegisterValueChangedCallback(evt => setValue(field.value));
//     field.SetValueWithoutNotify((bool)fieldInfo.GetValue(node));
//     return setFieldStyle(field);
// }
// else
// {
//     var field = new ObjectField() { objectType = fieldInfo.FieldType };
//     field.RegisterValueChangedCallback(evt => setValue(field.value));
//     field.SetValueWithoutNotify(fieldInfo.GetValue(node) as UnityEngine.Object);
//     return setFieldStyle(field);
// }
