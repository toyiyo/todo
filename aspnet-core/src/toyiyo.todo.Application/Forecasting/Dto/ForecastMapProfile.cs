using AutoMapper;
using toyiyo.todo.Forecasting.Dto;  
using toyiyo.todo.Forecasting; 

namespace toyiyo.todo.Forecasting.Dto
{
    public class ForecastMapProfile : Profile
    {
        public ForecastMapProfile()
        {
            CreateMap<ForecastResult, ForecastResultDto>();
            CreateMap<ProgressPoint, ProgressPointDto>();
        }
    }
}
