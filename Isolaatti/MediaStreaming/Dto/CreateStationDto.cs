using System.ComponentModel.DataAnnotations;

namespace Isolaatti.MediaStreaming.Dto;

public record CreateStationDto(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    string Name, 
    string? Description );

public record CreateStationResultDto(StationDto Station);