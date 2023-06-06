using System;
namespace NovaanServer.Developer
{
	public interface IDevService
	{
		public bool IsServerHealthy();

		public bool IsDatabaseHealthy();

		public bool IsS3Healthy();
	}
}

