using NicolasQuiPaieData.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace NicolasQuiPaieWeb.Services;

/// <summary>
/// Client-side service for comment operations via API
/// </summary>
public class ApiCommentService(HttpClient httpClient, ILogger<ApiCommentService> logger)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Récupère tous les commentaires d'une proposition
    /// </summary>
    public async Task<IEnumerable<CommentDto>> GetCommentsForProposalAsync(int proposalId)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<List<CommentDto>>($"/api/comments/proposal/{proposalId}", _jsonOptions);
            return response ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des commentaires pour la proposition {ProposalId}", proposalId);
            return [];
        }
    }

    /// <summary>
    /// Crée un nouveau commentaire
    /// </summary>
    public async Task<CommentDto?> CreateCommentAsync(CreateCommentDto createDto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/comments", createDto, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CommentDto>(_jsonOptions);
            }
            
            logger.LogWarning("Échec de la création du commentaire. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la création du commentaire");
            return null;
        }
    }

    /// <summary>
    /// Met à jour un commentaire existant
    /// </summary>
    public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto updateDto)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"/api/comments/{commentId}", updateDto, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CommentDto>(_jsonOptions);
            }
            
            logger.LogWarning("Échec de la mise à jour du commentaire {CommentId}. Status: {StatusCode}", commentId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la mise à jour du commentaire {CommentId}", commentId);
            return null;
        }
    }

    /// <summary>
    /// Supprime un commentaire
    /// </summary>
    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/api/comments/{commentId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la suppression du commentaire {CommentId}", commentId);
            return false;
        }
    }
}

/// <summary>
/// Client-side wrapper service for comment operations
/// </summary>
public class CommentService(ApiCommentService apiCommentService)
{
    public async Task<IEnumerable<CommentDto>> GetCommentsForProposalAsync(int proposalId)
        => await apiCommentService.GetCommentsForProposalAsync(proposalId);

    public async Task<CommentDto?> CreateCommentAsync(CreateCommentDto createDto)
        => await apiCommentService.CreateCommentAsync(createDto);

    public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto updateDto)
        => await apiCommentService.UpdateCommentAsync(commentId, updateDto);

    public async Task<bool> DeleteCommentAsync(int commentId)
        => await apiCommentService.DeleteCommentAsync(commentId);
}