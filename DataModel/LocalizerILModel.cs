namespace Localizer.DataModel;
public class LocalizerILModel
{
    public string TypeName { get; set; }

    public string MethodName { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }

    public LocalizerILModel Formate()
    {
        NewValue = NewValue[1..^1];
        OldValue = OldValue[1..^1];
        NewValue = NewValue.Replace(@"\n", "\n"); // => \\n -> \n
        OldValue = OldValue.Replace(@"\n", "\n");
        return this;
    }
}