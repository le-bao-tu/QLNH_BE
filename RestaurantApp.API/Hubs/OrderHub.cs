using Microsoft.AspNetCore.SignalR;

namespace RestaurantApp.API.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinBranch(string branchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, branchId);
        }
    }
}
