﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;

namespace S3stat.SecureSetup.Helpers
{
	public class IAMHelper
	{
		public static string GetAccountID()
		{
			var iam = new AmazonIdentityManagementServiceClient(AppState.AWSAccessKey, AppState.AWSSecretKey);

			try
			{
				var userInfo = iam.GetUser();
				return GetAccountIDFromARN(userInfo.User.Arn);
			}
			catch (Exception e)
			{
				// HACK: We can still pull the AccountID from the error message, which like this:
				// User: arn:aws:iam::471320790765:user/S3stat is not authorized to perform: iam:GetUser on resource: arn:aws:iam::471320790765:user/S3stat
				return GetAccountIDFromARN(e.Message);
			}
		}

		public static string GetAccountIDFromARN(string arn)
		{
			var splitter = new Regex(@"::([^:]+):(user|root|group)", RegexOptions.Compiled);
			if (splitter.IsMatch(arn))
			{
				try
				{
					return splitter.Split(arn)[1];
				}
				catch
				{
				}
			}

			return "";
		}

		public static string GetRoleAccessPolicy(bool includeCloudWatchPermission)
		{
			string s3Statement = @"{
	""Sid"": ""S3ReadOnly"",
	""Action"": [
		""s3:GetObject"",
		""s3:ListBucket"",
		""s3:GetBucketLocation""
	],
	""Effect"": ""Allow"",
	""Resource"": [""arn:aws:s3:::*""]
}";

			string cloudWatchStatement = @", {
	""Sid"": ""AllowCloudWatchBucketInfo"",
	""Action"": [
		""cloudwatch:GetMetricStatistics""
	],
	""Effect"": ""Allow"",
	""Resource"": [""*""]
}";

			string accessPolicy = @"{""Statement"": ["
				+ s3Statement
				+ (includeCloudWatchPermission ? cloudWatchStatement : "")
			+ "]}";

			return accessPolicy;
		}

		public static bool CreateLogReaderRole()
		{
			var iam = new AmazonIdentityManagementServiceClient(AppState.AWSAccessKey, AppState.AWSSecretKey);

			var assumeRolePolicy = @"{
""Version"":""2012-10-17"",
""Statement"":[{
	""Sid"":"""",
	""Effect"":""Allow"",
	""Principal"":{""AWS"":""S3STAT_USER_ARN""},
	""Action"":""sts:AssumeRole"",
	""Condition"":{""StringEquals"":{""sts:ExternalId"":""S3STAT_ROLE_EXTERNAL_ID""}}
}]}";
			assumeRolePolicy = assumeRolePolicy.Replace("S3STAT_USER_ARN", LogEnabler.LogReaderUserARN);
			assumeRolePolicy = assumeRolePolicy.Replace("S3STAT_ROLE_EXTERNAL_ID", AppState.Account.RoleExternalID);

			try
			{
				iam.CreateRole(new CreateRoleRequest()
				{
					AssumeRolePolicyDocument = assumeRolePolicy,
					RoleName = LogEnabler.LogReadersRoleName
				});
			}
			catch (EntityAlreadyExistsException e)
			{
				// no worries.  already exists
			}

			// Attempt to grant CloudWatch Access.  Fall back to not if we're
			// using an older IAM policy that predates us adding that permission.
			try
			{
				iam.PutRolePolicy(new PutRolePolicyRequest()
				{
					PolicyDocument = GetRoleAccessPolicy(true),
					PolicyName = "S3statReadAccess",
					RoleName = LogEnabler.LogReadersRoleName
				});
			}
			catch (Exception e)
			{
				// 20170615: AWS doesn't appear to throw when creating a role with more permissions than
				// the creating user.  That's unexpected, so we'll trap it if it ever happens.
				AppState.NoteException(e, "PutRolePolicyRequest with CloudWatch", true);
				iam.PutRolePolicy(new PutRolePolicyRequest()
				{
					PolicyDocument = GetRoleAccessPolicy(false),
					PolicyName = "S3statReadAccess",
					RoleName = LogEnabler.LogReadersRoleName
				});
			}



			return true;
		}
	}
}
