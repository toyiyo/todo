using System.Linq;
using AutoMapper;
using Abp.Authorization;
using toyiyo.todo.Projects;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs.Dto
{
    public class JobMapProfile : Profile
    {
        public JobMapProfile()
        {
            //map from dto to entity
            CreateMap<JobCreateInputDto, Job>();
            CreateMap<Job, JobDto>().ReverseMap();
        }
    }
}
