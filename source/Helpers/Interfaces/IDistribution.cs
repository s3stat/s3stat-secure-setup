using System;
using System.Data.SqlTypes;

namespace S3stat.SecureSetup.Helpers.Interfaces
{
	public interface IDistribution : ILoggable
	{
		int DistributionID { get; set; }
		SqlInt32 BucketID { get; set; }
		string AWSDistributionID { get; set; }
		bool IsActive { get; set; }
		bool IsLogging { get; set; }
		bool IsEC2 { get; set; }
		bool IsEU { get; set; }
		bool IsStreaming { get; set; }
		bool IsReadable { get; set; }

		void SetDefaults();
	}
}
