using Notes.Model;
using Notes.Services;
using System.Collections.ObjectModel;

namespace Notes.View;

public partial class FavoritesPage : ContentPage
{
    private readonly NoteService _service = new();
    private readonly ObservableCollection<Note> _favorites = new();
    private bool _isLoaded = false;

    public FavoritesPage()
    {
        InitializeComponent();
        FavoritesCollection.ItemsSource = _favorites;

        MessagingCenter.Subscribe<MainPage>(this, "FavoritesChanged", async _ =>
        {
            _isLoaded = false;
            await LoadFavoritesAsync();
        });

        MessagingCenter.Subscribe<NotePage>(this, "NotesChanged", async _ =>
        {
            _isLoaded = false;
            await LoadFavoritesAsync();
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded)
            await LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        var notes = await _service.GetNotesAsync();
        _favorites.Clear();
        foreach (var note in notes.Where(n => n.IsFavorite))
            _favorites.Add(note);

        UpdateEmptyState();
        _isLoaded = true;
    }

    private void UpdateEmptyState()
    {
        EmptyFavoritesStack.IsVisible = _favorites.Count == 0;
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
            var notes = await _service.GetNotesAsync();
            var existingNote = notes.FirstOrDefault(n => n.Id == note.Id);

            if (existingNote != null)
            {
                existingNote.IsFavorite = false;
                await _service.SaveNotesAsync(notes);
                _favorites.Remove(note);
                UpdateEmptyState();
                MessagingCenter.Send(this, "FavoritesChanged");
            }
        }
    }

    private async void OnDeleteNote(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipe && swipe.BindingContext is Note note)
        {
            bool confirm = await DisplayAlert("Удаление", "Удалить заметку?", "Да", "Нет");
            if (!confirm) return;

            var notes = await _service.GetNotesAsync();
            notes.RemoveAll(n => n.Id == note.Id);
            await _service.SaveNotesAsync(notes);
            _favorites.Remove(note);
            UpdateEmptyState();
            MessagingCenter.Send(this, "NotesChanged");
            MessagingCenter.Send(this, "FavoritesChanged");
        }
    }
}