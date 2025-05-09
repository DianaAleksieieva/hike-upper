public class Achievement
{
    public string Caption { get; }
    // we no longer store or need imageUrl here

    public Achievement(string caption)
    {
        Caption = caption;
    }
}