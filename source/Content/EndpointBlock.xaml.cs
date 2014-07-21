using System;
using System.Windows;
using System.Windows.Controls;
using S3stat.SecureSetup.Helpers.LightObjects;

namespace S3stat.SecureSetup.Content
{
	/// <summary>
	/// Interaction logic for EndpointBlock.xaml
	/// </summary>
	public partial class EndpointBlock
	{
		public static readonly DependencyProperty IsS3statProperty = DependencyProperty.RegisterAttached(
			"IsS3stat",
			typeof(Boolean),
			typeof(EndpointBlock),
			new PropertyMetadata(false)
		);
		public static void SetIsS3stat(UIElement element, Boolean value)
		{
			element.SetValue(IsS3statProperty, value);
		}
		public static Boolean GetIsS3stat(UIElement element)
		{
			return (Boolean)element.GetValue(IsS3statProperty);
		}

		public bool IsS3stat
		{
			get { return (bool)GetValue(IsS3statProperty); }
			set { SetValue(IsS3statProperty, value); }
		}


		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
			"IsSelected",
			typeof(Boolean),
			typeof(EndpointBlock),
			new PropertyMetadata(false)
		);
		public static void SetIsSelected(UIElement element, Boolean value)
		{
			element.SetValue(IsSelectedProperty, value);
		}
		public static Boolean GetIsSelected(UIElement element)
		{
			return (Boolean)element.GetValue(IsSelectedProperty);
		}

		public bool IsSelected
		{
			get { return (bool)GetValue(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}



		public EndpointBlock()
		{
			InitializeComponent();
		}

		public EndpointBlock(CombinedEndpoint endpoint)
			: this()
		{
			Populate(endpoint);
		}

		public void Populate(CombinedEndpoint endpoint)
		{
			Tag = endpoint;
			Title.Text = endpoint.Title;
			SubTitle.Text = endpoint.Subtitle;

			Tags.Children.Clear();
			Tags.Children.Add(new TextBlock { Text = endpoint.Type.ToString(), Style = (Style)Application.Current.FindResource("TagInfo") });

			if (endpoint.IsS3statKnown)
			{
				if (endpoint.IsS3stat)
				{
					Tags.Children.Add(new TextBlock {Text = "S3stat", Style = (Style) Application.Current.FindResource("Tag")});
				}
				else
				{
					Tags.Children.Add(new TextBlock
						{
							Text = "Not S3stat",
							Style = (Style) Application.Current.FindResource("TagNeutral")
						});
				}
			}

			if (endpoint.IsLoggingKnown)
			{
				if (endpoint.IsLogging)
				{
					Tags.Children.Add(new TextBlock { Text = "Logging", Style = (Style)Application.Current.FindResource("Tag") });
				}
				else
				{
					Tags.Children.Add(new TextBlock
						{
							Text = "Not Logging",
							Style = endpoint.IsS3stat ? (Style)Application.Current.FindResource("TagBad") : (Style)Application.Current.FindResource("TagNeutral")
						});
				}
			}


			if (endpoint.IsS3stat)
			{
				if (endpoint.HasReports)
				{
					Tags.Children.Add(new TextBlock { Text = "Reporting", Style = (Style)Application.Current.FindResource("Tag") });
				}
				else
				{
					Tags.Children.Add(new TextBlock { Text = "Pending", Style = (Style)Application.Current.FindResource("TagWarning") });
				}
				IsS3stat = true;
			}


			if (!endpoint.IsLoggingKnown || !endpoint.IsS3statKnown)
			{
				Tags.Children.Add(new TextBlock
					{
						Text = "Discovering...",
						Style = (Style)Application.Current.FindResource("TagWarning")
					});
			}
			else
			{
				progress.Visibility = Visibility.Hidden;
			}

		}
	}
}
