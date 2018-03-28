﻿using System;
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
			return new ApiCommandBuilder(new ApiClassInfo());
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
			m_CurrentClass = new ApiClassInfo();
			m_CurrentNodeGroup.AddNode(key, m_CurrentClass);

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
		/// Adds a get property command to the current node.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IApiClassBuilder GetProperty(string name)
		{
			ApiPropertyInfo property = new ApiPropertyInfo
			{
				Name = name,
				Read = true
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
				Write = true
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
		/// Appends the given methods, parameters, etc to the current class.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public IApiClassBuilder Append(ApiClassInfo info)
		{
			m_CurrentClass.Update(info);
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
		/// Appends the given methods, parameters, etc to the current class.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		IApiClassBuilder Append(ApiClassInfo info);
	}

	public interface IApiNodeGroupBuilder : IApiCommandBuilder
	{
		/// <summary>
		/// Adds the key to the current node group.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		IApiClassBuilder AtKey(uint key);
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
