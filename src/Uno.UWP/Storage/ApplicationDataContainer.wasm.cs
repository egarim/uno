#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Uno.Foundation;
using Uno.Foundation.Interop;
using Uno.Logging;
using Windows.Foundation.Collections;

namespace Windows.Storage
{
	public partial class ApplicationDataContainer
	{
		partial void InitializePartial(ApplicationData owner)
		{
			Values = new FilePropertySet(owner, Locality);
		}

		private class FilePropertySet : IPropertySet
		{
			private readonly ApplicationDataLocality _locality;

			public FilePropertySet(ApplicationData owner, ApplicationDataLocality locality)
			{
				_locality = locality;
			}

			public object? this[string key]
			{
				get
				{
					if (ApplicationDataContainerInterop.TryGetValue(_locality, key, out var value))
					{
						return DataTypeSerializer.Deserialize(value);
					}
					return null;
				}
				set
				{
					if (value != null)
					{
						ApplicationDataContainerInterop.SetValue(_locality, key, DataTypeSerializer.Serialize(value));
					}
					else
					{
						Remove(key);
					}
				}
			}

			public ICollection<string> Keys
			{
				get
				{
					var keys = new List<string>();

					for (int i = 0; i < Count; i++)
					{
						keys.Add(ApplicationDataContainerInterop.GetKeyByIndex(_locality, i));
					}

					return keys.AsReadOnly();
				}
			}

			public ICollection<object> Values
			{
				get
				{
					var values = new List<object>();

					for (int i = 0; i < Count; i++)
					{
						var rawValue = ApplicationDataContainerInterop.GetValueByIndex(_locality, i);
						values.Add(DataTypeSerializer.Deserialize(rawValue));
					}

					return values.AsReadOnly();
				}
			}

			public int Count
				=> ApplicationDataContainerInterop.GetCount(_locality);

			public bool IsReadOnly => false;

			public event MapChangedEventHandler<string, object>? MapChanged;

			public void Add(string key, object value)
			{
				if (ContainsKey(key))
				{
					throw new ArgumentException("An item with the same key has already been added.");
				}
				if (value != null)
				{
					ApplicationDataContainerInterop.SetValue(_locality, key, DataTypeSerializer.Serialize(value));
					MapChanged?.Invoke(this, null);
				}
			}

			public void Add(KeyValuePair<string, object> item)
				=> Add(item.Key, item.Value);

			public void Clear()
			{
				ApplicationDataContainerInterop.Clear(_locality);
			}

			public bool Contains(KeyValuePair<string, object> item)
				=> throw new NotSupportedException();

			public bool ContainsKey(string key)
				=> ApplicationDataContainerInterop.ContainsKey(_locality, key);

			public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
				=> throw new NotSupportedException();

			public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
				=> throw new NotSupportedException();

			public bool Remove(string key)
			{
				var ret = ApplicationDataContainerInterop.Remove(_locality, key);
				return ret;
			}

			public bool Remove(KeyValuePair<string, object> item) => Remove(item.Key);

			public bool TryGetValue(string key, out object? value)
			{
				if (ApplicationDataContainerInterop.TryGetValue(_locality, key, out var innervalue))
				{
					value = DataTypeSerializer.Deserialize(innervalue);
					return true;
				}

				value = null;
				return false;
			}

			IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
		}
	}

