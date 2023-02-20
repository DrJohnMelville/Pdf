namespace Melville.CSJ2K.Util
{
	internal interface IFileInfo
	{
		#region PROPERTIES

		string Name { get; }

		string FullName { get; }

		bool Exists { get; }

		#endregion

		#region METHODS

		bool Delete();

		#endregion
	}
}