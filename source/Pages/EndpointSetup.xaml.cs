using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.S3;
using Amazon.S3.Model;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using S3stat.SecureSetup.Content;
using S3stat.SecureSetup.Helpers;
using S3stat.SecureSetup.Helpers.LightObjects;
using FragmentNavigationEventArgs = FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs;
using NavigatingCancelEventArgs = FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs;
using NavigationEventArgs = FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs;

namespace S3stat.SecureSetup.Pages
{
	/// <summary>
	/// Interaction logic for EndpointSetup.xaml
	/// </summary>
	public partial class EndpointSetup : AppControl, IContent
	{
		private Dictionary<string, CombinedEndpoint> _combinedEndpoints;

		private AmazonS3Client _s3;
		private AmazonCloudFrontClient _cf;

		public AmazonCloudFrontClient CFClient
		{
			get
			{
				if (_cf == null)
				{
					_cf = new AmazonCloudFrontClient(AppState.AWSAccessKey, AppState.AWSSecretKey, RegionEndpoint.USEast1);
				}
				return _cf;
			}
			set { _cf = value; }
		}

		public AmazonS3Client S3ClientDefault
		{
			get
			{
				if (_s3 == null)
				{
					_s3 = new AmazonS3Client(AppState.AWSAccessKey, AppState.AWSSecretKey, new AmazonS3Config() { ServiceURL = "http://s3.amazonaws.com/",  });
				}
				return _s3;
			}
			set { _s3 = value; }
		}

		public EndpointSetup()
		{
			InitializeComponent();
		}

		void EndpointSetup_Loaded(object sender, RoutedEventArgs e)
		{
		}
		public void OnFragmentNavigation(FragmentNavigationEventArgs e)
		{
		}

		public void OnNavigatedFrom(NavigationEventArgs e)
		{
		}

		public void OnNavigatedTo(NavigationEventArgs e)
		{
			Populate();
		}

		public void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
		}

		private async void Populate()
		{
			

			if (String.IsNullOrEmpty(AppState.UserName) || String.IsNullOrEmpty(AppState.Password))
			{
				NavigateToLogin();
				return;
			}
			if (String.IsNullOrEmpty(AppState.AWSAccessKey) || String.IsNullOrEmpty(AppState.AWSSecretKey) || String.IsNullOrEmpty(AppState.AWSAccountID))
			{
				NavigateToCredentials();
				return;
			}

			EnsureAccountLoaded();
			if (!AppState.Account.CanAssumeRole)
			{
				NavigateToRole();
				return;
			}

			// todo: hook up async loader
			//frame.ContentLoader.LoadContentAsync()
			await PopulateAWSEndpoints(new CancellationToken());
			PopulateEndpointList();
			IntegrateS3statEndpoints();
			
		}


		private async Task PopulateAWSEndpoints(CancellationToken cancellationToken)
		{
			_combinedEndpoints = new Dictionary<string, CombinedEndpoint>();
			CombinedEndpoint endpoint;

			var cf = CFClient;
			try
			{
				var distributions = await cf.ListDistributionsAsync(new ListDistributionsRequest(), cancellationToken);
				foreach (var summary in distributions.DistributionList.Items)
				{
					var origin = summary.Origins.Items[0];
					endpoint = new CombinedEndpoint() { 
						Id = summary.Id,
						Type = CombinedEndpoint.EndpointType.Cloudfront,
						BucketName = origin.DomainName,
						Title = StripBucketName(origin.DomainName),
						Subtitle = summary.Id,
						IsCloudfront = true,
						HasReports = false,
						IsS3 = false,
						IsS3stat = false,
						IsStreaming = false,
						IsLoggingKnown = false,
						IsS3statKnown = false,
						LogBucketName = StripBucketName(origin.DomainName),
						LogPath = "cflog/",
						LogPrefix = "",
						
					};
					_combinedEndpoints.Add(summary.Id, endpoint);

					GetDistributionLogging(cf, endpoint);
				}

				var streamingDistributions = await cf.ListStreamingDistributionsAsync(new ListStreamingDistributionsRequest(), cancellationToken);
				foreach (var summary in streamingDistributions.StreamingDistributionList.Items)
				{
					var origin = summary.S3Origin;
					endpoint = new CombinedEndpoint()
					{
						Id = summary.Id,
						Type = CombinedEndpoint.EndpointType.Streaming,
						BucketName = origin.DomainName,
						Title = StripBucketName(origin.DomainName),
						Subtitle = summary.Id,
						IsCloudfront = true,
						HasReports = false,
						IsS3 = false,
						IsS3stat = false,
						IsStreaming = true,
						IsLoggingKnown = false,
						IsS3statKnown = false,
						LogBucketName = StripBucketName(origin.DomainName),
						LogPath = "streamlog/",
						LogPrefix = "",
					};
					_combinedEndpoints.Add(summary.Id, endpoint);
					GetStreamingDistributionLogging(cf, endpoint);
				}
			}
			catch (Exception e)
			{
				var s3stat = new S3statHelper(AppState.UserName, AppState.Password);
				s3stat.NoteException(e, "GetCFLogging", true);

				ModernDialog.ShowMessage("Couldn't read distribution list or logging.", "Insufficient permissions on your IAM User.", MessageBoxButton.OK);
				
				NavigateToCredentials();
			}

			try
			{
				var buckets = await S3ClientDefault.ListBucketsAsync(new ListBucketsRequest(), cancellationToken);
				foreach (var bucketInfo in buckets.Buckets)
				{
					endpoint = new CombinedEndpoint()
					{
						Id = bucketInfo.BucketName,
						Type = CombinedEndpoint.EndpointType.S3,
						BucketName = bucketInfo.BucketName,
						Title = bucketInfo.BucketName,
						Subtitle = "",
						IsCloudfront = false,
						HasReports = false,
						IsS3 = true,
						IsS3stat = false,
						IsStreaming = false,
						IsLoggingKnown = false,
						IsS3statKnown = false,
						LogBucketName = bucketInfo.BucketName,
						LogPath = "log/",
						LogPrefix = "access_log-",
					};
					_combinedEndpoints.Add(bucketInfo.BucketName, endpoint);

					GetBucketLogging(endpoint);
				}
			}
			catch (Exception e)
			{
				var s3stat = new S3statHelper(AppState.UserName, AppState.Password);
				s3stat.NoteException(e, "GetS3Logging", true);

				ModernDialog.ShowMessage("Couldn't read S3 bucket list or logging.", "Insufficient permissions on your IAM User.", MessageBoxButton.OK);
				NavigateToCredentials();
			}

		}

