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
            if(string.IsNullOrEmpty(value))
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
            string nestedField,
            int key
        )
        {
            Type propertyType = property.PropertyType;

            // Handle special cases for list of objects
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listItemType = propertyType.GetGenericArguments()[0];
                var list = property.GetValue(obj) as IList ??
                    throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);
                var listItemProperties = listItemType.GetProperties();

                if (key < list.Count)
                {
                    var listItem = list[key];
                    var subProperty = listItemProperties
                        .FirstOrDefault(p => p.Name == nestedField);

                    if (subProperty == null)
                    {
                        throw new NovaanException(
                            ErrorCodes.CONTENT_FIELD_INVALID,
                            HttpStatusCode.BadRequest
                        );
                    }

                    var convertedValue = Convert.ChangeType(value, subProperty.PropertyType);
                    subProperty.SetValue(listItem, convertedValue);
                    return;
                }

                // When encounter new list item
                if (key == list.Count)
                {
                    var listItem = Activator.CreateInstance(listItemType);
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
                    nestedProp.SetValue(listItem, convertedValue);
                    list.Add(listItem);
                    return;
                }
            }

            throw new NovaanException(
                ErrorCodes.CONTENT_FIELD_INVALID,
                HttpStatusCode.BadRequest
            );
        }
    }


}

