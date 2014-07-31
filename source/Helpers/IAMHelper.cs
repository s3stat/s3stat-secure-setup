using System;
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
	""Condition"":{""StringEquals"":{""sts:ExternalId"":""S3stat""}}
}]}";
			assumeRolePolicy = assumeRolePolicy.Replace("S3STAT_USER_ARN", LogEnabler.LogReaderUserARN);

			const string accessPolicy = @"{
""Statement"": [{
	""Sid"": """",
	""Action"": [
		""s3:GetObject"",
		""s3:ListBucket""
	],
	""Effect"": ""Allow"",
	""Resource"": [""arn:aws:s3:::*""]
}]}";

			try
			{
				iam.CreateRole(new CreateRoleRequest()
				{
					AssumeRolePolicyDocument = assumeRolePolicy,
					RoleName = LogEnabler.LogReadersRoleName
				});
				iam.PutRolePolicy(new PutRolePolicyRequest()
				{
					PolicyDocument = accessPolicy,
					PolicyName = "S3statReadAccess",
					RoleName = LogEnabler.LogReadersRoleName
				});
			}
			catch (EntityAlreadyExistsException e)
			{
				// no worries.  already exists
			}
			catch (Exception e2)
			{
				return false;
			}

			return true;
		}
	}
}
