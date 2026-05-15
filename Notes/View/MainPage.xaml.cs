using Notes.Model;
using Notes.Services;
using Notes.View;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;

namespace Notes;

public partial class MainPage : ContentPage
{
    private readonly ObservableCollection<Note> _notes = new();
    private readonly NoteService _service = new();
    private List<Note> _allNotes = new();
    private bool _isLoaded = false;
    private string _currentSort = "date_new";

    public MainPage()
    {
        InitializeComponent();
        NotesCollection.ItemsSource = _notes;

        MessagingCenter.Subscribe<NotePage>(this, "NotesChanged", async _ =>
        {
            _isLoaded = false;
            await LoadNotesAsync();
        });

        MessagingCenter.Subscribe<FavoritesPage>(this, "FavoritesChanged", async _ =>
        {
            _isLoaded = false;
            await LoadNotesAsync();
        });

        MessagingCenter.Subscribe<SettingsPage>(this, "SortChanged", _ =>
        {
            ApplyCurrentSort();
        });

        MessagingCenter.Subscribe<SettingsPage>(this, "NotesChanged", async _ =>
        {
            _isLoaded = false;
            await LoadNotesAsync();
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded)
            await LoadNotesAsync();
    }

    private async Task LoadNotesAsync()
    {
        _allNotes = await _service.GetNotesAsync();
        ApplyCurrentSort();

        EmptyNotesStack.IsVisible = _notes.Count == 0;
        _isLoaded = true;
    }

    private void ApplyCurrentSort()
    {
        _currentSort = Preferences.Get("sort_type", "date_new");
        var sorted = ApplySorting(_allNotes);

        _notes.Clear();
        foreach (var note in sorted)
            _notes.Add(note);

        EmptyNotesStack.IsVisible = _notes.Count == 0;
    }

    private List<Note> ApplySorting(List<Note> notes)
    {
        return _currentSort switch
        {
            "date_new" => notes.OrderByDescending(n => n.CreatedAt).ToList(),
            "date_old" => notes.OrderBy(n => n.CreatedAt).ToList(),
            "name_asc" => notes.OrderBy(n => n.DisplayTitle).ToList(),
            "name_desc" => notes.OrderByDescending(n => n.DisplayTitle).ToList(),
            _ => notes.OrderByDescending(n => n.CreatedAt).ToList()
        };
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NotePage(null));
    }

    private async void OnNoteTapped(object sender, EventArgs e)
    {
        var note = (sender as Element)?.BindingContext as Note;
        if (note != null)
            await Navigation.PushAsync(new NotePage(note));
    }

    private async void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.BindingContext is Note note)
        {
            note.IsFavorite = !note.IsFavorite;
            await _service.SaveNotesAsync(_allNotes);
            MessagingCenter.Send(this, "FavoritesChanged");
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue?.ToLower() ?? "";
        _notes.Clear();

        var filtered = string.IsNullOrEmpty(text)
            ? _allNotes
            : _allNotes.Where(n => n.DisplayTitle.ToLower().Contains(text));

        var sorted = ApplySorting(filtered.ToList());

        foreach (var note in sorted)
            _notes.Add(note);

        EmptyNotesStack.IsVisible = _notes.Count == 0;
    }

    private async void OnDeleteNote(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipe && swipe.BindingContext is Note note)
        {
            bool confirm = await DisplayAlert("Удаление", "Удалить эту заметку?", "Да", "Нет");
            if (!confirm) return;

            _allNotes.Remove(note);
            _notes.Remove(note);
            await _service.SaveNotesAsync(_allNotes);

            EmptyNotesStack.IsVisible = _notes.Count == 0;
            MessagingCenter.Send(this, "FavoritesChanged");
        }
    }
}