using CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.CreateCardAndAssignMember;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.DeleteCard;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.MoveCard;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.CardCommands.UpdateCardDetails;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.CardMemberCommands;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.CreateList;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.MoveList;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.RenameList;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.SubTaskCommands.CreateSubTask;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.SubTaskCommands.DeleteSubTask;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.SubTaskCommands.RenameSubTask;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.SubTaskCommands.UpdateSubTaskDetails;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.TaskCommands.CreateTask;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.TaskCommands.DeleteTask;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.TaskCommands.RenameTask;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.WorkSpaceCommands.JoinWorkspace;
using CollabSphere.Application.Features.TeamWorkSpace.Commands.WorkSpaceCommands.LeaveWorkspace;
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


        #region WORKSPACE - DONE
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

                //If validate success
                if (result.IsSuccess)
                {
                    //Connect requester
                    await Groups.AddToGroupAsync(Context.ConnectionId, workspaceId.ToString());
                }
                else
                {
                    throw new HubException(result.ErrorList.ToString());
                }


            }
            catch (Exception)
            {
                throw;
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

                if (result.IsSuccess)
                {
                    //Remove requester
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, workspaceId.ToString());
                }
                else
                {
                    throw new HubException(result.ErrorList.ToString());
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region LIST - DONE
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
                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveListCreated", result.Message);
                }
                else
                {
                    throw new HubException("Fail to create new List");
                }

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
                command.WorkspaceId = workspaceId;
                command.ListId = listId;

                //Send command
                var result = await _mediator.Send(command);
                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveListMoved", command.ListId, command.NewPosition);
                }
                else
                {
                    throw new HubException("Fail to move List");
                }
            }
            catch (Exception)
            {
                throw;
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
                command.WorkspaceId = workspaceId;
                command.ListId = listId;

                //Send command
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveListRenamed", command.ListId, command.NewTitle);
                }
                else
                {
                    throw new HubException(result.ErrorList.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region CARD - DONE

        //Create Card and assign member to card
        public async Task CreateCardAndAssignMember(int workspaceId, int listId, CreateCardCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkspaceId = workspaceId;
                command.ListId = listId;

                //Send command
                var result = await _mediator.Send(command);
                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardCreated", listId, result.Message);
                }
                else
                {
                    throw new HubException("Fail to create new card");
                }
            }
            catch (Exception)
            {
                throw new HubException("Fail to create new card");
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
                command.WorkspaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);
                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardMoved", command.CardId, command.NewListId, command.NewPosition);
                }
                else
                {
                    throw new HubException("Fail to move card");
                }
            }
            catch (Exception)
            {
                throw new HubException("Fail to move card");
            }
        }

        //Update Card Details
        public async Task UpdateCardDetails(int workspaceId, int listId, int cardId, UpdateCardDetailsCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkspaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardUpdated", command.CardId, result.Message);
                }
                else
                {
                    throw new HubException("Fail to update card");
                }
            }
            catch (Exception)
            {
                throw new HubException("Fail to update card");
            }
        }

        //Delete Card 
        public async Task DeleteCard(int workspaceId, int listId, int cardId, DeleteCardCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkspaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardDeleted", command.ListId, command.CardId);
                }
                else
                {
                    throw new HubException("Fail to delete card");
                }
            }
            catch (Exception)
            {
                throw new HubException("Fail to delete card");
            }
        }

        //Assign member to card
        public async Task AssignMembersToCard(int workspaceId, int listId, int cardId, AssignMembersToCardCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkspaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);
                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardAssigned", command.ListId, command.CardId, result.Message);
                }
                else
                {
                    throw new HubException("Fail to assign member to card");
                }
            }
            catch (Exception)
            {
                throw new HubException("Fail to assign members to card");
            }
        }

        //Assign member to card
        public async Task UnAssignMembersToCard(int workspaceId, int listId, int cardId, UnAssignMembersToCardCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkspaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveCardUnAssigned", command.ListId, command.CardId, command.StudentId);
                }
                else
                {
                    throw new HubException("Fail to unassign member of card");
                }
            }
            catch (Exception)
            {
                throw new HubException("Fail to unassign members of card");
            }
        }
        #endregion

        #region Task - DONE
        //Create new Task
        public async Task CreateTask(int workspaceId, int listId, int cardId, CreateTaskCommand command)
        {
            try
            {
                //Get Requester Info
                var userId = GetUserId();

                //Bind to command
                command.RequesterId = userId;
                command.WorkspaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;

                //Send command
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveTaskCreated", command.ListId, command.CardId, result.Message);
                }
                else
                {
                    throw new HubException("Fail to create new task");
                }
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
                command.WorkspaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;
                command.TaskId = taskId;

                //Send command
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveTaskRenamed", command.ListId, command.CardId, command.TaskId, command.NewTitle);
                }
                else
                {
                    throw new HubException("Fail to rename the task");
                }

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
                command.WorkspaceId = workspaceId;
                command.ListId = listId;
                command.CardId = cardId;
                command.TaskId = taskId;

                //Send command
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    //Broadcase to other 
                    await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveTaskDeleted", command.ListId, command.CardId, command.TaskId);
                }
                else
                {
                    throw new HubException("Fail to delete the task");
                }
            }
            catch (Exception)
            {
                throw new HubException("Fail to delete the task");
            }
        }
        #endregion

        #region Sub-Task
        //Create new Sub-Task
        public async Task CreateSubTask(int workspaceId, int listId, int cardId, CreateSubTaskCommand command)
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
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveSubTaskCreated", result.CreatedSubTaskDto);
            }
            catch (Exception)
            {
                throw new HubException("Fail to create new subtask");
            }
        }

        //Rename SubTask
        public async Task RenameSubTask(int workspaceId, int listId, int cardId, int taskId, int subtaskId, RenameSubTaskCommand command)
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
                command.SubTaskId = subtaskId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveSubTaskRenamed", command.ListId, command.CardId, command.TaskId, command.SubTaskId, command.NewTitle);
            }
            catch (Exception)
            {
                throw new HubException("Fail to rename subtask");
            }
        }

        //Update SubTask
        public async Task DeleteSubTask(int workspaceId, int listId, int cardId, int taskId, int subtaskId, DeleteSubTaskCommand command)
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
                command.SubTaskId = subtaskId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveSubTaskDeleted", command.ListId, command.CardId, command.TaskId, command.SubTaskId);
            }
            catch (Exception)
            {
                throw new HubException("Fail to delete subtask");
            }
        }

        //Update SubTask
        public async Task UpdateSubTaskDetails(int workspaceId, int listId, int cardId, int taskId, int subtaskId, UpdateSubTaskDetailsCommand command)
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
                command.SubTaskId = subtaskId;

                //Send command
                var result = await _mediator.Send(command);

                //Broadcase to other 
                await Clients.OthersInGroup(workspaceId.ToString()).SendAsync("ReceiveSubTaskUpdated", result.UpdatedSubTaskDto);
            }
            catch (Exception)
            {
                throw new HubException("Fail to update subtask");
            }
        }
        #endregion

    }
}
