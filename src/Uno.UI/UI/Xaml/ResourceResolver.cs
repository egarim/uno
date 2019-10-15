﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Uno.UI.DataBinding;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Uno.UI
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class ResourceResolver
	{
		/// <summary>
		/// The master system resources dictionary.
		/// </summary>
		private static ResourceDictionary MasterDictionary => Uno.UI.GlobalStaticResources.MasterDictionary;

		private static readonly Stack<XamlScope> _scopeStack;

		/// <summary>
		/// The current xaml scope for resource resolution.
		/// </summary>
		internal static XamlScope CurrentScope => _scopeStack.Peek();

		static ResourceResolver()
		{
			_scopeStack = new Stack<XamlScope>();
			_scopeStack.Push(XamlScope.Create()); //There should always be a base-level scope (this will be used when no template is being resolved)
		}

		/// <summary>
		/// Performs a one-time, typed resolution of a named resource, using Application.Resources.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="context">Currently unused. In place to have the possibility to modify the lookup mechanism without introducing a binary breaking change.</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static T ResolveResourceStatic<T>(object key, object context = null)
		{
			if (TryStaticRetrieval(key, out var value) && value is T tValue)
			{
				return tValue;
			}

			return default(T);
		}

		/// <summary>
		/// Apply a StaticResource or ThemeResource assignment to a DependencyProperty of a DependencyObject. The assignment will be provisionally
		/// made immediately using Application.Resources if possible, and retried at load-time using the visual-tree scope.
		/// </summary>
		/// <param name="owner">Owner of the property</param>
		/// <param name="property">The property to assign</param>
		/// <param name="resourceKey">Key to the resource</param>
		/// <param name="context">Currently unused. In place to have the possibility to modify the lookup mechanism without introducing a binary breaking change.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void ApplyResource(DependencyObject owner, DependencyProperty property, object resourceKey, bool isThemeResourceExtension, object context = null)
		{
			// Set initial value based on statically-available top-level resources.
			if (TryStaticRetrieval(resourceKey, out var value))
			{
				owner.SetValue(property, value);
			}

			(owner as IDependencyObjectStoreProvider).Store.SetBinding(property, new ResourceBinding(resourceKey, isThemeResourceExtension));
		}

		/// <summary>
		/// Try to retrieve a resource statically (at parse time). This will check resources in 'xaml scope' first, then top-level resources.
		/// </summary>
		private static bool TryStaticRetrieval(object resourceKey, out object value)
		{
			foreach (var source in CurrentScope.Sources)
			{
				var dictionary = (source.Target as FrameworkElement)?.Resources;
				if (dictionary != null && dictionary.TryGetValue(resourceKey, out value))
				{
					return true;
				}
			}

			return TryTopLevelRetrieval(resourceKey, out value);
		}
		/// <summary>
		/// Tries to retrieve a resource from top-level resources (Application-level and system level).
		/// </summary>
		/// <param name="resourceKey">The resource key</param>
		/// <param name="value">Out parameter to which the retrieved resource is assigned.</param>
		/// <returns>True if the resource was found, false if not.</returns>
		private static bool TryTopLevelRetrieval(object resourceKey, out object value)
		{
			return Application.Current.Resources.TryGetValue(resourceKey, out value) ||
				MasterDictionary.TryGetValue(resourceKey, out value);
		}

		/// <summary>
		/// Get a system-level resource with the given key.
		/// </summary>
		internal static T GetSystemResource<T>(object key)
		{
			if (MasterDictionary.TryGetValue(key, out var value) && value is T t)
			{
				return t;
			}

			return default(T);
		}

		/// <summary>
		/// Push a new <see cref="XamlScope"/>, typically because a template is being materialized.
		/// </summary>
		/// <param name="scope"></param>
		internal static void PushNewScope(XamlScope scope) => _scopeStack.Push(scope);
		/// <summary>
		/// Push a new Resources source to the current xaml scope.
		/// </summary>
		/// <param name="source"></param>
		internal static void PushSourceToScope(ManagedWeakReference source)
		{
			var current = _scopeStack.Pop();
			_scopeStack.Push(current.Push(source));
		}
		/// <summary>
		/// Pop Resources source from current xaml scope.
		/// </summary>
		internal static void PopSourceFromScope()
		{
			var current = _scopeStack.Pop();
			_scopeStack.Push(current.Pop());
		}
		/// <summary>
		/// Pop current <see cref="XamlScope"/>, typically because template materialization is complete.
		/// </summary>
		internal static void PopScope()
		{
			_scopeStack.Pop();
			if (_scopeStack.Count == 0)
			{
				throw new InvalidOperationException("Base scope should never be popped.");
			}
		}
	}
}
