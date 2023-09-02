namespace CameraLoader.Config;

public enum PresetSortingMode
{
    CreationDate,
    NameAscend,
    NameDescend,
}

static class PresetSortingModeEx
{
    public static string GetDescription(this PresetSortingMode value)
    {
        return value switch
        {
            PresetSortingMode.CreationDate => "Creation Date",
            PresetSortingMode.NameAscend => "Name (Ascending)",
            PresetSortingMode.NameDescend => "Name (Descending)",
            _ => value.ToString(),
        };
    }
}