using CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.CreateCardAndAssignMember;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.MoveCard;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.UpdateCardDetails;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.CreateList;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.MoveList;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.RenameList;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.TaskCommands;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.WorkSpaceCommands.JoinWorkspace;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.WorkSpaceCommands.LeaveWorkspace;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

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
            catch (Exception)
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
            catch (Exception)
            {
                throw new HubException("Failed to leave workspace!");
            }
        }
        #endregion

        #region LIST
        //Create new List
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
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveListCreated", result.NewListDto);
            }
            catch (Exception)
            {
                throw new HubException("Fail to create new list");
            }
        }

        //Move List
        public async Task MoveList(int workspaceId, int listId, MoveListCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;
                command.ListId = listId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveListMoved", command.ListId, command.NewPosition);
            }
            catch (Exception)
            {
                throw new HubException("Fail to move the list");
            }
        }

        //Rename List
        public async Task RenameList(int workspaceId, int listId, RenameListCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;
                command.ListId = listId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveListRenamed", command.ListId, command.NewTitle);
            }
            catch (Exception)
            {
                throw new HubException("Fail to move the list");
            }
        }
        #endregion

        #region CARD

        //Create Card and assign member to card
        public async Task CreateCardAndAssignMember(int workspaceId, int listId, CreateCardCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;
                command.ListId = listId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardCreated", result.CreatedCardDto);
            }
            catch (Exception)
            {
                throw new HubException("Fail to create new card or assign members to card");
            }
        }

        //Move Card
        public async Task MoveCard(int workspaceId, int listId, int cardId, MoveCardCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardMoved", command.CardId, command.NewListId, command.NewPosition);
            }
            catch (Exception)
            {
                throw new HubException("Fail to move the list");
            }
        }

        //Update Cart Details
        public async Task UpdateCardDetails(int workspaceId, int listId, int cardId, UpdateCardDetailsCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardUpdated", result.UpdatedCardDto);
            }
            catch (Exception)
            {
                throw new HubException("Fail to move the list");
            }
        }
        #endregion

        #region Task
        //Create new Task
        public async Task CreateTask(int workspaceId, int listId, int cardId, CreateTaskCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveTaskCreated", result.CreatedTaskDto);
            }
            catch (Exception)
            {
                throw new HubException("Fail to create new task");
            }
        }

        //Rename Task
        public async Task RenameTask(int workspaceId, int listId, int cardId, int taskId, RenameTaskCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;
                command.TaskId = taskId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveTaskRenamed", command.ListId, command.CardId, command.TaskId, command.NewTitle);
            }
            catch (Exception)
            {
                throw new HubException("Fail to rename the task");
            }
        }

        //Delete Task
        public async Task DeleteTask(int workspaceId, int listId, int cardId, int taskId, DeleteTaskCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkSpaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;
                command.TaskId = taskId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveTaskDeleted", command.ListId, command.CardId, command.TaskId);
            }
            catch (Exception)
            {
                throw new HubException("Fail to delete the task");
            }
        }
        #endregion

        #region Sub-Task

        #endregion
    }
}
