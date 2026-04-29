using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProductType = PartnerDemoApp.Wpf.src.AppDataContext.ProductType;
using MaterialType = PartnerDemoApp.Wpf.src.AppDataContext.MaterialType;
using PartnerDemoApp.Wpf.src;

namespace PartnerDemoApp.Wpf;

public partial class PartnersWindow : Window
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    public PartnersWindow()
    {
        InitializeComponent();
        LoadPartners();
        LoadMaterialDictionaries();
    }

    private void ShowMessage(string message, string title, MessageBoxImage image)
    {
        MessageBox.Show(this, message, title, MessageBoxButton.OK, image);
    }

    private void LoadPartners()
    {
        try
        {
            var partners = AppData.GetPartners();
            PartnersListBox.ItemsSource = partners;

            if (partners.Count > 0)
            {
                PartnersListBox.SelectedIndex = 0;
                return;
            }

            LoadPartnerHistory();
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить список партнеров.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void LoadMaterialDictionaries()
    {
        try
        {
            var productTypes = AppData.GetProductTypes();
            var materialTypes = AppData.GetMaterialTypes();

            ProductTypeComboBox.ItemsSource = productTypes;
            MaterialTypeComboBox.ItemsSource = materialTypes;
            ProductTypeComboBox.SelectedIndex = productTypes.Count > 0 ? 0 : -1;
            MaterialTypeComboBox.SelectedIndex = materialTypes.Count > 0 ? 0 : -1;
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить справочники для расчета.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private PartnerListItem? GetSelectedPartner(string? warningMessage = null)
    {
        var partner = PartnersListBox.SelectedItem as PartnerListItem;

        if (partner is null && !string.IsNullOrWhiteSpace(warningMessage))
        {
            ShowMessage(warningMessage, "Предупреждение", MessageBoxImage.Warning);
        }

        return partner;
    }

    private void LoadPartnerHistory()
    {
        var selectedPartner = GetSelectedPartner();

        if (selectedPartner is null)
        {
            HistoryPartnerTextBlock.Text = "Партнер не выбран";
            HistoryDataGrid.ItemsSource = null;
            return;
        }

        try
        {
            HistoryPartnerTextBlock.Text = $"Партнер: {selectedPartner.Name}";
            HistoryDataGrid.ItemsSource = AppData.GetPartnerHistory(selectedPartner.Id);
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить историю продаж.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void OpenPartnerEditor(int? partnerId)
    {
        var window = new PartnerEditWindow(partnerId) { Owner = this };

        if (window.ShowDialog() == true)
        {
            LoadPartners();
        }
    }

    private bool TryParsePositiveInt(string value, string warningMessage, out int result)
    {
        if (int.TryParse(value.Trim(), NumberStyles.Integer, RuCulture, out result) && result > 0)
        {
            return true;
        }

        ShowMessage(warningMessage, "Предупреждение", MessageBoxImage.Warning);
        return false;
    }

    private bool TryParsePositiveDouble(string value, string warningMessage, out double result)
    {
        if (double.TryParse(value.Trim(), NumberStyles.Any, RuCulture, out result) && result > 0)
        {
            return true;
        }

        if (double.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result) && result > 0)
        {
            return true;
        }

        ShowMessage(warningMessage, "Предупреждение", MessageBoxImage.Warning);
        return false;
    }

    private void CalculateMaterial()
    {
        if (ProductTypeComboBox.SelectedItem is not ProductType productType)
        {
            ShowMessage("Выберите тип продукции.", "Предупреждение", MessageBoxImage.Warning);
            return;
        }

        if (MaterialTypeComboBox.SelectedItem is not MaterialType materialType)
        {
            ShowMessage("Выберите тип материала.", "Предупреждение", MessageBoxImage.Warning);
            return;
        }

        if (!TryParsePositiveInt(QuantityTextBox.Text, "Укажите корректное количество (больше 0).", out var quantity))
        {
            return;
        }

        if (!TryParsePositiveDouble(Parameter1TextBox.Text, "Укажите корректный параметр 1 (больше 0).", out var parameter1))
        {
            return;
        }

        if (!TryParsePositiveDouble(Parameter2TextBox.Text, "Укажите корректный параметр 2 (больше 0).", out var parameter2))
        {
            return;
        }

        try
        {
            var result = MaterialCalculator.CalculateRequiredMaterial(
                productType.Id,
                materialType.Id,
                quantity,
                parameter1,
                parameter2);

            if (result < 0)
            {
                ShowMessage("Не удалось рассчитать материал. Проверьте входные параметры.", "Предупреждение", MessageBoxImage.Warning);
                ResultTextBlock.Text = "Результат: -1";
                return;
            }

            ResultTextBlock.Text = $"Результат: {result}";
        }
        catch (Exception exception)
        {
            ShowMessage($"Ошибка при расчете материала.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void AddPartnerButton_Click(object sender, RoutedEventArgs e)
    {
        OpenPartnerEditor(partnerId: null);
    }

    private void EditPartnerButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedPartner = GetSelectedPartner("Выберите партнера для редактирования.");

        if (selectedPartner is null)
        {
            return;
        }

        OpenPartnerEditor(selectedPartner.Id);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        CalculateMaterial();
    }

    private void PartnersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadPartnerHistory();
    }

    private void PartnersListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var selectedPartner = GetSelectedPartner();

        if (selectedPartner is null)
        {
            return;
        }

        OpenPartnerEditor(selectedPartner.Id);
    }
}
