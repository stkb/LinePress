using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.Linq;

namespace LinePress.Options
{
   public class SettingAttribute : Attribute
   { }

   public interface ISettings
   {
      string Name { get; }
   }

   public static class SettingsStore
   {
      private static readonly WritableSettingsStore store
          = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetWritableSettingsStore(SettingsScope.UserSettings);

      public static event Action SettingsChanged;

      public static void SaveSettings(ISettings settings)
      {
         try
         {
            if (store.CollectionExists(settings.Name) != true)
               store.CreateCollection(settings.Name);

            var anySaved = false;

            var type = settings.GetType();

            foreach (var prop in type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(SettingAttribute))))
            {
               if (prop.PropertyType == typeof(bool))
               {
                  store.SetBoolean(settings.Name, prop.Name, ((bool)(prop.GetValue(settings))));
                  anySaved = true;
               }
               else if (prop.PropertyType == typeof(int))
               {
                  store.SetInt32(settings.Name, prop.Name, ((int)(prop.GetValue(settings))));
                  anySaved = true;
               }
               else if (prop.PropertyType == typeof(double))
               {
                  store.SetString(settings.Name, prop.Name, prop.GetValue(settings).ToString());
                  anySaved = true;
               }
               else if (prop.PropertyType == typeof(string))
               {
                  store.SetString(settings.Name, prop.Name, ((string)(prop.GetValue(settings))));
                  anySaved = true;
               }
            }

            if (anySaved)
               SettingsChanged?.Invoke();
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }

      public static void LoadSettings(ISettings settings)
      {
         try
         {
            if (store.CollectionExists(settings.Name) != true)
               return;

            var type = settings.GetType();

            foreach (var prop in type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(SettingAttribute))))
            {
               if (prop.PropertyType == typeof(bool))
               {
                  if (store.PropertyExists(settings.Name, prop.Name))
                     prop.SetValue(settings, store.GetBoolean(settings.Name, prop.Name));
               }
               else if (prop.PropertyType == typeof(int))
               {
                  if (store.PropertyExists(settings.Name, prop.Name))
                     prop.SetValue(settings, store.GetInt32(settings.Name, prop.Name));
               }
               else if (prop.PropertyType == typeof(double))
               {
                  if (store.PropertyExists(settings.Name, prop.Name))
                  {
                     double.TryParse(store.GetString(settings.Name, prop.Name), out double value);
                     prop.SetValue(settings, value);
                  }
               }
               else if (prop.PropertyType == typeof(string))
               {
                  if (store.PropertyExists(settings.Name, prop.Name))
                     prop.SetValue(settings, store.GetString(settings.Name, prop.Name));
               }
            }
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }
   }
}