using System.Text.Json;
using Notes.Model;

namespace Notes.Services;

public class NoteService
{
    private string FilePath => Path.Combine(FileSystem.AppDataDirectory, "notes.json");

    public async Task<List<Note>> GetNotesAsync()
    {
        if (!File.Exists(FilePath))
            return new List<Note>();

        var json = await File.ReadAllTextAsync(FilePath);
        return JsonSerializer.Deserialize<List<Note>>(json) ?? new List<Note>();
    }

    public async Task SaveNotesAsync(List<Note> notes)
    {
        var json = JsonSerializer.Serialize(notes);
        await File.WriteAllTextAsync(FilePath, json);
    }
}
