using System;
using ICD.Common.Utils;
using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public abstract class AbstractApiInfo : IApiInfo
	{
		/// <summary>
		/// Gets/sets the name for the API attribute.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets/sets the help for the API attribute.
		/// </summary>
		public string Help { get; set; }

		/// <summary>
		/// Gets/sets the response message for this request.
		/// </summary>
		public ApiResult Result { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		protected AbstractApiInfo(IApiAttribute attribute)
		{
			Name = attribute == null ? null : attribute.Name;
			Help = attribute == null ? null : attribute.Help;
		}

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Name", Name);

			return builder.ToString();
		}

		/// <summary>
		/// Creates a copy of the instance containing none of the child nodes.
		/// </summary>
		/// <returns></returns>
		public IApiInfo ShallowCopy()
		{
			AbstractApiInfo instance = Instantiate();
			ShallowCopy(instance);
			return instance;
		}

		/// <summary>
		/// Adds the given item as an immediate child to this node.
		/// </summary>
		/// <param name="child"></param>
		void IApiInfo.AddChild(IApiInfo child)
		{
			AddChild(child);
		}

		/// <summary>
		/// Adds the given item as an immediate child to this node.
		/// </summary>
		/// <param name="child"></param>
		protected abstract void AddChild(IApiInfo child);

		/// <summary>
		/// Copies the current state onto the given instance.
		/// </summary>
		/// <param name="info"></param>
		protected virtual void ShallowCopy(IApiInfo info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			info.Name = Name;
			info.Help = Help;
		}

		/// <summary>
		/// Creates a new instance of the current type.
		/// </summary>
		/// <returns></returns>
		private AbstractApiInfo Instantiate()
		{
			return ReflectionUtils.CreateInstance(GetType()) as AbstractApiInfo;
		}
	}
}
