namespace NicolasQuiPaieAPI.Application.Results;

// Outcomes of the proposal and voting services. They replace the exceptions (ArgumentException,
// UnauthorizedAccessException) and the nulls the endpoints used to translate into 404 and 403: a switch on a union
// must name every case.

/// <summary>No proposal has this id.</summary>
public sealed record ProposalNotFound(int ProposalId);

/// <summary>The user is not the author of the proposal they try to change or delete.</summary>
public sealed record NotProposalAuthor(int ProposalId, string UserId);

/// <summary>The proposal was deleted.</summary>
public sealed record ProposalDeleted(int ProposalId);

/// <summary>No category has this id: a proposal cannot be filed under it.</summary>
public sealed record UnknownCategory(int CategoryId);

/// <summary>A proposal created by a user, or refused because its category does not exist.</summary>
public readonly union ProposalCreation(ProposalDto, UnknownCategory);

/// <summary>A proposal read by its id.</summary>
public readonly union ProposalLookup(ProposalDto, ProposalNotFound);

/// <summary>A proposal changed by its author (update), or by a SuperUser/Admin (status, where authorship is not checked).</summary>
public readonly union ProposalChange(ProposalDto, ProposalNotFound, NotProposalAuthor);

/// <summary>A proposal deleted by its author.</summary>
public readonly union ProposalDeletion(ProposalDeleted, ProposalNotFound, NotProposalAuthor);

/// <summary>The user has no vote on this proposal.</summary>
public sealed record NoVote(string UserId, int ProposalId);

/// <summary>The vote was removed and the proposal's counts updated.</summary>
public sealed record VoteRemoved(string UserId, int ProposalId);

/// <summary>The vote of a user on a proposal.</summary>
public readonly union VoteLookup(VoteDto, NoVote);

/// <summary>A vote removed by its user.</summary>
public readonly union VoteRemoval(VoteRemoved, NoVote);
