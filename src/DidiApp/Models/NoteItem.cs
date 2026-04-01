using System;
using DidiApp.ViewModels;

namespace DidiApp.Models;

public sealed class NoteItem : ViewModelBase
{
    private Guid _id = Guid.NewGuid();
    private string _content = string.Empty;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public Guid Id { get => _id; set => SetProperty(ref _id, value); }
    public string Content { get => _content; set => SetProperty(ref _content, value); }
    public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }
    public DateTime UpdatedAt { get => _updatedAt; set => SetProperty(ref _updatedAt, value); }
}
