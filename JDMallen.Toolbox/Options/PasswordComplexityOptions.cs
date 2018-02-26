namespace JDMallen.Toolbox.Options
{
	public class PasswordComplexityOptions
	{
		private float _bitsThreshold;
		private int _minimumLength;

		public float BitsThreshold
		{
			get => _bitsThreshold <= 0 ? 50F : _bitsThreshold;
			set => _bitsThreshold = value;
		}

		public int MinimumLength
		{
			get => _minimumLength < 2 ? 8 : _minimumLength;
			set => _minimumLength = value;
		}
	}
}
