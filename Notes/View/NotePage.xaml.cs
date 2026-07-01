using Microsoft.Maui.Storage;
using Notes.Model;
using Notes.Services;
using Notes.View;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Notes;

// Model для элемента в коллекции
public class ImageItem
{
    public string ImagePath { get; set; }
    public bool IsImage { get; set; }
    public bool IsAddButton { get; set; }
}

public partial class NotePage : ContentPage
{
    private readonly NoteService _service = new();
    private Note _note;
    private bool _isNew;
    private ObservableCollection<ImageItem> _imageItems = new();
    private List<string> _images = new();
    private bool _hasChanges;

    public NotePage(Note note)
    {
        InitializeComponent();

        if (note == null)
        {
            _note = new Note();
            _isNew = true;
            _images = new List<string>();
        }
        else
        {
            _note = note;
            _isNew = false;
            TitleEntry.Text = note.Title;
            ContentEditor.Text = note.Content;
            _images = new List<string>(_note.Images ?? new List<string>());
        }

        // Инициализируем коллекцию изображений с кнопкой добавления в начале
        UpdateImageCollection();

        ImagesCollection.ItemsSource = _imageItems;

        TitleEntry.TextChanged += (s, e) => _hasChanges = true;
        ContentEditor.TextChanged += (s, e) => _hasChanges = true;
    }

    private void UpdateImageCollection()
    {
        _imageItems.Clear();

        // Добавляем кнопку добавления в начало
        _imageItems.Add(new ImageItem { IsAddButton = true, IsImage = false });

        // Добавляем существующие изображения
        foreach (var image in _images)
        {
            _imageItems.Add(new ImageItem { ImagePath = image, IsImage = true, IsAddButton = false });
        }
    }

    protected override bool OnBackButtonPressed()
    {
        HandleBackAsync();
        return true;
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await HandleBackAsync();
    }

    private async Task HandleBackAsync()
    {
        if (!_hasChanges)
        {
            await Navigation.PopAsync();
            return;
        }

        if (await DisplayAlert("Сохранить заметку?", "Вы хотите сохранить изменения?", "Да", "Нет"))
            await SaveNoteAsync();

        await Navigation.PopAsync();
    }

    private async Task SaveNoteAsync()
    {
        _note.Title = TitleEntry.Text?.Trim();
        _note.Content = ContentEditor.Text?.Trim();
        _note.Images = _images;

        var notes = await _service.GetNotesAsync();

        if (_isNew)
        {
            notes.Add(_note);
            _isNew = false;
        }
        else
        {
            var existing = notes.FirstOrDefault(n => n.Id == _note.Id);
            if (existing != null)
            {
                existing.Title = _note.Title;
                existing.Content = _note.Content;
                existing.Images = _note.Images;
                existing.IsFavorite = _note.IsFavorite;
            }
        }

        await _service.SaveNotesAsync(notes);
        MessagingCenter.Send(this, "NotesChanged");
        _hasChanges = false;
    }

    private async void OnAddImageClicked(object sender, EventArgs e)
    {
        try
        {
            // Используем FilePicker.Default для выбора нескольких файлов
            var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Выберите изображения"
            });

            if (results == null || !results.Any()) return;

            // Добавляем все выбранные изображения в список
            foreach (var result in results)
            {
                if (!_images.Contains(result.FullPath))
                {
                    _images.Add(result.FullPath);
                }
            }

            UpdateImageCollection();
            _hasChanges = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "Ok");
        }
    }

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        if (sender is Image img && e.Parameter is string path)
        {
            int index = _images.IndexOf(path);
            var page = new ImagePage(_images, index);
            page.ImagesUpdated += updated =>
            {
                _images = updated;
                UpdateImageCollection();
                _hasChanges = true;
            };
            await Navigation.PushAsync(page);
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await SaveNoteAsync();
        await DisplayAlert("Сохранено", "Заметка успешно сохранена", "Ok");
        await Navigation.PopAsync();
    }
}