using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
using FirstFloor.ModernUI.Windows.Controls;
using S3stat.SecureSetup.Helpers;
using S3stat.SecureSetup.Helpers.LightObjects;
using S3stat.SecureSetup.Pages;

namespace S3stat.SecureSetup.Content
{
	/// <summary>
	/// Interaction logic for EndpointDetail.xaml
	/// </summary>
	public partial class EndpointDetail : UserControl
	{
		public EndpointDetail()
		{
			InitializeComponent();
		}

		public CombinedEndpoint Endpoint
		{
			get { return (CombinedEndpoint)Form.DataContext; }
			set { SetEndpoint(value);  }
		}

		private void SetEndpoint(CombinedEndpoint endpoint)
		{
			Form.DataContext = endpoint;
			if (endpoint == null)
			{
				return;
			}

			introGood.Visibility = Visibility.Collapsed;
			introLoggingNoStat.Visibility = Visibility.Collapsed;
			introNoLoggingNoStat.Visibility = Visibility.Collapsed;
			introBadLoggingStat.Visibility = Visibility.Collapsed;
			introUnknownLogging.Visibility = Visibility.Collapsed;

			this.txtLogPath.IsEnabled = true;
			this.txtLogPrefix.IsEnabled = true;
			this.selLogBucketName.IsEnabled = true;
			if (!endpoint.IsLoggingKnown)
			{
				this.IsEnabled = false;
				introUnknownLogging.Visibility = Visibility.Visible;
				return;
			}
			else
			{
				this.IsEnabled = true;
				if (endpoint.IsLogging)
				{
					this.txtLogPath.IsEnabled = false;
					this.txtLogPrefix.IsEnabled = false;
					this.selLogBucketName.IsEnabled = false;
				}
			}

			if (endpoint.IsS3stat)
			{
				if (!endpoint.IsLogging)
				{
					introBadLoggingStat.Visibility = Visibility.Visible;
				}
				else
				{
					introGood.Visibility = Visibility.Visible;
				}
				btnStopS3stat.IsEnabled = true;
			}
			else
			{
				if (endpoint.IsLogging)
				{
					introLoggingNoStat.Visibility = Visibility.Visible;
				}
				else
				{
					introNoLoggingNoStat.Visibility = Visibility.Visible;
				}
				btnStopS3stat.IsEnabled = false;
			}

			panelLogPrefix.Visibility = endpoint.IsS3 ? Visibility.Visible : Visibility.Collapsed;

			if (endpoint.IsLogging)
			{
				btnStopLogging.IsEnabled = true;
			}
			else
			{
				btnStopLogging.IsEnabled = false;
			}
			SetSelect(endpoint);

		}

		private void SetSelect(CombinedEndpoint endpoint)
		{
			foreach (ComboBoxItem item in selLogBucketName.Items)
			{
				if (item.Content.ToString() == endpoint.LogBucketName)
				{
					selLogBucketName.SelectedItem = item;
				}
			}
		}

