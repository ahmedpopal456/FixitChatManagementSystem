using Fixit.Core.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Fixit.Chat.Management.Lib.Extensions
{
  public static class HttpContentExtensions
  {
    public static bool DeserializeToObject<T>(this HttpContent httpContent, out T validatedObject) where T : IDtoValidator
    {
      bool isValid = false;
      validatedObject = default(T);

      try
      {
        var deserializedObject = JsonConvert.DeserializeObject<T>(httpContent.ReadAsStringAsync().Result);
        if (deserializedObject != null && deserializedObject.Validate())
        {
          validatedObject = deserializedObject;
          isValid = true;
        }
      }
      catch
      {
        // Fall through 
      }
      return isValid;
    }

    public static bool DeserializeToObjectList<T>(this HttpContent httpContent, out IEnumerable<T> validatedObjects) where T : IDtoValidator
    {
      bool isValid = true;
      var validatedObjectsList = default(List<T>);

      try
      {
        var deserializedObjects = JsonConvert.DeserializeObject<IEnumerable<T>>(httpContent.ReadAsStringAsync().Result);
        foreach(var deserializedObject in deserializedObjects)
        {
          if (deserializedObject is { } && deserializedObject.Validate())
          {
            validatedObjectsList ??= new List<T>();
            validatedObjectsList.Add(deserializedObject);
          }
          else
          {
            isValid = false;
            break;
          }
        }
      }
      catch
      {
        isValid = false; 
      }

      validatedObjects = validatedObjectsList;
      return isValid;
    }
  }
}
