using IdeaBranch.App.Resources;

namespace IdeaBranch.App;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = string.Format(AppResources.MainPage_CounterClickedOnce, count);
		else
			CounterBtn.Text = string.Format(AppResources.MainPage_CounterClickedMultiple, count);

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