		public void SetBucketList(Dictionary<string, CombinedEndpoint> endpoints)
		{
			selLogBucketName.Items.Clear();
			foreach (var endpoint in endpoints.Values)
			{
				if (endpoint.IsS3 && endpoint.BucketName == endpoint.BucketName.ToLowerInvariant())
				{
					selLogBucketName.Items.Add(new ComboBoxItem { Content = endpoint.BucketName, Tag = endpoint });
				}
			}
			selLogBucketName.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending)); 
		}

		private EndpointSetup FindContainingPage()
		{
			var parent = Parent;
			while (parent != null && parent is FrameworkElement)
			{
				var page = parent as EndpointSetup;
				if (page != null)
				{
					return page;
				}
				parent = ((FrameworkElement)parent).Parent;
			}


			return null;
		}

		private void Save(EndpointSetup setupPage)
		{
			if (Endpoint.Endpoint == null)
			{
				if (Endpoint.IsS3)
				{
					Endpoint.Endpoint = new CBucket
					{
						BucketName = Endpoint.BucketName,
						IsActive = true,
						IsCompactStats = true,
						IsLogging = true,
						IsPrivateStats = false,
						IsSelfManaged = false,
						LogBucketName = Endpoint.LogBucketName,
						LogPath = Endpoint.LogPath,
						LogPrefix = Endpoint.LogPrefix,
						DisplayName = Endpoint.BucketName,
						UploadReports = true,
					};
				}
				else
				{
					Endpoint.Endpoint = new CDistribution
					{
						AWSDistributionID = Endpoint.Id,
						BucketName = Endpoint.BucketName,
						IsActive = true,
						IsCompactStats = true,
						IsLogging = true,
						IsPrivateStats = false,
						IsSelfManaged = false,
						IsStreaming = Endpoint.IsStreaming,
						LogBucketName = Endpoint.LogBucketName,
						LogPath = Endpoint.LogPath,
					};
				}
			}

			var cf = setupPage.CFClient;
			if (!Endpoint.IsLogging ||
				Endpoint.Endpoint.LogBucketName != Endpoint.LogBucketName
				|| Endpoint.Endpoint.LogPath != Endpoint.LogPath
				|| (Endpoint.IsS3 && (((CBucket)Endpoint.Endpoint).LogPrefix != Endpoint.LogPrefix)
				))
			{
				Endpoint.Endpoint.LogBucketName = Endpoint.LogBucketName;
				Endpoint.Endpoint.LogPath = Endpoint.LogPath;
				if (Endpoint.IsS3)
				{
					((CBucket)Endpoint.Endpoint).LogPrefix = Endpoint.LogPrefix;
					try
					{
						LogEnabler.SetLoggingS3(setupPage.S3ClientDefault, Endpoint);
					}
					catch
					{
						var s3 = S3Helper.GetS3ClientForEndpoint(Endpoint, S3Helper.LogOrSource.Source);
						LogEnabler.SetLoggingS3(s3, Endpoint);
					}
				}
				else if (Endpoint.IsStreaming)
				{
					LogEnabler.SetLoggingStreaming(cf, Endpoint);
				}
				else
				{
					LogEnabler.SetLoggingCloudfront(cf, Endpoint);
				}
				Endpoint.IsLogging = true;
			}


			Endpoint.Endpoint.S3AccountID = AppState.Account.S3AccountID;
			var s3stat = new S3statHelper(AppState.UserName, AppState.Password);
			try
			{
				int id = s3stat.SetEndpoint(Endpoint.Endpoint);
				if (id > 0)
				{
					if (Endpoint.IsS3)
					{
						((CBucket) Endpoint.Endpoint).BucketID = id;
					}
					else
					{
						((CDistribution) Endpoint.Endpoint).DistributionID = id;
					}
					Endpoint.IsS3stat = true;
				}
			}
			catch
			{
				ModernDialog.ShowMessage("Couldn't save to S3stat.", "Is this endpoint already set up in another account?", MessageBoxButton.OK);
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			var setupPage = FindContainingPage();
			Save(setupPage);

			SetEndpoint(Endpoint);
			setupPage.UpdateEndpointEntry(Endpoint);

			setupPage.ShowComplete();
		}

		private void btnStopS3stat_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnStopLogging_Click(object sender, RoutedEventArgs e)
		{

		}

		private void txtLogPrefix_LostFocus(object sender, EventArgs e)
		{
			if (txtLogPrefix.Text.Contains("/"))
			{
				txtLogPrefix.Text = txtLogPrefix.Text.Replace("/", "");
			}
		}

		private void txtLogPath_LostFocus(object sender, EventArgs e)
		{
			if (!txtLogPath.Text.EndsWith("/") && txtLogPath.Text != "")
			{
				txtLogPath.Text = txtLogPath.Text + "/";
			}

		}
	}
}
