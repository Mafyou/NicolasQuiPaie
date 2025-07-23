using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace NicolasQuiPaieWeb.Hubs
{
    [Authorize]
    public class VotingHub : Hub
    {
        public async Task JoinProposal(string proposalId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"proposal_{proposalId}");
        }

        public async Task LeaveProposal(string proposalId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"proposal_{proposalId}");
        }

        public async Task JoinGlobalUpdates()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "global_updates");
        }

        public async Task LeaveGlobalUpdates()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "global_updates");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}