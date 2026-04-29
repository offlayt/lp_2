using System.Windows;
using PartnerType = PartnerDemoApp.Wpf.src.AppDataContext.PartnerType;
using Partner = PartnerDemoApp.Wpf.src.AppDataContext.Partner;
using PartnerDemoApp.Wpf.src;

namespace PartnerDemoApp.Wpf;

public partial class PartnerEditWindow : Window
{
    private readonly int? _partnerId;

    public PartnerEditWindow(int? partnerId)
    {
        _partnerId = partnerId;

        InitializeComponent();

        ApplyModeText();
        LoadPartnerTypes();
        LoadPartnerIfEditMode();
    }

    private void ShowMessage(string message, string title, MessageBoxImage image)
    {
        MessageBox.Show(this, message, title, MessageBoxButton.OK, image);
    }

    private void ApplyModeText()
    {
        var isEditMode = _partnerId.HasValue;
        Title = isEditMode ? "Редактирование партнера" : "Добавление партнера";
        TitleTextBlock.Text = isEditMode ? "Редактирование данных партнера" : "Добавление партнера";
    }

    private void LoadPartnerTypes()
    {
        try
        {
            var partnerTypes = AppData.GetPartnerTypes();
            PartnerTypeComboBox.ItemsSource = partnerTypes;
            PartnerTypeComboBox.SelectedIndex = partnerTypes.Count > 0 ? 0 : -1;
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить типы партнеров.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void LoadPartnerIfEditMode()
    {
        if (!_partnerId.HasValue)
        {
            return;
        }

        try
        {
            var partner = AppData.GetPartnerById(_partnerId.Value);

            if (partner is null)
            {
                ShowMessage("Партнер для редактирования не найден.", "Ошибка", MessageBoxImage.Error);
                Close();
                return;
            }

            PartnerTypeComboBox.SelectedValue = partner.PartnerTypeId;
            NameTextBox.Text = partner.Name;
            RatingTextBox.Text = partner.Rating.ToString();
            AddressTextBox.Text = partner.LegalAddress;
            DirectorTextBox.Text = partner.Director;
            PhoneTextBox.Text = partner.Phone;
            EmailTextBox.Text = partner.Email;
            InnTextBox.Text = partner.Inn;
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось загрузить данные партнера.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
            Close();
        }
    }

    private bool ValidateInput(out int rating)
    {
        rating = 0;

        if (PartnerTypeComboBox.SelectedItem is not PartnerType)
        {
            ShowMessage("Выберите тип партнера.", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        var requiredFields = new (string Value, string Message)[]
        {
            (NameTextBox.Text, "Укажите наименование партнера."),
            (AddressTextBox.Text, "Укажите адрес партнера."),
            (DirectorTextBox.Text, "Укажите ФИО директора."),
            (PhoneTextBox.Text, "Укажите телефон партнера."),
            (EmailTextBox.Text, "Укажите email партнера."),
            (InnTextBox.Text, "Укажите ИНН партнера.")
        };

        foreach (var field in requiredFields)
        {
            if (!string.IsNullOrWhiteSpace(field.Value))
            {
                continue;
            }

            ShowMessage(field.Message, "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        if (!int.TryParse(RatingTextBox.Text.Trim(), out rating) || rating < 0)
        {
            ShowMessage("Укажите корректный рейтинг (целое число 0 и больше).", "Предупреждение", MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void SavePartner()
    {
        if (!ValidateInput(out var rating))
        {
            return;
        }

        var selectedPartnerType = (PartnerType)PartnerTypeComboBox.SelectedItem!;

        var partner = new Partner
        {
            Id = _partnerId ?? 0,
            PartnerTypeId = selectedPartnerType.Id,
            Name = NameTextBox.Text.Trim(),
            Director = DirectorTextBox.Text.Trim(),
            Email = EmailTextBox.Text.Trim(),
            Phone = PhoneTextBox.Text.Trim(),
            LegalAddress = AddressTextBox.Text.Trim(),
            Inn = InnTextBox.Text.Trim(),
            Rating = rating
        };

        try
        {
            AppData.SavePartner(partner);
            ShowMessage("Данные партнера успешно сохранены.", "Информация", MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception exception)
        {
            ShowMessage($"Не удалось сохранить данные партнера.\n{exception.Message}", "Ошибка", MessageBoxImage.Error);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SavePartner();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
