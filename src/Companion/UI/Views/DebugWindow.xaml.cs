using System.Windows;

namespace PWCompanion.UI.Views;

public partial class DebugWindow : Window
{
    private readonly DebugWindowViewModel _viewModel;

    public DebugWindow(DebugWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void OnInspectMemory(object sender, RoutedEventArgs e) => _viewModel.InspectMemory();
    private void OnReloadOffsets(object sender, RoutedEventArgs e) => _viewModel.ReloadOffsets();
    private void OnClose(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.Dispose();
        base.OnClosed(e);
    }
}
