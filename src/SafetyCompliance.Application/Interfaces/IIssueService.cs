using SafetyCompliance.Application.DTOs;
using SafetyCompliance.Domain.Entities;

namespace SafetyCompliance.Application.Interfaces;

public interface IIssueService
{
    Task<List<IssueDto>> GetIssuesAsync(IssueStatus? status = null, IssuePriority? priority = null, int? equipmentId = null, CancellationToken ct = default);
    Task<IssueDto?> GetIssueByIdAsync(int id, CancellationToken ct = default);
    Task<IssueDto> CreateIssueAsync(IssueCreateDto dto, string userId, CancellationToken ct = default);
    Task UpdateIssueAsync(IssueUpdateDto dto, string userId, CancellationToken ct = default);
    Task ResolveIssueAsync(int id, string? resolutionNotes, string userId, CancellationToken ct = default);
    Task ReopenIssueAsync(int id, string userId, CancellationToken ct = default);
    Task<List<CommentDto>> GetCommentsAsync(int issueId, CancellationToken ct = default);
    Task<CommentDto> AddCommentAsync(CommentCreateDto dto, string userId, CancellationToken ct = default);
    Task<List<CommentDto>> GetInspectionCommentsAsync(int inspectionRoundId, CancellationToken ct = default);
    Task<CommentDto> AddInspectionCommentAsync(CommentCreateDto dto, string userId, CancellationToken ct = default);
}
