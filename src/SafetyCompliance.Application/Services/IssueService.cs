using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Domain.Entities;
using SafetyCompliance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SafetyCompliance.Application.Services;

public class IssueService(ApplicationDbContext context) : IIssueService
{
    public async Task<List<IssueDto>> GetIssuesAsync(IssueStatus? status = null, IssuePriority? priority = null, int? equipmentId = null, CancellationToken ct = default)
    {
        var query = context.Issues.AsQueryable();

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);
        if (priority.HasValue)
            query = query.Where(i => i.Priority == priority.Value);
        if (equipmentId.HasValue)
            query = query.Where(i => i.EquipmentId == equipmentId.Value);

        return await query
            .OrderByDescending(i => i.Priority)
            .ThenByDescending(i => i.CreatedAt)
            .Select(i => new IssueDto(
                i.Id, i.Title, i.Description, i.Priority, i.Status,
                i.AssignedTo, i.DueDate, i.ResolvedAt,
                i.ResolvedBy != null ? i.ResolvedBy.FirstName + " " + i.ResolvedBy.LastName : null,
                i.InspectionRoundId, i.EquipmentInspectionId, i.EquipmentId,
                i.Equipment != null ? i.Equipment.Identifier : null,
                i.Equipment != null ? i.Equipment.EquipmentType.Name : null,
                i.PhotoBase64, i.PhotoFileName,
                i.CreatedById,
                i.CreatedAt,
                i.Comments.Count))
            .ToListAsync(ct);
    }

    public async Task<IssueDto?> GetIssueByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Issues
            .Where(i => i.Id == id)
            .Select(i => new IssueDto(
                i.Id, i.Title, i.Description, i.Priority, i.Status,
                i.AssignedTo, i.DueDate, i.ResolvedAt,
                i.ResolvedBy != null ? i.ResolvedBy.FirstName + " " + i.ResolvedBy.LastName : null,
                i.InspectionRoundId, i.EquipmentInspectionId, i.EquipmentId,
                i.Equipment != null ? i.Equipment.Identifier : null,
                i.Equipment != null ? i.Equipment.EquipmentType.Name : null,
                i.PhotoBase64, i.PhotoFileName,
                i.CreatedById,
                i.CreatedAt,
                i.Comments.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IssueDto> CreateIssueAsync(IssueCreateDto dto, string userId, CancellationToken ct = default)
    {
        var issue = new Issue
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            AssignedTo = dto.AssignedTo,
            DueDate = dto.DueDate,
            InspectionRoundId = dto.InspectionRoundId,
            EquipmentInspectionId = dto.EquipmentInspectionId,
            EquipmentId = dto.EquipmentId,
            PhotoBase64 = dto.PhotoBase64,
            PhotoFileName = dto.PhotoFileName,
            CreatedById = userId
        };

        context.Issues.Add(issue);
        await context.SaveChangesAsync(ct);

        return (await GetIssueByIdAsync(issue.Id, ct))!;
    }

    public async Task UpdateIssueAsync(IssueUpdateDto dto, string userId, CancellationToken ct = default)
    {
        var issue = await context.Issues.FindAsync([dto.Id], ct)
            ?? throw new InvalidOperationException($"Issue {dto.Id} not found");

        issue.Title = dto.Title;
        issue.Description = dto.Description;
        issue.Priority = dto.Priority;
        issue.Status = dto.Status;
        issue.AssignedTo = dto.AssignedTo;
        issue.DueDate = dto.DueDate;
        issue.ModifiedById = userId;
        issue.ModifiedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task ResolveIssueAsync(int id, string? resolutionNotes, string userId, CancellationToken ct = default)
    {
        var issue = await context.Issues.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Issue {id} not found");

        issue.Status = IssueStatus.Resolved;
        issue.ResolvedAt = DateTime.UtcNow;
        issue.ResolvedById = userId;
        issue.ModifiedById = userId;
        issue.ModifiedAt = DateTime.UtcNow;

        if (resolutionNotes is not null)
        {
            context.Comments.Add(new Comment
            {
                IssueId = id,
                Text = "Resolved: " + resolutionNotes,
                CreatedById = userId
            });
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task ReopenIssueAsync(int id, string userId, CancellationToken ct = default)
    {
        var issue = await context.Issues.FindAsync([id], ct)
            ?? throw new InvalidOperationException($"Issue {id} not found");

        issue.Status = IssueStatus.Open;
        issue.ResolvedAt = null;
        issue.ResolvedById = null;
        issue.ModifiedById = userId;
        issue.ModifiedAt = DateTime.UtcNow;

        context.Comments.Add(new Comment
        {
            IssueId = id,
            Text = "Issue reopened",
            CreatedById = userId
        });

        await context.SaveChangesAsync(ct);
    }

    public async Task<List<CommentDto>> GetCommentsAsync(int issueId, CancellationToken ct = default)
    {
        return await context.Comments
            .Where(c => c.IssueId == issueId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(
                c.Id, c.Text, c.PhotoBase64, c.PhotoFileName,
                c.CreatedById, c.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<CommentDto> AddCommentAsync(CommentCreateDto dto, string userId, CancellationToken ct = default)
    {
        var comment = new Comment
        {
            IssueId = dto.IssueId,
            Text = dto.Text,
            PhotoBase64 = dto.PhotoBase64,
            PhotoFileName = dto.PhotoFileName,
            CreatedById = userId
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync(ct);

        return new CommentDto(comment.Id, comment.Text, comment.PhotoBase64, comment.PhotoFileName,
            userId, comment.CreatedAt);
    }

    public async Task<List<CommentDto>> GetInspectionCommentsAsync(int inspectionRoundId, CancellationToken ct = default)
    {
        return await context.Comments
            .Where(c => c.InspectionRoundId == inspectionRoundId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(
                c.Id, c.Text, c.PhotoBase64, c.PhotoFileName,
                c.CreatedById, c.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<CommentDto> AddInspectionCommentAsync(CommentCreateDto dto, string userId, CancellationToken ct = default)
    {
        var comment = new Comment
        {
            InspectionRoundId = dto.InspectionRoundId,
            Text = dto.Text,
            PhotoBase64 = dto.PhotoBase64,
            PhotoFileName = dto.PhotoFileName,
            CreatedById = userId
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync(ct);

        return new CommentDto(comment.Id, comment.Text, comment.PhotoBase64, comment.PhotoFileName,
            userId, comment.CreatedAt);
    }
}