		private async Task GetDistributionLogging(AmazonCloudFrontClient cf, CombinedEndpoint endpoint)
		{
			var config = await cf.GetDistributionConfigAsync(new GetDistributionConfigRequest() { Id = endpoint.Id });
			endpoint.ETag = config.ETag;
			endpoint.DistributionConfig = config.DistributionConfig;
			endpoint.IsLogging = config.DistributionConfig.Logging.Enabled;
			endpoint.IsLoggingKnown = true;

			if (endpoint.IsLogging)
			{
				endpoint.LogBucketName = StripBucketName(endpoint.DistributionConfig.Logging.Bucket);
				endpoint.LogPath = endpoint.DistributionConfig.Logging.Prefix;
			}

			UpdateEndpointEntry(endpoint);
		}

		private async Task GetStreamingDistributionLogging(AmazonCloudFrontClient cf, CombinedEndpoint endpoint)
		{
			var config = await cf.GetStreamingDistributionConfigAsync(new GetStreamingDistributionConfigRequest() { Id = endpoint.Id });
			endpoint.ETag = config.ETag;
			endpoint.StreamingDistributionConfig = config.StreamingDistributionConfig;
			endpoint.IsLogging = config.StreamingDistributionConfig.Logging.Enabled;
			endpoint.IsLoggingKnown = true;

			if (endpoint.IsLogging)
			{
				endpoint.LogBucketName = StripBucketName(endpoint.StreamingDistributionConfig.Logging.Bucket);
				endpoint.LogPath = endpoint.StreamingDistributionConfig.Logging.Prefix;
			}

			UpdateEndpointEntry(endpoint);
		}

		public static void ApplyBucketLoggingPaths(CombinedEndpoint endpoint, string targetPrefix)
		{
			var splitter = new Regex(@"[^/]+$", RegexOptions.Compiled);

			if (splitter.IsMatch(targetPrefix))
			{
				endpoint.LogPath = splitter.Split(targetPrefix)[0];
				endpoint.LogPrefix = splitter.Matches(targetPrefix)[0].Value;
			}
			else
			{
				endpoint.LogPath = targetPrefix;
				endpoint.LogPrefix = "";
			}
		}

		private async Task GetBucketLoggingInner(AmazonS3Client s3, CombinedEndpoint endpoint)
		{
			// Uncomment this to test slow connections:
			//await Task.Delay(5000);

			var logging = await s3.GetBucketLoggingAsync(new GetBucketLoggingRequest() { BucketName = endpoint.BucketName });
			endpoint.BucketLoggingConfig = logging.BucketLoggingConfig;
			endpoint.IsLogging = !String.IsNullOrEmpty(logging.BucketLoggingConfig.TargetBucketName);
			if (endpoint.IsLogging)
			{
				var targetPrefix = logging.BucketLoggingConfig.TargetPrefix;
				endpoint.LogBucketName = logging.BucketLoggingConfig.TargetBucketName;
				ApplyBucketLoggingPaths(endpoint, targetPrefix);
			}

			endpoint.IsLoggingKnown = true;
			UpdateEndpointEntry(endpoint);
		}

