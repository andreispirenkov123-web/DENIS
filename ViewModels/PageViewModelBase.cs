namespace HouseholdGoodsIS.ViewModels;

public abstract class PageViewModelBase : ViewModelBase
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        protected set => SetProperty(ref _title, value);
    }

    private string _glyph = string.Empty;
    public string Glyph
    {
        get => _glyph;
        protected set => SetProperty(ref _glyph, value);
    }

    public abstract void Refresh();
}
