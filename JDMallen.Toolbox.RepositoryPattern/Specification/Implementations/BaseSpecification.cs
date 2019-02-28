using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;

namespace JDMallen.Toolbox.EFCore.Patterns.Specification.Implementations
{
	/// <summary>
	/// https://deviq.com/specification-pattern/
	/// </summary>
	public class BaseSpecification<T> : ISpecification<T>
	{
		private readonly List<Expression<Func<T, object>>> _includes = new List<Expression<Func<T, object>>>();
		private readonly List<string> _includeStrings = new List<string>();

		public BaseSpecification(Expression<Func<T, bool>> criteria)
		{
			Criteria = criteria;
		}

		public int Take { get; private set; }

		public int Skip { get; private set; }

		public bool IsPagingEnabled { get; private set; } = false;

		public Expression<Func<T, object>> OrderBy { get; private set; }

		public Expression<Func<T, object>> OrderByDescending
		{
			get;
			private set;
		}

		public Expression<Func<T, bool>> Criteria { get; }

		public IEnumerable<Expression<Func<T, object>>> Includes => _includes?.AsReadOnly();

		public IEnumerable<string> IncludeStrings => _includeStrings?.AsReadOnly();

		protected virtual void AddInclude(
			Expression<Func<T, object>> includeExpression)
		{
			_includes.Add(includeExpression);
		}

		// string-based includes allow for including children of children,
		// e.g. Basket.Items.Product
		protected virtual void AddInclude(string includeString)
		{
			_includeStrings.Add(includeString);
		}

		protected virtual void ApplyPaging(int skip, int take)
		{
			Skip = skip;
			Take = take;
			IsPagingEnabled = true;
		}

		protected virtual void ApplyOrderBy(
			Expression<Func<T, object>> orderByExpression)
		{
			OrderBy = orderByExpression;
		}

		protected virtual void ApplyOrderByDescending(
			Expression<Func<T, object>> orderByDescendingExpression)
		{
			OrderByDescending = orderByDescendingExpression;
		}
	}
}
