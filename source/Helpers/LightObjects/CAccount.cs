using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using S3stat.SecureSetup.Helpers.Interfaces;

namespace S3stat.SecureSetup.Helpers.LightObjects
{
	public class CAccount : IS3Account
	{
		private List<CBucket> cBuckets;

		#region properties

		public int S3AccountID { get; set; }

		public int UserID { get; set; }

		public string PublicKey { get; set; }

		public string PrivateKey { get; set; }

		public string Handle { get; set; }

		public bool IsActive { get; set; }

		public DateTime CreateDate { get; set; }

		public string AWSAccountID { get; set; }

		public bool CanAssumeRole { get; set; }
		public string RoleExternalID { get; set; }

		[XmlIgnore]
		public List<IBucket> Buckets { get; set; }

		public List<CBucket> CBuckets
		{
			get { return cBuckets; }
			set { cBuckets = value; }
		}

		[XmlIgnore]
		public List<IDistribution> Distributions { get; set; }

		public List<CDistribution> CDistributions { get; set; }

		#endregion

		public CAccount()
		{
			cBuckets = new List<CBucket>();
		}
		public CAccount(IS3Account account)
		{
			S3AccountID = account.S3AccountID;
			CreateDate = account.CreateDate;
			IsActive = account.IsActive;
			Handle = account.Handle;
			PrivateKey = account.PrivateKey;
			PublicKey = account.PublicKey;
			UserID = account.UserID;
			CanAssumeRole = account.CanAssumeRole;
			RoleExternalID = account.RoleExternalID;

			Buckets = account.Buckets;
			Distributions = account.Distributions;

			cBuckets = new List<CBucket>(account.Buckets.Count);
			foreach (IBucket bucket in account.Buckets)
			{
				cBuckets.Add(new CBucket(bucket));
			}

			CDistributions = new List<CDistribution>(account.Distributions.Count);
			foreach (IDistribution bucket in account.Distributions)
			{
				CDistributions.Add(new CDistribution(bucket));
			}
		}
	}
}
