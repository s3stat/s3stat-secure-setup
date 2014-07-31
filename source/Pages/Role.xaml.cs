using System;
using System.Collections.Generic;
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
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using S3stat.SecureSetup.Helpers;
using S3stat.SecureSetup.Helpers.LightObjects;
using NavigationEventArgs = FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs;

namespace S3stat.SecureSetup.Pages
{
	/// <summary>
	/// Interaction logic for Role.xaml
	/// </summary>
	public partial class Role : AppControl, IContent
	{
		public Role()
		{
			InitializeComponent();
		}

		public void OnNavigatedTo(NavigationEventArgs e)
		{
			Populate();
		}

		public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
		{
		}

		public void OnNavigatedFrom(NavigationEventArgs e)
		{
		}

		public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
		{
		}
		private void Populate()
		{
			EnsureAccountLoaded();

			if (String.IsNullOrEmpty(AppState.AWSAccountID))
			{
				NavigateToCredentials();
				return;
			}

			cbAllow.IsChecked = AppState.Account.CanAssumeRole;
			btnContinue.IsEnabled = AppState.Account.CanAssumeRole;
		}

		private void Go()
		{
			if (!IAMHelper.CreateLogReaderRole())
			{
				ModernDialog.ShowMessage("Couldn't create S3statLogReaders Role with the supplied credentials.  Check to ensure your IAM policy includes both iam:CreateRole and iam:PutRolePolicy permissions", "Insufficient Permission", MessageBoxButton.OK);
				return;
			}

			if (AppState.Account.CanAssumeRole)
			{
				NavigateToEndpointSetup();
				return;
			}

			AppState.Account.CanAssumeRole = true;
			var s3stat = new S3statHelper(AppState.UserName, AppState.Password);
			try
			{
				s3stat.SetS3statAccount(AppState.Account);
			}
			catch
			{
				ModernDialog.ShowMessage("Couldn't access your S3stat account with the supplied credentials.  Check to ensure your username and password are correct.", "Error Accessing S3stat", MessageBoxButton.OK);
				return;
			}

			NavigateToEndpointSetup();
		}



		private void btnContinue_Click(object sender, RoutedEventArgs e)
		{
			Go();
		}

		private void cbAllow_Click(object sender, RoutedEventArgs e)
		{
			btnContinue.IsEnabled = (cbAllow.IsChecked.HasValue && cbAllow.IsChecked.Value);

		}

	}
}
