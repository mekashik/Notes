using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Notes.Model;

public class Note : INotifyPropertyChanged
{
    private bool _isFavorite;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite != value)
            {
                _isFavorite = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<string> Images { get; set; } = new List<string>();

    public string DisplayTitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Title))
                return Title;

            if (!string.IsNullOrWhiteSpace(Content))
                return Content.Split(' ').FirstOrDefault() ?? "Без названия";

            return "Без названия";
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}