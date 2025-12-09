using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.ChatMessages;
using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.DTOs.MilestoneEvaluations;
using CollabSphere.Application.DTOs.Notifications;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.Mappings.TeamMilestones;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.API.Hubs
{
    public interface ITeamBoardClient
    {
        Task ReceiveMilestoneCreated(TeamMilestoneVM teamMilestone);

        Task ReceiveMilestoneUpdated(TeamMilestoneVM teamMilestone);

        Task ReceiveMilestoneDeleted(int teamMilestoneId);

        Task ReceiveMilestoneCheckedDone(TeamMilestoneVM teamMilestone);

        Task ReceiveMilestoneEvaluated(TeamMilestoneEvaluationVM teamMilestone);

        // Checkpoints
        Task ReceiveCheckpointCreate(CheckpointVM checkpoint);

        Task ReceiveCheckpointUpdate(CheckpointVM checkpoint);

        Task ReceiveCheckpointDelete(int checkpointId);

        // Notifications
        Task ReceiveNotification(NotificationDto notification);

        Task ReceiveNotificationHistory(List<NotificationDto> notification);
    }

    public interface ITeamBoardHub
    {
        Task JoinServer();

        Task BroadcastMilestoneCreated(int teamId, int teamMilestoneId, string linkForTeamMeber);

        Task BroadcastMilestoneUpdated(int teamId, int teamMilestoneId, string linkForTeamMeber);

        Task BroadcastMilestoneDeleted(int teamId, int teamMilestoneId, string linkForTeamMeber);

        Task BroadcastMilestoneCheckedDone(int teamId, int teamMilestoneId, string linkForTeamMember, string linkForLecturer);

        Task BroadcastMilestoneEvaluated(int teamId, int teamMilestoneId, string linkForTeamMember);
    }

    public static class HubExentions
    {
        public struct JwtUserInfo
        {
            public int UserId { get; set; }

            public int RoleId { get; set; }
        }

        public static JwtUserInfo GetCurrentUserInfo(this TeamBoardHub hub)
        {
            if (hub.Context.User == null)
            {
                throw new HubException("Hub user is not logged in.");
            }

            var UIdClaim = hub.Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var roleClaim = hub.Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            JwtUserInfo userInfo = new JwtUserInfo()
            {
                UserId = int.TryParse(UIdClaim?.Value, out int userId) ? userId : -1,
                RoleId = int.TryParse(roleClaim?.Value, out int roleId) ? roleId : -1,
            };

            return userInfo;
        }
    }

    public class TeamWorkConnectionInfo
    {
        public required string ConnectionId { get; set; }

        public required int UserId { get; set; }

        public required int UserRole { get; set; }

        public required List<int> ConnectedTeamIds { get; set; }
    }

    [Authorize(Roles = "4, 5")]
    public class TeamBoardHub : Hub<ITeamBoardClient>, ITeamBoardHub
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Details of connectionIds
        /// Dictionary<ConnectionId, ConnectionInfo>
        /// </summary
        private readonly ConcurrentDictionary<string, TeamWorkConnectionInfo> _mapping;

        public TeamBoardHub(IUnitOfWork unitOfWork, ConnectionMappings connectionMappings)
        {
            _unitOfWork = unitOfWork;
            _mapping = connectionMappings.TeamBoardMapping;
        }

        public async Task JoinServer()
        {
            var userInfo = this.GetCurrentUserInfo();

            var userTeams = await _unitOfWork.TeamRepo.GetTeamsByUser(userInfo.UserId);
            foreach (var team in userTeams)
            {
                var newConnectionInfo = new TeamWorkConnectionInfo()
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userInfo.UserId,
                    UserRole = userInfo.RoleId,
                    ConnectedTeamIds = new List<int>()
                };

                var connectInfo = _mapping.GetOrAdd(Context.ConnectionId, newConnectionInfo);
                connectInfo.ConnectedTeamIds.Add(team.TeamId);

                var groupName = $"{team.TeamId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }

            var notifications = await _unitOfWork.NotificationRepo.GetChatNotificationsOfUser(userInfo.UserId);
            await Clients.Caller.ReceiveNotificationHistory(notifications.ToNotificationDtos(userInfo.UserId));
        }

        [Authorize(Roles = "4")]
        public async Task BroadcastMilestoneCreated(int teamId, int teamMilestoneId, string linkForTeamMeber)
        {
            var userInfo = this.GetCurrentUserInfo();
            if (!_mapping.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                throw new HubException("SignalR failed to get info of connected user");
            }
            else if (!connectionInfo.ConnectedTeamIds.Contains(teamId))
            {
                throw new HubException($"You are not allowed to access Team with ID '{teamId}'.");
            }

            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(teamMilestoneId);
            if (teamMilestone == null)
            {
                throw new HubException($"No Team Milestone with ID '{teamMilestoneId}' found.");
            }

            var team = await _unitOfWork.TeamRepo.GetTeamDetail(teamId);
            if (teamMilestone.TeamId != team!.TeamId)
            {
                throw new HubException($"Team Milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) is not of team '{team.TeamName}'({team.TeamId}).");
            }

            var groupName = $"{team.TeamId}";

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Create new notification
                var notification = new Notification()
                {
                    Link = linkForTeamMeber,
                    CreatedAt = DateTime.UtcNow,
                    Content = $"New Team Milestone '{teamMilestone.Title}' has been created.",
                    Title = $"Team '{team!.TeamName}' - Milestone created.",
                    ReferenceType = NotificationTypes.MILESTONE.ToString(),
                };
                await _unitOfWork.NotificationRepo.Create(notification);
                await _unitOfWork.SaveChangesAsync();

                // Create recipient entries for team members
                var teamMembers = team.ClassMembers.Where(x => x.Status == (int)ClassMemberStatus.VALID);
                foreach (var teamMember in teamMembers)
                {
                    var recipient = new NotificationRecipient()
                    {
                        NotificationId = notification.NotificationId,
                        IsRead = false,
                        ReadAt = null,
                        ReceiverId = teamMember.StudentId,
                    };
                    await _unitOfWork.NotificationRecipientRepo.Create(recipient);
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Broadcast about newly created milestone
                var _ = Clients.OthersInGroup(groupName).ReceiveMilestoneCreated(teamMilestone.ToTeamMilestoneVM());

                // Notification Dto
                var notiDto = notification.ToNotificationDto();
                notiDto.IsRead = false;
                notiDto.ReadAt = null;

                var validConnections = _mapping.Where(x => x.Value.ConnectedTeamIds.Contains(team.TeamId));
                if (validConnections.Any())
                {
                    var _noti = Clients.Clients(validConnections.Select(x => x.Value.ConnectionId))
                        .ReceiveNotification(notiDto);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new HubException($"Failed to broadcast new Team Milestone. {ex.Message}");
            }
        }

        [Authorize(Roles = "4")]
        public async Task BroadcastMilestoneUpdated(int teamId, int teamMilestoneId, string linkForTeamMember)
        {
            var userInfo = this.GetCurrentUserInfo();
            if (!_mapping.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                throw new HubException("SignalR failed to get info of connected user");
            }
            else if (!connectionInfo.ConnectedTeamIds.Contains(teamId))
            {
                throw new HubException($"You are not allowed to access Team with ID '{teamId}'.");
            }

            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(teamMilestoneId);
            if (teamMilestone == null)
            {
                throw new HubException($"No Team Milestone with ID '{teamMilestoneId}' found.");
            }

            var team = await _unitOfWork.TeamRepo.GetTeamDetail(teamId);
            if (teamMilestone.TeamId != team!.TeamId)
            {
                throw new HubException($"Team Milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) is not of team '{team.TeamName}'({team.TeamId}).");
            }

            var groupName = $"{team.TeamId}";

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Create new notification
                var notification = new Notification()
                {
                    Link = linkForTeamMember,
                    CreatedAt = DateTime.UtcNow,
                    Content = $"Team Milestone '{teamMilestone.Title}' has been updated.",
                    Title = $"Team '{team!.TeamName}' - Milestone updated.",
                    ReferenceType = NotificationTypes.MILESTONE.ToString(),
                };
                await _unitOfWork.NotificationRepo.Create(notification);
                await _unitOfWork.SaveChangesAsync();

                // Create recipient entries for team members
                var teamMembers = team.ClassMembers.Where(x => x.Status == (int)ClassMemberStatus.VALID);
                foreach (var teamMember in teamMembers)
                {
                    var recipient = new NotificationRecipient()
                    {
                        NotificationId = notification.NotificationId,
                        IsRead = false,
                        ReadAt = null,
                        ReceiverId = teamMember.StudentId,
                    };
                    await _unitOfWork.NotificationRecipientRepo.Create(recipient);
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Broadcast about updated milestone
                var _ = Clients.OthersInGroup(groupName).ReceiveMilestoneUpdated(teamMilestone.ToTeamMilestoneVM());

                // Notification Dto
                var notiDto = notification.ToNotificationDto();
                notiDto.IsRead = false;
                notiDto.ReadAt = null;

                var validConnections = _mapping.Where(x => x.Value.ConnectedTeamIds.Contains(team.TeamId));
                if (validConnections.Any())
                {
                    var _noti = Clients.Clients(validConnections.Select(x => x.Value.ConnectionId))
                        .ReceiveNotification(notiDto);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new HubException($"Failed to broadcast update Team Milestone. {ex.Message}");
            }
        }

        [Authorize(Roles = "4")]
        public async Task BroadcastMilestoneDeleted(int teamId, int teamMilestoneId, string linkForTeamMember)
        {
            var userInfo = this.GetCurrentUserInfo();
            if (!_mapping.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                throw new HubException("SignalR failed to get info of connected user");
            }
            else if (!connectionInfo.ConnectedTeamIds.Contains(teamId))
            {
                throw new HubException($"You are not allowed to access Team with ID '{teamId}'.");
            }

            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(teamMilestoneId);
            if (teamMilestone != null)
            {
                throw new HubException($"Team Milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) has not been deleteted.");
            }

            var team = await _unitOfWork.TeamRepo.GetTeamDetail(teamId);
            var groupName = $"{team!.TeamId}";

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Create new notification
                var notification = new Notification()
                {
                    Link = linkForTeamMember,
                    CreatedAt = DateTime.UtcNow,
                    Content = $"A Team Milestone has been deleted.",
                    Title = $"Team '{team!.TeamName}' - Milestone deleted.",
                    ReferenceType = NotificationTypes.MILESTONE.ToString(),
                };
                await _unitOfWork.NotificationRepo.Create(notification);
                await _unitOfWork.SaveChangesAsync();

                // Create recipient entries for team members
                var teamMembers = team.ClassMembers.Where(x => x.Status == (int)ClassMemberStatus.VALID);
                foreach (var teamMember in teamMembers)
                {
                    var recipient = new NotificationRecipient()
                    {
                        NotificationId = notification.NotificationId,
                        IsRead = false,
                        ReadAt = null,
                        ReceiverId = teamMember.StudentId,
                    };
                    await _unitOfWork.NotificationRecipientRepo.Create(recipient);
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Broadcast about deleted milestone
                var _ = Clients.OthersInGroup(groupName).ReceiveMilestoneDeleted(teamMilestoneId);

                // Notification Dto
                var notiDto = notification.ToNotificationDto();
                notiDto.IsRead = false;
                notiDto.ReadAt = null;

                var validConnections = _mapping.Where(x => x.Value.ConnectedTeamIds.Contains(team.TeamId));
                if (validConnections.Any())
                {
                    var _noti = Clients.Clients(validConnections.Select(x => x.Value.ConnectionId))
                        .ReceiveNotification(notiDto);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new HubException($"Failed to broadcast delete Team Milestone. {ex.Message}");
            }
        }

        [Authorize(Roles = "5")]
        public async Task BroadcastMilestoneCheckedDone(int teamId, int teamMilestoneId, string linkForTeamMember, string linkForLecturer)
        {
            var userInfo = this.GetCurrentUserInfo();
            if (!_mapping.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                throw new HubException("SignalR failed to get info of connected user");
            }
            else if (!connectionInfo.ConnectedTeamIds.Contains(teamId))
            {
                throw new HubException($"You are not allowed to access Team with ID '{teamId}'.");
            }

            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(teamMilestoneId);
            if (teamMilestone == null)
            {
                throw new HubException($"No Team Milestone with ID '{teamMilestoneId}' found.");
            }

            var team = await _unitOfWork.TeamRepo.GetTeamDetail(teamMilestone.TeamId);
            if (teamMilestone.TeamId != team!.TeamId)
            {
                throw new HubException($"Team Milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) is not of team '{team.TeamName}'({team.TeamId}).");
            }

            var groupName = $"{team.TeamId}";

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                var currentTime = DateTime.UtcNow;

                // Create new notification
                var notification = new Notification()
                {
                    Link = linkForTeamMember,
                    CreatedAt = currentTime,
                    Content = $"Team Milestone '{teamMilestone.Title}' has been marked done. ",
                    Title = $"Team '{team!.TeamName}' - Milestone is done.",
                    ReferenceType = NotificationTypes.MILESTONE.ToString(),
                };

                await _unitOfWork.NotificationRepo.Create(notification);
                await _unitOfWork.SaveChangesAsync();

                // Create recipient entries for other team members
                var teamMembers = team.ClassMembers
                    .Where(x =>
                        x.StudentId != userInfo.UserId &&
                        x.Status == (int)ClassMemberStatus.VALID
                    );
                foreach (var teamMember in teamMembers)
                {
                    var recipient = new NotificationRecipient()
                    {
                        NotificationId = notification.NotificationId,
                        IsRead = false,
                        ReadAt = null,
                        ReceiverId = teamMember.StudentId,
                    };
                    await _unitOfWork.NotificationRecipientRepo.Create(recipient);
                }
                await _unitOfWork.SaveChangesAsync();

                // Seperate notification for lecturer because difference in page link
                var lecturerNoti = new Notification()
                {
                    Link = linkForLecturer,
                    CreatedAt = currentTime,
                    Content = $"Team Milestone Done. '{teamMilestone.Title}'.",
                    Title = $"Team '{team.TeamName}' - Milestone is done.",
                    ReferenceType = NotificationTypes.MILESTONE.ToString(),
                };
                await _unitOfWork.NotificationRepo.Create(lecturerNoti);
                await _unitOfWork.SaveChangesAsync();

                var lecturerRecipient = new NotificationRecipient()
                {
                    NotificationId = lecturerNoti.NotificationId,
                    IsRead = false,
                    ReadAt = null,
                    ReceiverId = team.Class.LecturerId!.Value,
                };
                await _unitOfWork.NotificationRecipientRepo.Create(lecturerRecipient);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Broadcast about done milestone
                var _ = Clients.OthersInGroup(groupName).ReceiveMilestoneCheckedDone(teamMilestone.ToTeamMilestoneVM());

                // Boardcast notification to other members in team & lecturer
                var validConnections = _mapping.Where(x =>
                    x.Value.UserId != userInfo.UserId &&
                    x.Value.ConnectedTeamIds.Contains(team.TeamId));
                if (validConnections.Any())
                {
                    // Other team members
                    var notiDto = notification.ToNotificationDto();
                    notiDto.IsRead = false;
                    notiDto.ReadAt = null;

                    var studentConnectionIds = validConnections
                        .Where(x => x.Value.UserRole == RoleConstants.STUDENT)
                        .Select(x => x.Value.ConnectionId);
                    var _noti = Clients.Clients(studentConnectionIds).ReceiveNotification(notiDto);

                    // Lecturer
                    var lecNotiDto = lecturerNoti.ToNotificationDto();
                    lecNotiDto.IsRead = false;
                    lecNotiDto.ReadAt = null;

                    var lecConnectionIds = validConnections
                        .Where(x => x.Value.UserRole == RoleConstants.LECTURER)
                        .Select(x => x.Value.ConnectionId);
                    var _lectNoti = Clients.Clients(lecConnectionIds).ReceiveNotification(lecNotiDto);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new HubException($"Failed to broadcast check done Team Milestone. {ex.Message}");
            }
        }

        [Authorize(Roles = "4")]
        public async Task BroadcastMilestoneEvaluated(int teamId, int teamMilestoneId, string linkForTeamMember)
        {
            var userInfo = this.GetCurrentUserInfo();
            if (!_mapping.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                throw new HubException("SignalR failed to get info of connected user");
            }
            else if (!connectionInfo.ConnectedTeamIds.Contains(teamId))
            {
                throw new HubException($"You are not allowed to access Team with ID '{teamId}'.");
            }

            var teamMilestone = await _unitOfWork.TeamMilestoneRepo.GetDetailById(teamMilestoneId);
            if (teamMilestone == null)
            {
                throw new HubException($"No Team Milestone with ID '{teamMilestoneId}' found.");
            }
            else if (teamMilestone.MilestoneEvaluation == null)
            {
                throw new HubException($"No Milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) has not been graded yet.");
            }

            var team = await _unitOfWork.TeamRepo.GetTeamDetail(teamId);
            if (teamMilestone.TeamId != team!.TeamId)
            {
                throw new HubException($"Team Milestone '{teamMilestone.Title}'({teamMilestone.TeamMilestoneId}) is not of team '{team.TeamName}'({team.TeamId}).");
            }

            var groupName = $"{team!.TeamId}";

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                // Create new notification
                var notification = new Notification()
                {
                    Link = linkForTeamMember,
                    CreatedAt = DateTime.UtcNow,
                    Content = $"Team Milestone '{teamMilestone.Title}' has been evaluated.",
                    Title = $"Team '{team!.TeamName}' - Milestone has been evaluated.",
                    ReferenceType = NotificationTypes.MILESTONE.ToString(),
                };
                await _unitOfWork.NotificationRepo.Create(notification);
                await _unitOfWork.SaveChangesAsync();

                // Create recipient entries for team members
                var teamMembers = team.ClassMembers.Where(x => x.Status == (int)ClassMemberStatus.VALID);
                foreach (var teamMember in teamMembers)
                {
                    var recipient = new NotificationRecipient()
                    {
                        NotificationId = notification.NotificationId,
                        IsRead = false,
                        ReadAt = null,
                        ReceiverId = teamMember.StudentId,
                    };
                    await _unitOfWork.NotificationRecipientRepo.Create(recipient);
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                // Broadcast about evaluated milestone
                var evaluateVM = teamMilestone.ToDetailDto().MilestoneEvaluation;
                var _ = Clients.OthersInGroup(groupName).ReceiveMilestoneEvaluated(evaluateVM);

                // Notification Dto
                var notiDto = notification.ToNotificationDto();
                notiDto.IsRead = false;
                notiDto.ReadAt = null;

                var validConnections = _mapping.Where(x => x.Value.ConnectedTeamIds.Contains(team.TeamId));
                if (validConnections.Any())
                {
                    var _noti = Clients.Clients(validConnections.Select(x => x.Value.ConnectionId))
                        .ReceiveNotification(notiDto);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new HubException($"Failed to broadcast evaluate Team Milestone. {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_mapping.Remove(Context.ConnectionId, out var removedInfo))
            {
                // Remove connection Id from groups
                foreach (var teamId in removedInfo.ConnectedTeamIds)
                {
                    var groupName = $"{teamId}";
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                }

                Console.WriteLine($"REMOVE USER_ID '{removedInfo.UserId}', CONNECTION_ID: {removedInfo.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
