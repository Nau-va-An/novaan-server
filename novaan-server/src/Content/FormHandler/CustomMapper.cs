using System;
using System.Collections;
using System.Net;
using System.Reflection;
using NovaanServer.src.ExceptionLayer.CustomExceptions;

namespace NovaanServer.src.Content.FormHandler
{
    public class CustomMapper
    {
        /// <summary>
        /// Map a value to an object's property
        /// </summary>
        /// <typeparam name="T">Destination object type</typeparam>
        /// <param name="obj">Destination object</param>
        /// <param name="property">Property of destination object</param>
        /// <param name="value">Value that need to be mapped</param>
        public static void MappingObjectData<T>(T? obj, PropertyInfo property, string value)
        {
            Type propertyType = property.PropertyType;
            if (string.IsNullOrEmpty(value))
            {
                throw new NovaanException(
                    ErrorCodes.CONTENT_FIELD_INVALID,
                    HttpStatusCode.BadRequest
                );
            }
            // Handle special cases for enums
            if (propertyType.IsEnum)
            {
                var enumValue = Enum.Parse(propertyType, value);
                property.SetValue(obj, enumValue);
            }

            // Handle special cases for TimeSpan
            else if (propertyType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(value, out var timeSpan))
                {
                    property.SetValue(obj, timeSpan);
                }
            }
            // Handle default case
            else
            {
                var convertedValue = value as IConvertible;
                if (convertedValue != null)
                {
                    var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                    var convertedObject = convertedValue.ToType(targetType, null);
                    property.SetValue(obj, convertedObject);
                }
            }
        }

        /// <summary>
        /// Map a value to an object's property, handle when the value is in a list property
        /// </summary>
        /// <typeparam name="T">Type of destination object</typeparam>
        /// <param name="obj">Destination object</param>
        /// <param name="property">Property of a list inside the destination object</param>
        /// <param name="value">Value to be mapped</param>
        /// <param name="nestedField">Property of nested object that is inside a list</param>
        /// <param name="key">Index of the item inside a list</param>
        /// <exception cref="NovaanException"></exception>
        public static void MappingObjectData<T>(
            T? obj,
            PropertyInfo property,
            string value,
            string? nestedField,
            int key
        )
        {
            Type propertyType = property.PropertyType;

            // Make sure propertyType can be anything that can iterate
            if (propertyType.GetInterface(nameof(IEnumerable)) != null)
            {
                var listItemType = propertyType.GetGenericArguments()[0];
                var list = property.GetValue(obj) as IEnumerable ??
                    throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);

                var listItemProperties = listItemType.GetProperties();

                // get number of items in list
                var listCount = list.GetType().GetProperty("Count")?.GetValue(list, null) as int? ??
                    throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);
                if (key < listCount)
                {
                    var itemAtIndex = list.GetType().GetMethod("get_Item")?.Invoke(list, new object[] { key });

                    // case 1: nestedField is null, value is a primitive type
                    if (nestedField == null)
                    {
                        var convertedValue = Convert.ChangeType(value, itemAtIndex?.GetType());
                        list.GetType().GetMethod("set_Item")?.Invoke(list, new object[] { key, convertedValue });
                        return;
                    }
                    else
                    {
                        // case 2: nestedField is not null, value is not a primitive type
                        var nestedProp = listItemProperties
                            .FirstOrDefault(p => p.Name == nestedField);

                        if (nestedProp == null)
                        {
                            throw new NovaanException(
                                ErrorCodes.CONTENT_FIELD_INVALID,
                                HttpStatusCode.BadRequest
                            );
                        }

                        var convertedValue = Convert.ChangeType(value, nestedProp.PropertyType);
                        nestedProp.SetValue(itemAtIndex, convertedValue);
                        return;
                    }
                }

                // When encounter new list item
                if (key == listCount)
                {

                    object convertedValue;
                    // case 1: nestedField is null, value is a primitive type
                    if (nestedField == null)
                    {
                        convertedValue = Convert.ChangeType(value, listItemType);
                        list.GetType().GetMethod("Add")?.Invoke(list, new object[] { convertedValue });
                        return;
                    }
                    else
                    {
                        var listItem = Activator.CreateInstance(listItemType);
                        // case 2: nestedField is not null, value is not a primitive type
                        var nestedProp = listItemProperties
                            .FirstOrDefault(p => p.Name == nestedField);

                        if (nestedProp == null)
                        {
                            throw new NovaanException(
                                ErrorCodes.CONTENT_FIELD_INVALID,
                                HttpStatusCode.BadRequest
                            );
                        }

                        convertedValue = Convert.ChangeType(value, nestedProp.PropertyType);
                        nestedProp.SetValue(listItem, convertedValue);
                        list.GetType().GetMethod("Add")?.Invoke(list, new object[] { listItem });
                        return;
                    }
                }
            }

            throw new NovaanException(
                ErrorCodes.CONTENT_FIELD_INVALID,
                HttpStatusCode.BadRequest
            );
        }
    }


}

