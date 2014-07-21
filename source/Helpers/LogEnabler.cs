using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.S3;
using Amazon.S3.Model;
using S3stat.SecureSetup.Helpers.LightObjects;
using ThirdParty.Json.LitJson;

namespace S3stat.SecureSetup.Helpers
{
	public class LogEnabler
	{
		public const string LogReaderUserARN = "arn:aws:iam::300171020542:user/s3stat_log_reader";
		public const string LogReadersRoleName = "S3statLogReaders";

		#region logging
		public static void SetLoggingS3(AmazonS3Client s3, CombinedEndpoint endpoint)
		{
			var acl = s3.GetACL(new GetACLRequest { BucketName = endpoint.LogBucketName });
			acl.AccessControlList.RemoveGrant(new S3Grantee { URI = "http://acs.amazonaws.com/groups/s3/LogDelivery" });
			acl.AccessControlList.AddGrant(new S3Grantee { URI = "http://acs.amazonaws.com/groups/s3/LogDelivery" }, S3Permission.WRITE);
			acl.AccessControlList.AddGrant(new S3Grantee { URI = "http://acs.amazonaws.com/groups/s3/LogDelivery" }, S3Permission.READ_ACP);
			s3.PutACL(new PutACLRequest { BucketName = endpoint.LogBucketName, AccessControlList = acl.AccessControlList });

			s3.PutBucketLogging(new PutBucketLoggingRequest
			{
				BucketName = endpoint.BucketName,
				LoggingConfig = new S3BucketLoggingConfig
				{
					TargetBucketName = endpoint.LogBucketName,
					TargetPrefix = endpoint.LogPath + endpoint.LogPrefix,
					Grants = new List<S3Grant> 
					{
						new S3Grant { Grantee = new S3Grantee { URI = "http://acs.amazonaws.com/groups/global/AuthenticatedUsers"}, Permission = S3Permission.READ} ,
					}
				}
			});
		}

		public static void SetLoggingCloudfront(AmazonCloudFrontClient cf, CombinedEndpoint endpoint)
		{
			endpoint.DistributionConfig.Logging.Enabled = true;
			endpoint.DistributionConfig.Logging.Bucket = endpoint.LogBucketName + ".s3.amazonaws.com";
			endpoint.DistributionConfig.Logging.Prefix = endpoint.LogPath;

			cf.UpdateDistribution(new UpdateDistributionRequest
			{
				Id = endpoint.Id,
				DistributionConfig = endpoint.DistributionConfig,
				IfMatch = endpoint.ETag,
			});
		}

		public static void SetLoggingStreaming(AmazonCloudFrontClient cf, CombinedEndpoint endpoint)
		{
			endpoint.StreamingDistributionConfig.Logging.Enabled = true;
			endpoint.StreamingDistributionConfig.Logging.Bucket = endpoint.LogBucketName + ".s3.amazonaws.com";
			endpoint.StreamingDistributionConfig.Logging.Prefix = endpoint.LogPath;

			cf.UpdateStreamingDistribution(new UpdateStreamingDistributionRequest
			{
				Id = endpoint.Id,
				StreamingDistributionConfig = endpoint.StreamingDistributionConfig,
				IfMatch = endpoint.ETag,
			});
		}
		#endregion

		#region bucket policy

		public static bool PolicyHasS3statPermissions(CombinedEndpoint endpoint, string policy)
		{
			if (String.IsNullOrEmpty(policy))
			{
				return false;
			}
			var correctPolicy = GetCorrectBucketPolicy(endpoint, policy);
			return policy == correctPolicy;
		}

		public static string GetCorrectBucketPolicy(CombinedEndpoint endpoint, string existingPolicy)
		{
			const string newPolicy = @"{
				""Version"": ""2008-10-17"",
				""Statement"": []
			}";

			var data = GetStatementFromTemplate(newPolicy, endpoint);


			if (!String.IsNullOrEmpty(existingPolicy))
			{
				// Policy already exists.  Tread lightly!!!
				data = JsonMapper.ToObject(existingPolicy);
			}

			var newStatements = data["Statement"].Cast<JsonData>().Where(statement => !statement["Principal"].ToJson().Contains(LogReaderUserARN)).ToList();

			const string listTemplate = @"{
				""Sid"": """",
				""Effect"": ""Allow"",
				""Principal"": {
					""AWS"": ""S3STAT_USER_ARN""
				},
				""Action"": ""s3:ListBucket"",
				""Resource"": ""arn:aws:s3:::S3STAT_BUCKET_NAME""
			}";
			const string readTemplate = @"		{
				""Sid"": """",
				""Effect"": ""Allow"",
				""Principal"": {
					""AWS"": ""S3STAT_USER_ARN""
				},
				""Action"": ""s3:GetObject"",
				""Resource"": ""arn:aws:s3:::S3STAT_BUCKET_NAME/*""
			}";

			newStatements.Add(GetStatementFromTemplate(listTemplate, endpoint));
			newStatements.Add(GetStatementFromTemplate(readTemplate, endpoint));

			data["Statement"].Clear();

			foreach (JsonData statement in newStatements)
			{
				data["Statement"].Add(statement);
			}

			return data.ToJson();
		}


		public static bool SetBucketPolicy(AmazonS3Client s3, CombinedEndpoint endpoint)
		{
			var policy = s3.GetBucketPolicy(new GetBucketPolicyRequest { BucketName = endpoint.LogBucketName });

			var newPolicy = GetCorrectBucketPolicy(endpoint, policy.Policy);

			s3.PutBucketPolicy(new PutBucketPolicyRequest { BucketName = endpoint.LogBucketName, Policy = newPolicy });

			return true;
		}

		private static JsonData GetStatementFromTemplate(string template, CombinedEndpoint endpoint)
		{
			template = template.Replace("S3STAT_BUCKET_NAME", endpoint.LogBucketName);
			template = template.Replace("S3STAT_LOG_PATH", endpoint.LogPath);
			template = template.Replace("S3STAT_USER_ARN", LogReaderUserARN);
			return JsonMapper.ToObject(template);
		}

		#endregion


	}
}
