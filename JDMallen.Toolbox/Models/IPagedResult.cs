using System.Collections.Generic;

namespace JDMallen.Toolbox.Models
{
	/// <summary>
	/// Represents a result that has been divided into pages for ease of displaying to end users.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	public interface IPagedResult<TModel> where TModel : IModel
	{
		int Skipped { get; set; }

		int Taken { get; set; }

		long TotalItemCount { get; set; }

		int PageIndex { get; }

		int TotalPageCount { get; }

		IEnumerable<TModel> Items { get; set; }
	}
}
