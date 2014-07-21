using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;

namespace S3stat.SecureSetup.Helpers
{
	public abstract class AppControl : UserControl
	{
		public void NavigateTo(string url)
		{
			var uri = new Uri(url, UriKind.Relative);
			var frame = this.Parent as ModernFrame ?? NavigationHelper.FindFrame(null, this);
			frame.Source = uri;
		}


		public void NavigateToLogin()
		{
			NavigateTo("/Pages/S3statLogin.xaml");
		}

		public void NavigateToCredentials()
		{
			NavigateTo("/Pages/AWSCredentials.xaml");
		}

		public void NavigateToRole()
		{
			NavigateTo("/Pages/Role.xaml");
		}

		public void EnsureAccountLoaded()
		{
			if (AppState.Account != null)
			{
				return;
			}

			if (String.IsNullOrEmpty(AppState.AWSAccountID))
			{
				NavigateToCredentials();
				return;
			}

			var s3stat = new S3statHelper(AppState.UserName, AppState.Password);
			try
			{
				AppState.Account = s3stat.GetOrCreateS3StatAccount(AppState.AWSAccountID);
			}
			catch (Exception e)
			{
				NavigateToLogin();
			}
		}
	}
}
