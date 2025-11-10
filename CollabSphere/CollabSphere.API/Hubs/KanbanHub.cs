using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.JoinWorkspace;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CollabSphere.API.Hubs
{
    [Authorize]
    public class KanbanHub : Hub
    {
        private readonly IMediator _mediator;
        private bool _isAuthoried;

        public KanbanHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Helper function using for get RequesterID
        /// </summary>
        /// <returns>UserId - INT</returns>
        private int GetUserId()
        {
            return int.Parse(Context.UserIdentifier!);
        }
        #region WORKSPACE
        //Join in workspace
        public async Task JoinWorkspace(int workspaceId)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Send command
                var command = new JoinWorkspaceCommand
                {
                    WorkspaceId = workspaceId,
                    UserId = userId
                };
                var result = await _mediator.Send(command);

                //Connect requester
                await Groups.AddToGroupAsync(Context.ConnectionId, workspaceId.ToString());
            }
            catch (Exception ex)
            {
                throw new HubException("Failed to join workspace!");
            }
        }

        //Leave workspace
        public async Task LeaveWorkspace(int workspaceId)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Send command
                var command = new LeaveWorkspaceCommand
                {
                    WorkspaceId = workspaceId,
                    UserId = userId
                };
                var result = await _mediator.Send(command);

                //Remove requester
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, workspaceId.ToString());
            }
            catch (Exception ex)
            {
                throw new HubException("Failed to join workspace!");
            }
        }
        #endregion

        #region LIST
        public async Task CreateList(int workspaceId, CreateListCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveListCreated", result.newListDto);
            }
            catch (Exception ex)
            {
                throw new HubException("Fail to create new list");
            }
        }

        //public async Task MoveList(string workspaceId, int listId, MoveListCommand command)
        //{
        //    //Handle logic 

        //    await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveListMoved", listId, newPosition);
        //}

        //public async Task RenameList(string workspaceId, int listId, RenameListCommand command)
        //{
        //    //Handle logic 

        //    await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveListRenamed", listId, newTitle);
        //}
        #endregion

        #region CARD
        //public async Task CreateCard(string workspaceId, CreateCardCommand command)
        //{
        //    //Handle logic 

        //    await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveCardCreated", newCard);
        //}

        //public async Task MoveCard(string workspaceId, MoveCardCommand command)
        //{
        //    //Handle logic 

        //    await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveCardMoved", cardId, newListId, newPosition);
        //}


        //public async Task UpdateCardDetails(string workspaceId, UpdateCardDetailCommand command)
        //{
        //    //Handle logic 

        //    await Clients.OthersInGroup(workspaceId).SendAsync("ReceiveCardUpdated", updatedCard);
        //}
        #endregion
    }
}
