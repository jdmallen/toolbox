using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces
{
	/// <summary>
	/// https://deviq.com/specification-pattern/
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISpecification<T>
	{
		Expression<Func<T, bool>> Criteria { get; }

		IEnumerable<Expression<Func<T, object>>> Includes { get; }

		IEnumerable<string> IncludeStrings { get; }

		Expression<Func<T, object>> OrderBy { get; }

		Expression<Func<T, object>> OrderByDescending { get; }

		int Take { get; }

		int Skip { get; }

		bool IsPagingEnabled { get; }
	}
}
