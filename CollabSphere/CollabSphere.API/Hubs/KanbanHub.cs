using Microsoft.AspNetCore.SignalR;

namespace CollabSphere.API.Hubs
{
    public class KanbanHub : Hub
    {
        public KanbanHub()
        {
            
        }

        #region WORKSPACE
        //Join in workspace
        public async Task JoinWorkspace(string workspaceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, workspaceId);
        }

        //Leave workspace
        public async Task LeaveWorkspace(string workspaceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, workspaceId);
        }
        #endregion

        #region LIST
        public async Task CreateList(string workspaceId, CreateListCommand command)
        {
            //Handle logic 


            await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveListCreated", newList);
        }

        public async Task MoveList(string workspaceId, int listId, MoveListCommand command)
        {
            //Handle logic 

            await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveListMoved", listId, newPosition);
        }

        public async Task RenameList(string workspaceId, int listId, RenameListCommand command)
        {
            //Handle logic 

            await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveListRenamed", listId, newTitle);
        }
        #endregion

        #region CARD
        public async Task CreateCard(string workspaceId, CreateCardCommand command)
        {
            //Handle logic 

            await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveCardCreated", newCard);
        }

        public async Task MoveCard(string workspaceId, int cardId, int newListId, int newPosition)
        {
            //Handle logic 

            await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveCardMoved", cardId, newListId, newPosition);
        }

     
        public async Task UpdateCardDetails(string workspaceId, Card updatedCard)
        {
            //Handle logic 

            await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveCardUpdated", updatedCard);
        }
        #endregion
    }
}
