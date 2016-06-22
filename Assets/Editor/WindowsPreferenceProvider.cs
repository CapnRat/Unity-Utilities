using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using UnityEditor;
using System.Text;

public class WindowsPreferenceProvider : IPreferenceProvider
{
	private const string kUnityRootSubKey = "Software\\Unity Technologies\\Unity Editor 5.x\\";

	public void SetKeyValue(string valueName, object newValue)
	{
		if (valueName == null)
			throw new ArgumentNullException("valueName");

		if (newValue == null)
			throw new ArgumentNullException("newValue");

		using (RegistryKey key = Registry.CurrentUser.OpenSubKey(kUnityRootSubKey, true))
		{
			if (key == null)
				throw new KeyNotFoundException(string.Format("Failed to open sub key {0}.", kUnityRootSubKey));

			// Unity caches values, so it doesn't dip into the registry for every EditorPrefs.Get* call.
			// This means we need to tell Unity to delete this value to remove it from the cache and force Unity to look into registry for value.
			EditorPrefs.DeleteKey(NicifyValueName(valueName));

			key.SetValue(valueName, newValue);
		}
	}

	public void FetchKeyValues(IDictionary<string, object> prefsLookup)
	{
		using (RegistryKey key = Registry.CurrentUser.OpenSubKey(kUnityRootSubKey, false))
		{
			if (key == null)
				throw new KeyNotFoundException(string.Format("Failed to open sub key {0}.", kUnityRootSubKey));

			prefsLookup.Clear();

			foreach (string keyValueName in key.GetValueNames())
			{
				var value = key.GetValue(keyValueName);
				prefsLookup.Add(keyValueName, value);
			}
		}
	}

	public void ValueField(string valueName, object value)
	{

		// Strings are encoded as utf8 bytes
		var bytes = value as byte[];
		if (bytes != null)
		{
			string valueAsString = Encoding.UTF8.GetString(bytes);
			EditorGUI.BeginChangeCheck();
			string newString = EditorGUILayout.DelayedTextField(NicifyValueName(valueName), valueAsString);
			if (EditorGUI.EndChangeCheck())
			{
				value = Encoding.UTF8.GetBytes(newString);
			}
		}
		else if (value is int)
		{
			int valueAsInt = (int)value;
			EditorGUI.BeginChangeCheck();
			int newInt = EditorGUILayout.DelayedIntField(NicifyValueName(valueName), valueAsInt);
			if (EditorGUI.EndChangeCheck())
			{
				value = newInt;
			}
		}
		else
		{
			EditorGUILayout.LabelField(NicifyValueName(valueName), string.Format("Unhandled Type {0}", value.GetType()));
		}
	}

	private string NicifyValueName(string keyValueName)
	{
		return keyValueName.Split(new[] { "_h" }, StringSplitOptions.None).First();
	}
}