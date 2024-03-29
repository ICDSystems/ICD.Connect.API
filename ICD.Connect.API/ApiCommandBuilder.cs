﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.API.Info;

namespace ICD.Connect.API
{
	public sealed class ApiCommandBuilder : IApiClassBuilder, IApiMethodBuilder, IApiNodeGroupBuilder
	{
		private readonly ApiClassInfo m_Root;
		private ApiClassInfo m_CurrentClass;
		private ApiMethodInfo m_CurrentMethod;
		private ApiNodeGroupInfo m_CurrentNodeGroup;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="root"></param>
		private ApiCommandBuilder(ApiClassInfo root)
		{
			m_Root = root;
			m_CurrentClass = m_Root;
		}

		#region Factories

		/// <summary>
		/// Starts building a new command.
		/// </summary>
		/// <returns></returns>
		public static IApiClassBuilder NewCommand()
		{
			return UpdateCommand(new ApiClassInfo());
		}

		/// <summary>
		/// Starts building on top of the given command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public static IApiClassBuilder UpdateCommand(ApiClassInfo command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			return new ApiCommandBuilder(command);
		}

		/// <summary>
		/// Builds a CallMethod command with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static ApiClassInfo CallMethodCommand(string name, params object[] parameters)
		{
			IApiMethodBuilder builder = NewCommand().CallMethod(name);

			foreach (object param in parameters)
				builder.AddParameter(param);

			return builder.Complete();
		}

		/// <summary>
		/// Builds a SetProperty command with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static ApiClassInfo SetPropertyCommand(string name, object value)
		{
			return NewCommand().SetProperty(name, value).Complete();
		}

		/// <summary>
		/// Performs a copy of the given sequence of items, excluding anything outside of the path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="root"></param>
		/// <param name="leaf"></param>
		/// <returns></returns>
		public static IEnumerable<IApiInfo> CopyPath(IEnumerable<IApiInfo> path, out ApiClassInfo root, out IApiInfo leaf)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			root = null;
			leaf = null;

			IApiInfo[] shallowCopy = path.Select(i => i.ShallowCopy()).ToArray();

			// Daisy chain the nodes back together
			IApiInfo previous = null;
			foreach (IApiInfo node in shallowCopy)
			{
				if (previous == null)
				{
					root = (ApiClassInfo)node;
				}
				else
				{
					previous.AddChild(node);
					leaf = node;
				}

				previous = node;
			}

			return shallowCopy;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Finishes building and returns the command info.
		/// </summary>
		/// <returns></returns>
		public ApiClassInfo Complete()
		{
			CompleteMethod();

			return m_Root;
		}

		/// <summary>
		/// Creates the node with the given name and moves the builder.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IApiClassBuilder AtNode(string name)
		{
			ApiClassInfo nextClass = new ApiClassInfo();
			ApiNodeInfo node = new ApiNodeInfo
			{
				Name = name,
				Node = nextClass
			};

			m_CurrentClass.AddNode(node);
			m_CurrentClass = nextClass;

			return this;
		}

		/// <summary>
		/// Creates the node group with the given name and moves the builder.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IApiNodeGroupBuilder AtNodeGroup(string name)
		{
			m_CurrentNodeGroup = new ApiNodeGroupInfo
			{
				Name = name
			};

			m_CurrentClass.AddNodeGroup(m_CurrentNodeGroup);

			return this;
		}

		/// <summary>
		/// Adds the key to the current node group.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IApiClassBuilder AtKey(uint key)
		{
			return AddKey(key, new ApiClassInfo());
		}

		/// <summary>
		/// Adds the key and value to the current node group.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public IApiClassBuilder AddKey(uint key, ApiClassInfo value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			m_CurrentClass = value;
			m_CurrentNodeGroup.AddNode(ApiNodeGroupKeyInfo.FromClassInfo(key, value));

			m_CurrentNodeGroup = null;

