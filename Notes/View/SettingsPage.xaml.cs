using Microsoft.Maui.Storage;
using Notes.Services;

namespace Notes.View;

public partial class SettingsPage : ContentPage
{
    private readonly NoteService _service = new();
    private Picker _sortPicker;
    private Label _totalNotesLabel;
    private Label _favoritesCountLabel;
    private Label _totalImagesLabel;

    public SettingsPage()
    {
        InitializeComponent();

        _sortPicker = this.FindByName<Picker>("SortPicker");
        _totalNotesLabel = this.FindByName<Label>("TotalNotesLabel");
        _favoritesCountLabel = this.FindByName<Label>("FavoritesCountLabel");
        _totalImagesLabel = this.FindByName<Label>("TotalImagesLabel");

        LoadTheme();
        _ = UpdateStatistics();

        MessagingCenter.Subscribe<MainPage>(this, "NotesChanged", async _ => await UpdateStatistics());
        MessagingCenter.Subscribe<FavoritesPage>(this, "FavoritesChanged", async _ => await UpdateStatistics());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await UpdateStatistics();
    }

    private void LoadTheme()
    {
        var savedTheme = Preferences.Get("app_theme", "system");
        ThemePicker.SelectedIndex = savedTheme switch
        {
            "light" => 0,
            "dark" => 1,
            _ => 2
        };

        var savedSort = Preferences.Get("sort_type", "date_new");
        if (_sortPicker != null)
        {
            _sortPicker.SelectedIndexChanged -= OnSortChanged;
            _sortPicker.SelectedIndex = savedSort switch
            {
                "date_new" => 0,
                "date_old" => 1,
                "name_asc" => 2,
                "name_desc" => 3,
                _ => 0
            };
            _sortPicker.SelectedIndexChanged += OnSortChanged;
        }
    }

    private void OnThemeChanged(object sender, EventArgs e)
    {
        var theme = ThemePicker.SelectedIndex switch
        {
            0 => (AppTheme.Light, "light"),
            1 => (AppTheme.Dark, "dark"),
            _ => (AppTheme.Unspecified, "system")
        };

        Application.Current!.UserAppTheme = theme.Item1;
        Preferences.Set("app_theme", theme.Item2);

        // Подпишись на событие смены темы
        Application.Current.RequestedThemeChanged += (s, args) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdatePageBackgroundColor();
            });
        };

        UpdatePageBackgroundColor();
    }

    private void UpdatePageBackgroundColor()
    {
        var isDark = Application.Current!.UserAppTheme == AppTheme.Dark ||
                     (Application.Current.UserAppTheme == AppTheme.Unspecified &&
                      Application.Current.PlatformAppTheme == AppTheme.Dark);

        this.BackgroundColor = isDark
            ? Color.FromArgb("#0F1419")
            : Colors.White;
    }

    private void OnSortChanged(object sender, EventArgs e)
    {
        if (sender is not Picker picker) return;

        var sortType = picker.SelectedIndex switch
        {
            0 => "date_new",
            1 => "date_old",
            2 => "name_asc",
            3 => "name_desc",
            _ => "date_new"
        };

        Preferences.Set("sort_type", sortType);
        MessagingCenter.Send(this, "SortChanged");
    }

    private async Task UpdateStatistics()
    {
        try
        {
            var notes = await _service.GetNotesAsync();
            var totalNotes = notes.Count;
            var favoriteNotes = notes.Count(n => n.IsFavorite);
            var totalImages = notes.Sum(n => n.Images?.Count ?? 0);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_totalNotesLabel != null)
                    _totalNotesLabel.Text = $"Всего заметок: {totalNotes}";

                if (_favoritesCountLabel != null)
                    _favoritesCountLabel.Text = $"Заметок в избранном: {favoriteNotes}";

                if (_totalImagesLabel != null)
                    _totalImagesLabel.Text = $"Всего изображений: {totalImages}";
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "Ok");
        }
    }

    private async void OnClearAllClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Удаление",
            "Вы уверены? Это действие нельзя отменить!",
            "Да, удалить всё", "Отмена");

        if (!confirm) return;

        await _service.SaveNotesAsync(new List<Notes.Model.Note>());
        MessagingCenter.Send(this, "NotesChanged");
        await UpdateStatistics();
        await DisplayAlert("Готово", "Все заметки удалены", "Ок");
    }

    private Color GetBackgroundColor()
    {
        var isDark = Application.Current!.UserAppTheme == AppTheme.Dark ||
                     (Application.Current.UserAppTheme == AppTheme.Unspecified &&
                      Application.Current.PlatformAppTheme == AppTheme.Dark);

        return isDark
            ? Color.FromArgb("#0F1419")
            : Colors.White;
    }
}