namespace CinemaBooking.Application.Mappings;

using AutoMapper;
using CinemaBooking.Application.DTOs;
using CinemaBooking.Domain.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Movie, MovieDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.PosterUrl, opt => opt.MapFrom(s => s.Poster != null ? $"/posters/{s.Poster.FileName}" : null));
            
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Cinema, CinemaDto>().ReverseMap();
        CreateMap<Hall, HallDto>()
            .ForMember(d => d.CinemaName, opt => opt.MapFrom(s => s.Cinema.Name))
            .ReverseMap();
        CreateMap<ShowTime, ShowTimeDto>()
            .ForMember(d => d.MovieTitle, opt => opt.MapFrom(s => s.Movie.Title))
            .ForMember(d => d.HallName, opt => opt.MapFrom(s => s.Hall.Name))
            .ReverseMap();
        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.UserId))
            .ForMember(d => d.MovieTitle, opt => opt.MapFrom(s => s.ShowTime.Movie.Title))
            .ForMember(d => d.CinemaName, opt => opt.MapFrom(s => s.ShowTime.Hall.Cinema.Name))
            .ForMember(d => d.ShowTimeStart, opt => opt.MapFrom(s => s.ShowTime.StartTime))
            .ForMember(d => d.SeatCount, opt => opt.MapFrom(s => s.Items.Count));
    }
}