		private async Task GetBucketLogging(CombinedEndpoint endpoint)
		{
			try
			{
				await GetBucketLoggingInner(S3ClientDefault, endpoint);
			}
			catch (Exception e)
			{
				var s3 = S3Helper.GetS3ClientForEndpoint(endpoint, S3Helper.LogOrSource.Source);
				GetBucketLoggingInner(s3, endpoint);
			}
		}


		private string StripBucketName(string bucketName)
		{
			return bucketName.Replace(".s3.amazonaws.com", "");
		}

		private void IntegrateS3statEndpoints()
		{
			var s3stat = new S3statHelper(AppState.UserName, AppState.Password);

			CAccount endpoints;
			try
			{
				endpoints = s3stat.ListEndpoints(AppState.AWSAccountID);
			}
			catch (Exception e)
			{
				NavigateToLogin();
				return;
			}


			foreach (var distribution in endpoints.CDistributions)
			{
				if (_combinedEndpoints.ContainsKey(distribution.AWSDistributionID))
				{
					var combined = _combinedEndpoints[distribution.AWSDistributionID];
					combined.IsS3stat = true;
					combined.Endpoint = distribution;
					combined.HasReports = !distribution.LastProcessDate.IsNull;
				}
			}
			foreach (var bucket in endpoints.CBuckets)
			{
				if (_combinedEndpoints.ContainsKey(bucket.BucketName))
				{
					var combined = _combinedEndpoints[bucket.BucketName];
					combined.IsS3stat = true;
					combined.Endpoint = bucket;
					combined.HasReports = !bucket.LastProcessDate.IsNull;
				}
			}

			foreach (var combined in _combinedEndpoints.Values)
			{
				combined.IsS3statKnown = true;
				UpdateEndpointEntry(combined);
			}
		}

		public void PopulateEndpointList()
		{
			EndpointBlock selectedBlock = null;
			EndpointList.Children.Clear();
			foreach (var endpoint in _combinedEndpoints.Values)
			{
				var block = new EndpointBlock(endpoint) {IsS3stat = endpoint.IsS3stat};
				block.MouseDown += block_MouseDown;
				if (block.Tag == Detail.Endpoint)
				{
					selectedBlock = block;
				}
				EndpointList.Children.Add(block);
			}

			Detail.SetBucketList(_combinedEndpoints);

			if (selectedBlock != null)
			{
				selectedBlock.IsSelected = true;

				// Force update
				Detail.Endpoint = null;
				Detail.Endpoint = (CombinedEndpoint)selectedBlock.Tag;
			}
		}

		public void ShowComplete()
		{
			CollapseDetailSections();
			DetailComplete.Visibility = Visibility.Visible;
		}

		public void ShowPlaceholder()
		{
			CollapseDetailSections();
			DetailPlaceholder.Visibility = Visibility.Visible;

			foreach (EndpointBlock block in EndpointList.Children)
			{
				block.IsSelected = false;
			}
		}

		private void CollapseDetailSections()
		{
			DetailPlaceholder.Visibility = Visibility.Collapsed;
			DetailComplete.Visibility = Visibility.Collapsed;
			DetailEnabled.Visibility = Visibility.Collapsed;
			Detail.Visibility = Visibility.Collapsed;
		}

		void block_MouseDown(object sender, MouseButtonEventArgs e)
		{
			CollapseDetailSections();

			var block = (EndpointBlock)sender;
			block.IsSelected = true;
			var endpoint = (CombinedEndpoint)block.Tag;

			if (endpoint.IsS3stat)
			{
				DetailEnabled.Endpoint = endpoint;
				DetailEnabled.Visibility = Visibility.Visible;
			}
			else
			{
				Detail.Endpoint = endpoint;
				Detail.Visibility = Visibility.Visible;
			}

			foreach (EndpointBlock otherblock in EndpointList.Children)
			{
				if (otherblock != block)
				{
					otherblock.IsSelected = false;
				}
			}
		}

		public void UpdateEndpointEntry(CombinedEndpoint combined)
		{
			if (EndpointList.Children.Count > 0)
			{
				foreach (EndpointBlock block in EndpointList.Children)
				{
					if (block.Tag == combined)
					{
						block.Populate(combined);
						if (block.Tag == Detail.Endpoint)
						{
							block.IsSelected = true;

							// Force update
							Detail.Endpoint = null;
							Detail.Endpoint = (CombinedEndpoint)block.Tag;
						}
						return;
					}
				}
			}
			PopulateEndpointList();
		}


	}
}
