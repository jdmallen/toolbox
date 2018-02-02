namespace JDMallen.Toolbox.Models
{
    public interface IDomainModel
    {
    }

	public interface IDomainModel<TId> : IDomainModel
	{
		TId Id { get; set; }
	}
}