			return this;
		}

		/// <summary>
		/// Creates the node group with the given name, creates a node at the given index and moves the builder.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public IApiClassBuilder AtNodeGroupKey(string name, uint key)
		{
			return AtNodeGroup(name).AtKey(key);
		}

		/// <summary>
		/// Adds a get node group command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IApiClassBuilder GetNodeGroup(string name)
		{
			ApiNodeGroupInfo nodeGroup = new ApiNodeGroupInfo
			{
				Name = name
			};

			m_CurrentClass.AddNodeGroup(nodeGroup);
			
			return this;
		}

		/// <summary>
		/// Adds a get property command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IApiClassBuilder GetProperty(string name)
		{
			ApiPropertyInfo property = new ApiPropertyInfo
			{
				Name = name,
				ReadWrite = ApiPropertyInfo.eReadWrite.Read
			};

			m_CurrentClass.AddProperty(property);

			return this;
		}

		/// <summary>
		/// Adds a set property command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public IApiClassBuilder SetProperty<T>(string name, T value)
		{
			ApiPropertyInfo property = new ApiPropertyInfo
			{
				Name = name,
				ReadWrite = ApiPropertyInfo.eReadWrite.Write
			};
			property.SetValue(value);

			m_CurrentClass.AddProperty(property);

			return this;
		}

		/// <summary>
		/// Adds a call method command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IApiMethodBuilder CallMethod(string name)
		{
			m_CurrentMethod = new ApiMethodInfo
			{
				Name = name,
				Execute = true
			};
			m_CurrentClass.AddMethod(m_CurrentMethod);

			return this;
		}

		public IApiClassBuilder GetMethod(string name)
		{
			ApiMethodInfo method = new ApiMethodInfo
			{
				Name = name
			};
			m_CurrentClass.AddMethod(method);

			return this;
		}

		/// <summary>
		/// Adds an event subscription command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IApiClassBuilder SubscribeEvent(string name)
		{
			ApiEventInfo eventInfo = new ApiEventInfo
			{
				Name = name,
				SubscribeAction = ApiEventInfo.eSubscribeAction.Subscribe
			};
			m_CurrentClass.AddEvent(eventInfo);

			return this;
		}

		/// <summary>
		/// Adds an event unsubscription command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IApiClassBuilder UnsubscribeEvent(string name)
		{
			ApiEventInfo eventInfo = new ApiEventInfo
			{
				Name = name,
				SubscribeAction = ApiEventInfo.eSubscribeAction.Unsubscribe
			};
			m_CurrentClass.AddEvent(eventInfo);

			return this;
		}

		/// <summary>
		/// Adds the parameter to the current method.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public IApiMethodBuilder AddParameter<T>(T value)
		{
			ApiParameterInfo parameter = new ApiParameterInfo();
			parameter.SetValue(value);

			m_CurrentMethod.AddParameter(parameter);

			return this;
		}

		/// <summary>
		/// Adds the parameter to the current method.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public IApiMethodBuilder AddParameter(Type type, object value)
		{
			ApiParameterInfo parameter = new ApiParameterInfo();
			parameter.SetValue(type, value);

			m_CurrentMethod.AddParameter(parameter);

			return this;
		}

		/// <summary>
		/// Adds the parameter to the current method.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public IApiMethodBuilder AddParameter(object value)
		{
			ApiParameterInfo parameter = new ApiParameterInfo();
			parameter.SetValue(value);

			m_CurrentMethod.AddParameter(parameter);

			return this;
		}

		/// <summary>
		/// Finish adding parameters to the current method and return to the parent node.
		/// </summary>
		/// <returns></returns>
		public IApiClassBuilder CompleteMethod()
		{
			m_CurrentMethod = null;
			return this;
		}

		#endregion
	}

	public interface IApiClassBuilder : IApiCommandBuilder
	{
		/// <summary>
		/// Creates the node with the given name and moves the builder.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IApiClassBuilder AtNode(string name);

		/// <summary>
		/// Creates the node group with the given name and moves the builder.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IApiNodeGroupBuilder AtNodeGroup(string name);

		/// <summary>
		/// Creates the node group with the given name, creates a node at the given index and moves the builder.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		IApiClassBuilder AtNodeGroupKey(string name, uint key);

		/// <summary>
		/// Adds a get node group command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IApiClassBuilder GetNodeGroup(string name);

		/// <summary>
		/// Adds a get property command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IApiClassBuilder GetProperty(string name);

		/// <summary>
		/// Adds a set property command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		IApiClassBuilder SetProperty<T>(string name, T value);

		/// <summary>
		/// Adds a call method command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IApiMethodBuilder CallMethod(string name);

		/// <summary>
		/// Adds a get method command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IApiClassBuilder GetMethod(string name);

		/// <summary>
		/// Adds an event subscription command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IApiClassBuilder SubscribeEvent(string name);

		/// <summary>
		/// Adds an event unsubscription command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IApiClassBuilder UnsubscribeEvent(string name);
	}

	public interface IApiNodeGroupBuilder : IApiCommandBuilder
	{
		/// <summary>
		/// Adds the key to the current node group.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IApiClassBuilder AtKey(uint key);

		/// <summary>
		/// Adds the key and value to the current node group.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		IApiClassBuilder AddKey(uint key, ApiClassInfo value);
	}

	public interface IApiMethodBuilder : IApiCommandBuilder
	{
		/// <summary>
		/// Adds the parameter to the current method.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		IApiMethodBuilder AddParameter<T>(T value);

		/// <summary>
		/// Adds the parameter to the current method.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		IApiMethodBuilder AddParameter(Type type, object value);

		/// <summary>
		/// Adds the parameter to the current method.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		IApiMethodBuilder AddParameter(object value);

		/// <summary>
		/// Finish adding parameters to the current method and return to the parent node.
		/// </summary>
		/// <returns></returns>
		IApiClassBuilder CompleteMethod();
	}

	public interface IApiCommandBuilder
	{
		/// <summary>
		/// Finishes building and returns the command info.
		/// </summary>
		/// <returns></returns>
		ApiClassInfo Complete();
	}
}
