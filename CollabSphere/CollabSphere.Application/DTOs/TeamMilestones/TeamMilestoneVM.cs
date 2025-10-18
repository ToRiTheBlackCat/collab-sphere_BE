﻿using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.DTOs.MilestoneQuestions;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamMilestones
{
    public class TeamMilestoneVM
    {
        public int TeamMilestoneId { get; set; }

        public int ObjectiveMilestoneId { get; set; }

        public int TeamId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        
        public DateOnly? StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public float? Progress { get; set; }

        public int Status { get; set; }

        public string StatusString => ((TeamMilestoneStatuses)this.Status).ToString();

        public int CheckpointCount { get; set; }

        public int MilestoneQuestionCount { get; set; }
    }
}
