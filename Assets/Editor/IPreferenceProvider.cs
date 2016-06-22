using System.Collections.Generic;

public interface IPreferenceProvider
{
	void SetKeyValue(string valueName, object value);
	void FetchKeyValues (IDictionary<string, object> prefsLookup);
	void ValueField(string valueName, object value);
}