	class ApplicationDataContainerInterop
	{
		#region TryGetValue
		internal static bool TryGetValue(ApplicationDataLocality locality, string key, out string? value)
		{
			var parms = new ApplicationDataContainer_TryGetValueParams
			{
				Key = key,
				Locality = locality.ToStringInvariant()
			};

			var ret = TSInteropMarshaller.InvokeJS<ApplicationDataContainer_TryGetValueParams, ApplicationDataContainer_TryGetValueReturn>("UnoStatic_Windows_Storage_ApplicationDataContainer:tryGetValue", parms);

			value = ret.Value;

			return ret.HasValue;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_TryGetValueParams
		{
			public string Key;
			public string Locality;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_TryGetValueReturn
		{
			public string? Value;
			public bool HasValue;
		}
		#endregion

		#region SetValue
		internal static void SetValue(ApplicationDataLocality locality, string key, string value)
		{
			var parms = new ApplicationDataContainer_SetValueParams
			{
				Key = key,
				Value = value,
				Locality = locality.ToStringInvariant()
			};

			TSInteropMarshaller.InvokeJS<ApplicationDataContainer_SetValueParams>("UnoStatic_Windows_Storage_ApplicationDataContainer:setValue", parms);
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_SetValueParams
		{
			public string Key;
			public string Value;
			public string Locality;
		}

		#endregion

		#region ContainsKey
		internal static bool ContainsKey(ApplicationDataLocality locality, string key)
		{
			var parms = new ApplicationDataContainer_ContainsKeyParams
			{
				Key = key,
				Locality = locality.ToStringInvariant()
			};

			var ret = TSInteropMarshaller.InvokeJS<ApplicationDataContainer_ContainsKeyParams, ApplicationDataContainer_ContainsKeyReturn>("UnoStatic_Windows_Storage_ApplicationDataContainer:containsKey", parms);
			return ret.ContainsKey;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_ContainsKeyParams
		{
			public string Key;
			public string Value;
			public string Locality;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_ContainsKeyReturn
		{
			public bool ContainsKey;
		}
		#endregion

		#region GetKeyByIndex
		internal static string GetKeyByIndex(ApplicationDataLocality locality, int index)
		{
			var parms = new ApplicationDataContainer_GetKeyByIndexParams
			{
				Locality = locality.ToStringInvariant(),
				Index = index
			};

			var ret = TSInteropMarshaller.InvokeJS<ApplicationDataContainer_GetKeyByIndexParams, ApplicationDataContainer_GetKeyByIndexReturn>("UnoStatic_Windows_Storage_ApplicationDataContainer:getKeyByIndex", parms);
			return ret.Value;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_GetKeyByIndexParams
		{
			public string Locality;
			public int Index;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_GetKeyByIndexReturn
		{
			public string Value;
		}
		#endregion

		#region GetCount

		internal static int GetCount(ApplicationDataLocality locality)
		{
			var parms = new ApplicationDataContainer_GetCountParams
			{
				Locality = locality.ToStringInvariant()
			};

			var ret = TSInteropMarshaller.InvokeJS<ApplicationDataContainer_GetCountParams, ApplicationDataContainer_GetCountReturn>("UnoStatic_Windows_Storage_ApplicationDataContainer:getCount", parms);
			return ret.Count;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_GetCountParams
		{
			public string Locality;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_GetCountReturn
		{
			public int Count;
		}
		#endregion

		#region Clear

		internal static void Clear(ApplicationDataLocality locality)
		{
			var parms = new ApplicationDataContainer_ClearParams
			{
				Locality = locality.ToStringInvariant()
			};

			TSInteropMarshaller.InvokeJS("UnoStatic_Windows_Storage_ApplicationDataContainer:clear", parms);
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_ClearParams
		{
			public string Locality;
		}

		#endregion

		#region Remove

		internal static bool Remove(ApplicationDataLocality locality, string key)
		{
			var parms = new ApplicationDataContainer_RemoveParams
			{
				Locality = locality.ToStringInvariant(),
				Key = key
			};

			var ret = TSInteropMarshaller.InvokeJS<ApplicationDataContainer_RemoveParams, ApplicationDataContainer_RemoveReturn>("UnoStatic_Windows_Storage_ApplicationDataContainer:remove", parms);
			return ret.Removed;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_RemoveParams
		{
			public string Locality;
			public string Key;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct ApplicationDataContainer_RemoveReturn
		{
			public bool Removed;
		}

		#endregion

		#region GetValueByIndex

		internal static string GetValueByIndex(ApplicationDataLocality locality, int index)
		{
			var parms = new ApplicationDataContainer_GetValueByIndexParams
			{
				Locality = locality.ToStringInvariant(),
				Index = index
			};

			var ret = TSInteropMarshaller.InvokeJS<ApplicationDataContainer_GetValueByIndexParams, ApplicationDataContainer_GetValueByIndexReturn>("UnoStatic_Windows_Storage_ApplicationDataContainer:getValueByIndex", parms);
			return ret.Value;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct ApplicationDataContainer_GetValueByIndexParams
		{
			public string Locality;
			public int Index;
		}

		[TSInteropMessage]
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct ApplicationDataContainer_GetValueByIndexReturn
		{
			public string Value;
		}
		#endregion
	}
}
