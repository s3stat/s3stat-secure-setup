using System;
using System.Collections.Generic;

namespace S3stat.SecureSetup.Helpers.Interfaces
{
	public interface IS3Account
	{
		int S3AccountID { get; set; }
		int UserID { get; set; }
		string PublicKey { get; set; }
		string PrivateKey { get; set; }
		string Handle { get; set; }
		bool IsActive { get; set; }
		DateTime CreateDate { get; set; }
		string AWSAccountID { get; set; }

		bool CanAssumeRole { get; set; }
		bool CanCloudWatch { get; set; }
		bool IsValid { get; set; }
		string RoleExternalID { get; set; }

		List<IBucket> Buckets { get; set; }
		List<IDistribution> Distributions { get; set; }


	}
}
