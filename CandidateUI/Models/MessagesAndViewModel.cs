using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace ScjWin11UI.Models;

public sealed class CandidateItem
{
    public int Index { get; init; }
    public string Text { get; init; } = "";

    // Display in UI as 1..9.. (or whatever you prefer)
    public string DisplayIndex => (Index + 1).ToString();
}

public readonly record struct ScreenRect(int X, int Y, int Width, int Height);

public sealed class CandidateState
{
    [JsonPropertyName("composition")]
    public string? Composition { get; set; }

    [JsonPropertyName("hint")]
    public string? Hint { get; set; }

    [JsonPropertyName("selectedIndex")]
    public int SelectedIndex { get; set; } = 0;

    [JsonPropertyName("candidates")]
    public List<string> Candidates { get; set; } = new();

    [JsonPropertyName("caret")]
    public ScreenRect? Caret { get; set; }
}

public sealed class ImeToUiMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("state")]
    public CandidateState? State { get; set; }
}

public sealed class UiToImeMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("delta")]
    public int? Delta { get; set; }
}

public sealed class CandidateViewModel : ObservableObject
{
    private string _composition = "";
    private int _selectedIndex;
    private string _hint = "";

    public string Composition
    {
        get => _composition;
        set => SetProperty(ref _composition, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    public string Hint
    {
        get => _hint;
        set => SetProperty(ref _hint, value);
    }

    public ObservableCollection<CandidateItem> Candidates { get; } = new();

    public void SetCandidates(List<string>? candidates)
    {
        Candidates.Clear();
        if (candidates is null) return;

        for (int i = 0; i < candidates.Count; i++)
        {
            Candidates.Add(new CandidateItem { Index = i, Text = candidates[i] });
        }
    }
}

/// <summary>
/// Tiny MVVM helper (no external dependencies).
/// </summary>
public abstract class ObservableObject : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        return true;
    }
}
