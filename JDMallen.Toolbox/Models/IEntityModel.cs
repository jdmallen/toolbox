namespace JDMallen.Toolbox.Models
{
    public interface IEntityModel
    {
    }

	public interface IEntityModel<TId> : IEntityModel
	{
		TId Id { get; set; }
	}
}
