using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Notes.View;

public partial class ImagePage : ContentPage
{
    public ObservableCollection<string> Images { get; private set; }
    public int Position { get; private set; }
    public bool IsSwipeEnabled => Images.Count > 1;
    public event Action<List<string>> ImagesUpdated;

    public ImagePage(List<string> images, int startIndex)
    {
        InitializeComponent();

        Images = new ObservableCollection<string>(images);
        Position = startIndex;
        BindingContext = this;

        Carousel.Position = startIndex;
        Carousel.IsSwipeEnabled = Images.Count > 1;

        Carousel.PositionChanged += (s, e) =>
        {
            foreach (var item in Carousel.VisibleViews)
            {
                if (item is Grid grid && grid.Children.FirstOrDefault() is Image img)
                    img.Scale = 1; // сброс масштаба
            }
        };
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (Images.Count == 0) return;

        bool confirm = await DisplayAlert("Удаление", "Удалить это изображение?", "Да", "Нет");
        if (!confirm) return;

        Images.RemoveAt(Carousel.Position);
        ImagesUpdated?.Invoke(Images.ToList());

        if (Images.Count == 0)
            await Navigation.PopAsync();
        else
            Carousel.IsSwipeEnabled = Images.Count > 1;
    }
}