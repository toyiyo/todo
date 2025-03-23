using AutoMapper;

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
